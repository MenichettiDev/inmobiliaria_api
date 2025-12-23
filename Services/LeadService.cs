using inmobiliariaApi.DTOs.Common;
using inmobiliariaApi.DTOs.Lead;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;
using inmobiliariaApi.Services;
using inmobiliariaApi.Exceptions;
using inmobiliariaApi.Data;
using Microsoft.EntityFrameworkCore;

namespace inmobiliariaApi.Services
{
    public class LeadService : GenericService<Lead>
    {
        private readonly LeadRepository _leadRepository;
        private readonly ITenantContext _tenantContext;
        private readonly IUsoMensualService _usoMensualService;
        private readonly ApplicationDbContext _context;

        public LeadService(
            LeadRepository leadRepository,
            ITenantContext tenantContext,
            IUsoMensualService usoMensualService,
            ApplicationDbContext context)
            : base(leadRepository)
        {
            _leadRepository = leadRepository;
            _tenantContext = tenantContext;
            _usoMensualService = usoMensualService;
            _context = context;
        }

        public async Task<BaseResponseDto<LeadResponseDto>> CrearLeadDesdeFormularioAsync(CreateLeadDto createLeadDto, int usuarioId)
        {
            try
            {
                // Validar límites del plan antes de crear
                await _usoMensualService.ValidarLimiteLeadsAsync();

                // Validar que la propiedad (si existe) pertenece al tenant
                if (createLeadDto.IdPropiedad.HasValue)
                {
                    var propiedadValida = await _leadRepository.ValidatePropertiesBelongsToTenantAsync(createLeadDto.IdPropiedad);
                    if (!propiedadValida)
                    {
                        return new BaseResponseDto<LeadResponseDto>
                        {
                            Success = false,
                            Message = "La propiedad especificada no existe o no pertenece a su inmobiliaria",
                            Errors = new List<string> { "ID de propiedad inválido" }
                        };
                    }
                }

                // Validar que la fuente de contacto existe
                var fuenteExists = await _context.FuenteContacto
                    .AnyAsync(f => f.Id == createLeadDto.IdFuente);
                if (!fuenteExists)
                {
                    return new BaseResponseDto<LeadResponseDto>
                    {
                        Success = false,
                        Message = "La fuente de contacto especificada no es válida",
                        Errors = new List<string> { "ID de fuente de contacto inválido" }
                    };
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Crear el lead
                    var lead = new Lead
                    {
                        NombreCompleto = createLeadDto.NombreCompleto,
                        Email = createLeadDto.Email,
                        Telefono = createLeadDto.Telefono,
                        Mensaje = createLeadDto.Mensaje,
                        IdPropiedad = createLeadDto.IdPropiedad,
                        IdFuente = createLeadDto.IdFuente,
                        IdEstado = 1, // Nuevo por defecto
                        CreadoEn = DateTime.UtcNow,
                        ActualizadoEn = DateTime.UtcNow
                    };

                    var leadCreado = await _leadRepository.AddAsync(lead);

                    // Crear historial inicial
                    var historial = new LeadEstadoHistorial
                    {
                        IdLead = leadCreado.Id,
                        IdEstadoAnterior = 0, // Sin estado anterior
                        IdEstadoNuevo = 1, // Nuevo
                        IdUsuario = usuarioId,
                        Comentario = "Lead creado desde formulario",
                        CreadoEn = DateTime.UtcNow
                    };

                    _context.LeadEstadoHistorial.Add(historial);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    // Obtener el lead completo para la respuesta
                    var leadCompleto = await _leadRepository.GetByIdAsync(leadCreado.Id);
                    var responseDto = MapToResponseDto(leadCompleto!);

                    return new BaseResponseDto<LeadResponseDto>
                    {
                        Success = true,
                        Data = responseDto,
                        Message = "Lead creado exitosamente"
                    };
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (LimiteDeLeadsExcedidoException ex)
            {
                return new BaseResponseDto<LeadResponseDto>
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = new List<string> { $"Límite actual: {ex.LimiteActual}, Usados: {ex.LeadsUsados}" }
                };
            }
            catch (InmobiliariaInactivaException ex)
            {
                return new BaseResponseDto<LeadResponseDto>
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = new List<string> { $"Estado inmobiliaria: {ex.EstadoActual}" }
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<LeadResponseDto>
                {
                    Success = false,
                    Message = "Error al crear el lead",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<LeadResponseDto>> AsignarLeadAUsuarioAsync(int leadId, int usuarioId, int usuarioActualId)
        {
            try
            {
                var lead = await _leadRepository.GetByIdAsync(leadId);
                if (lead == null)
                {
                    return new BaseResponseDto<LeadResponseDto>
                    {
                        Success = false,
                        Message = "Lead no encontrado",
                        Errors = new List<string> { $"No existe lead con ID {leadId}" }
                    };
                }

                // Validar que el usuario pertenece al tenant
                var usuarioValido = await _leadRepository.ValidateUsuarioAsignadoBelongsToTenantAsync(usuarioId);
                if (!usuarioValido)
                {
                    return new BaseResponseDto<LeadResponseDto>
                    {
                        Success = false,
                        Message = "Usuario no válido para asignación",
                        Errors = new List<string> { "El usuario no pertenece a la inmobiliaria o no está activo" }
                    };
                }

                lead.IdUsuarioAsignado = usuarioId;
                lead.ActualizadoEn = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var leadActualizado = await _leadRepository.GetByIdAsync(leadId);
                var responseDto = MapToResponseDto(leadActualizado!);

                return new BaseResponseDto<LeadResponseDto>
                {
                    Success = true,
                    Data = responseDto,
                    Message = "Lead asignado exitosamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<LeadResponseDto>
                {
                    Success = false,
                    Message = "Error al asignar el lead",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<LeadResponseDto>> ActualizarEstadoConHistorialAsync(
            int leadId,
            int nuevoEstadoId,
            int usuarioId,
            string? comentario = null)
        {
            try
            {
                var lead = await _leadRepository.GetByIdAsync(leadId);
                if (lead == null)
                {
                    return new BaseResponseDto<LeadResponseDto>
                    {
                        Success = false,
                        Message = "Lead no encontrado",
                        Errors = new List<string> { $"No existe lead con ID {leadId}" }
                    };
                }

                // Validar que el estado existe (EstadoLead no tiene campo Activo)
                var estadoExists = await _context.EstadoLead
                    .AnyAsync(e => e.Id == nuevoEstadoId);
                if (!estadoExists)
                {
                    return new BaseResponseDto<LeadResponseDto>
                    {
                        Success = false,
                        Message = "Estado no válido",
                        Errors = new List<string> { "El estado especificado no existe" }
                    };
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var estadoAnterior = lead.IdEstado;

                    // Actualizar estado del lead
                    lead.IdEstado = nuevoEstadoId;
                    lead.ActualizadoEn = DateTime.UtcNow;

                    // Crear historial
                    var historial = new LeadEstadoHistorial
                    {
                        IdLead = leadId,
                        IdEstadoAnterior = estadoAnterior,
                        IdEstadoNuevo = nuevoEstadoId,
                        IdUsuario = usuarioId,
                        Comentario = comentario ?? $"Cambio automático de estado",
                        CreadoEn = DateTime.UtcNow
                    };

                    _context.LeadEstadoHistorial.Add(historial);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    var leadActualizado = await _leadRepository.GetByIdAsync(leadId);
                    var responseDto = MapToResponseDto(leadActualizado!);

                    return new BaseResponseDto<LeadResponseDto>
                    {
                        Success = true,
                        Data = responseDto,
                        Message = "Estado del lead actualizado exitosamente"
                    };
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<LeadResponseDto>
                {
                    Success = false,
                    Message = "Error al actualizar el estado del lead",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<PaginatedResponseDto<LeadResponseDto>>> GetLeadsConFiltrosAsync(LeadFiltrosDto filtros)
        {
            try
            {
                var (leads, totalCount) = await _leadRepository.GetLeadsConFiltrosAsync(filtros);

                var leadsDto = leads.Select(MapToResponseDto).ToList();

                var paginatedResponse = new PaginatedResponseDto<LeadResponseDto>
                {
                    Data = leadsDto,
                    TotalRecords = totalCount,
                    Page = filtros.Page,
                    PageSize = filtros.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / filtros.PageSize),
                    HasNextPage = filtros.Page < (int)Math.Ceiling((double)totalCount / filtros.PageSize),
                    HasPreviousPage = filtros.Page > 1
                };

                return new BaseResponseDto<PaginatedResponseDto<LeadResponseDto>>
                {
                    Success = true,
                    Data = paginatedResponse,
                    Message = "Leads obtenidos exitosamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<PaginatedResponseDto<LeadResponseDto>>
                {
                    Success = false,
                    Message = "Error al obtener los leads",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<LeadResponseDto>> ActualizarLeadAsync(int leadId, UpdateLeadDto updateLeadDto, int usuarioId)
        {
            try
            {
                var lead = await _leadRepository.GetByIdAsync(leadId);
                if (lead == null)
                {
                    return new BaseResponseDto<LeadResponseDto>
                    {
                        Success = false,
                        Message = "Lead no encontrado",
                        Errors = new List<string> { $"No existe lead con ID {leadId}" }
                    };
                }

                // Validaciones
                if (updateLeadDto.IdPropiedad.HasValue)
                {
                    var propiedadValida = await _leadRepository.ValidatePropertiesBelongsToTenantAsync(updateLeadDto.IdPropiedad);
                    if (!propiedadValida)
                    {
                        return new BaseResponseDto<LeadResponseDto>
                        {
                            Success = false,
                            Message = "La propiedad especificada no es válida",
                            Errors = new List<string> { "ID de propiedad inválido" }
                        };
                    }
                }

                if (updateLeadDto.IdUsuarioAsignado.HasValue)
                {
                    var usuarioValido = await _leadRepository.ValidateUsuarioAsignadoBelongsToTenantAsync(updateLeadDto.IdUsuarioAsignado);
                    if (!usuarioValido)
                    {
                        return new BaseResponseDto<LeadResponseDto>
                        {
                            Success = false,
                            Message = "El usuario asignado no es válido",
                            Errors = new List<string> { "ID de usuario inválido" }
                        };
                    }
                }

                // Actualizar campos
                lead.NombreCompleto = updateLeadDto.NombreCompleto;
                lead.Email = updateLeadDto.Email;
                lead.Telefono = updateLeadDto.Telefono;
                lead.Mensaje = updateLeadDto.Mensaje;
                lead.IdPropiedad = updateLeadDto.IdPropiedad;
                lead.IdFuente = updateLeadDto.IdFuente;
                lead.IdUsuarioAsignado = updateLeadDto.IdUsuarioAsignado;
                lead.ActualizadoEn = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var leadActualizado = await _leadRepository.GetByIdAsync(leadId);
                var responseDto = MapToResponseDto(leadActualizado!);

                return new BaseResponseDto<LeadResponseDto>
                {
                    Success = true,
                    Data = responseDto,
                    Message = "Lead actualizado exitosamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<LeadResponseDto>
                {
                    Success = false,
                    Message = "Error al actualizar el lead",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<LeadResponseDto>> GetByIdAsync(int id)
        {
            try
            {
                var lead = await _leadRepository.GetByIdAsync(id);
                if (lead == null)
                {
                    return new BaseResponseDto<LeadResponseDto>
                    {
                        Success = false,
                        Message = "Lead no encontrado",
                        Errors = new List<string> { $"No existe lead con ID {id}" }
                    };
                }

                var responseDto = MapToResponseDto(lead);

                return new BaseResponseDto<LeadResponseDto>
                {
                    Success = true,
                    Data = responseDto,
                    Message = "Lead obtenido exitosamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<LeadResponseDto>
                {
                    Success = false,
                    Message = "Error al obtener el lead",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<LeadResponseDto>> EliminarLeadAsync(int leadId, int usuarioId)
        {
            try
            {
                var lead = await _leadRepository.GetByIdAsync(leadId);
                if (lead == null)
                {
                    return new BaseResponseDto<LeadResponseDto>
                    {
                        Success = false,
                        Message = "Lead no encontrado",
                        Errors = new List<string> { $"No existe lead con ID {leadId}" }
                    };
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Actualizar estado admin a eliminado (3)
                    lead.IdEstadoAdmin = 3; // eliminado
                    lead.ActualizadoEn = DateTime.UtcNow;

                    // Crear historial de cambio de estado admin
                    var historial = new LeadEstadoHistorial
                    {
                        IdLead = leadId,
                        IdEstadoAnterior = lead.IdEstado,
                        IdEstadoNuevo = lead.IdEstado, // Mantener el mismo estado de lead
                        IdUsuario = usuarioId,
                        Comentario = "Lead marcado como eliminado por el usuario",
                        CreadoEn = DateTime.UtcNow
                    };

                    _context.LeadEstadoHistorial.Add(historial);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    var responseDto = MapToResponseDto(lead);

                    return new BaseResponseDto<LeadResponseDto>
                    {
                        Success = true,
                        Data = responseDto,
                        Message = "Lead eliminado exitosamente"
                    };
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<LeadResponseDto>
                {
                    Success = false,
                    Message = "Error al eliminar el lead",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private static LeadResponseDto MapToResponseDto(Lead lead)
        {
            return new LeadResponseDto
            {
                Id = lead.Id,
                NombreCompleto = lead.NombreCompleto,
                Email = lead.Email,
                Telefono = lead.Telefono,
                Mensaje = lead.Mensaje,
                IdPropiedad = lead.IdPropiedad,
                PropiedadTitulo = lead.Propiedad?.Titulo,
                PropiedadDireccion = lead.Propiedad?.Direccion,
                IdEstado = lead.IdEstado,
                EstadoNombre = lead.Estado?.Nombre ?? "Desconocido",
                IdFuente = lead.IdFuente,
                FuenteNombre = lead.Fuente?.Nombre ?? "Desconocido",
                IdUsuarioAsignado = lead.IdUsuarioAsignado,
                UsuarioAsignadoNombre = lead.UsuarioAsignado != null
                    ? $"{lead.UsuarioAsignado.Nombre}".Trim()
                    : null,
                UsuarioAsignadoEmail = lead.UsuarioAsignado?.Email,
                IdEstadoAdmin = lead.IdEstadoAdmin,
                EstadoAdminDescripcion = lead.EstadoAdmin?.Descripcion ?? "Activo",
                CreadoEn = lead.CreadoEn,
                ActualizadoEn = lead.ActualizadoEn
            };
        }
    }
}