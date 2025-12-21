using pyreApi.DTOs.Common;
using pyreApi.DTOs.Lead;
using pyreApi.Models;
using pyreApi.Repositories;
using pyreApi.Services;
using pyreApi.Exceptions;
using pyreApi.Data;
using Microsoft.EntityFrameworkCore;

namespace pyreApi.Services
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
                    .AnyAsync(f => f.Id == createLeadDto.IdFuenteContacto && f.Activo);
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
                        DireccionInteres = createLeadDto.DireccionInteres,
                        PresupuestoMinimo = createLeadDto.PresupuestoMinimo,
                        PresupuestoMaximo = createLeadDto.PresupuestoMaximo,
                        TipoOperacionInteres = createLeadDto.TipoOperacionInteres,
                        IdPropiedad = createLeadDto.IdPropiedad,
                        IdFuenteContacto = createLeadDto.IdFuenteContacto,
                        Notas = createLeadDto.Notas,
                        Puntuacion = createLeadDto.Puntuacion,
                        IdUsuarioCrea = usuarioId,
                        IdEstado = 1, // Nuevo por defecto
                        FechaContacto = DateTime.UtcNow
                    };

                    var leadCreado = await _leadRepository.CreateAsync(lead);

                    // Crear historial inicial
                    var historial = new HistorialEstadoLead
                    {
                        IdLead = leadCreado.Id,
                        IdEstadoAnterior = 0, // Sin estado anterior
                        IdEstadoNuevo = 1, // Nuevo
                        IdUsuario = usuarioId,
                        Comentario = "Lead creado desde formulario",
                        FechaCambio = DateTime.UtcNow
                    };

                    _context.HistorialEstadoLead.Add(historial);
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
                lead.FechaUltimaInteraccion = DateTime.UtcNow;
                lead.FechaModificacion = DateTime.UtcNow;
                lead.IdUsuarioModifica = usuarioActualId;

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

                // Validar que el estado existe
                var estadoExists = await _context.EstadoLead
                    .AnyAsync(e => e.Id == nuevoEstadoId && e.Activo);
                if (!estadoExists)
                {
                    return new BaseResponseDto<LeadResponseDto>
                    {
                        Success = false,
                        Message = "Estado no válido",
                        Errors = new List<string> { "El estado especificado no existe o no está activo" }
                    };
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var estadoAnterior = lead.IdEstado;

                    // Actualizar estado del lead
                    lead.IdEstado = nuevoEstadoId;
                    lead.FechaUltimaInteraccion = DateTime.UtcNow;
                    lead.FechaModificacion = DateTime.UtcNow;
                    lead.IdUsuarioModifica = usuarioId;

                    // Crear historial
                    var historial = new HistorialEstadoLead
                    {
                        IdLead = leadId,
                        IdEstadoAnterior = estadoAnterior,
                        IdEstadoNuevo = nuevoEstadoId,
                        IdUsuario = usuarioId,
                        Comentario = comentario ?? $"Cambio automático de estado",
                        FechaCambio = DateTime.UtcNow
                    };

                    _context.HistorialEstadoLead.Add(historial);
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
                    Items = leadsDto,
                    TotalCount = totalCount,
                    Page = filtros.Page,
                    PageSize = filtros.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / filtros.PageSize)
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
                lead.DireccionInteres = updateLeadDto.DireccionInteres;
                lead.PresupuestoMinimo = updateLeadDto.PresupuestoMinimo;
                lead.PresupuestoMaximo = updateLeadDto.PresupuestoMaximo;
                lead.TipoOperacionInteres = updateLeadDto.TipoOperacionInteres;
                lead.IdPropiedad = updateLeadDto.IdPropiedad;
                lead.IdFuenteContacto = updateLeadDto.IdFuenteContacto;
                lead.Notas = updateLeadDto.Notas;
                lead.Puntuacion = updateLeadDto.Puntuacion;
                lead.IdUsuarioAsignado = updateLeadDto.IdUsuarioAsignado;
                lead.FechaModificacion = DateTime.UtcNow;
                lead.IdUsuarioModifica = usuarioId;

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

        private static LeadResponseDto MapToResponseDto(Lead lead)
        {
            return new LeadResponseDto
            {
                Id = lead.Id,
                NombreCompleto = lead.NombreCompleto,
                Email = lead.Email,
                Telefono = lead.Telefono,
                Mensaje = lead.Mensaje,
                DireccionInteres = lead.DireccionInteres,
                PresupuestoMinimo = lead.PresupuestoMinimo,
                PresupuestoMaximo = lead.PresupuestoMaximo,
                TipoOperacionInteres = lead.TipoOperacionInteres,
                FechaContacto = lead.FechaContacto,
                FechaUltimaInteraccion = lead.FechaUltimaInteraccion,
                Notas = lead.Notas,
                Puntuacion = lead.Puntuacion,
                IdPropiedad = lead.IdPropiedad,
                PropiedadTitulo = lead.Propiedad?.Titulo,
                PropiedadDireccion = lead.Propiedad?.Direccion,
                IdEstado = lead.IdEstado,
                EstadoNombre = lead.Estado?.Nombre ?? "Desconocido",
                EstadoColor = lead.Estado?.ColorHex,
                IdFuenteContacto = lead.IdFuenteContacto,
                FuenteNombre = lead.FuenteContacto?.Nombre ?? "Desconocido",
                IdUsuarioAsignado = lead.IdUsuarioAsignado,
                UsuarioAsignadoNombre = lead.UsuarioAsignado != null
                    ? $"{lead.UsuarioAsignado.Nombre} {lead.UsuarioAsignado.Apellido}".Trim()
                    : null,
                UsuarioAsignadoEmail = lead.UsuarioAsignado?.Email,
                FechaCreacion = lead.FechaCreacion,
                FechaModificacion = lead.FechaModificacion,
                UsuarioCreaNombre = lead.UsuarioCrea != null
                    ? $"{lead.UsuarioCrea.Nombre} {lead.UsuarioCrea.Apellido}".Trim()
                    : null,
                UsuarioModificaNombre = lead.UsuarioModifica != null
                    ? $"{lead.UsuarioModifica.Nombre} {lead.UsuarioModifica.Apellido}".Trim()
                    : null
            };
        }
    }
}