using inmobiliariaApi.DTOs.Common;
using inmobiliariaApi.DTOs.Lead;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;
using inmobiliariaApi.Services;
using inmobiliariaApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace inmobiliariaApi.Services
{
    public class LeadService : GenericService<Lead>
    {
        private readonly LeadRepository _leadRepository;
        private readonly UsoMensualService _usoMensualService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LeadService> _logger;

        public LeadService(
            LeadRepository leadRepository,
            UsoMensualService usoMensualService,
            ApplicationDbContext context,
            ILogger<LeadService> logger)
            : base(leadRepository)
        {
            _leadRepository = leadRepository;
            _usoMensualService = usoMensualService;
            _context = context;
            _logger = logger;
        }

        private LeadDto MapToResponseDto(Lead lead)
        {
            return new LeadDto
            {
                Id = lead.Id,
                IdPropiedad = lead.IdPropiedad,
                IdInmobiliaria = lead.IdInmobiliaria,
                IdUsuarioAsignado = lead.IdUsuarioAsignado,
                NombreCompleto = lead.NombreCompleto,
                Email = lead.Email,
                Telefono = lead.Telefono,
                Mensaje = lead.Mensaje,
                IdFuente = lead.IdFuente,
                IdEstado = lead.IdEstado,
                Activo = lead.Activo,
                CreadoEn = lead.CreadoEn,
                ActualizadoEn = lead.ActualizadoEn,
                PropiedadTitulo = lead.Propiedad?.Titulo,
                InmobiliariaNombre = lead.Inmobiliaria?.Nombre,
                UsuarioAsignadoNombre = lead.UsuarioAsignado?.Nombre,
                FuenteNombre = lead.Fuente?.Nombre,
                EstadoNombre = lead.Estado?.Nombre
            };
        }

        public async Task<BaseResponseDto<PaginatedResponseDto<LeadDto>>> GetLeadsPaginatedAsync(
            int page, int pageSize, int tenantId, string? nombre = null, int? estadoId = null,
            int? fuenteId = null, int? usuarioAsignadoId = null, int? propiedadId = null, bool? activo = null)
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;

                var (leads, totalRecords) = await _leadRepository.GetPagedByTenantAsync(
                    page, pageSize, tenantId, nombre, estadoId, fuenteId, usuarioAsignadoId, propiedadId, activo);

                var leadsDto = leads.Select(MapToResponseDto).ToList();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var paginatedResponse = new PaginatedResponseDto<LeadDto>
                {
                    Data = leadsDto,
                    Page = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };

                return new BaseResponseDto<PaginatedResponseDto<LeadDto>>
                {
                    Success = true,
                    Data = paginatedResponse,
                    Message = "Leads obtenidos correctamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener leads paginados para tenant: {TenantId}", tenantId);
                return new BaseResponseDto<PaginatedResponseDto<LeadDto>>
                {
                    Success = false,
                    Message = "No se pudieron cargar los leads.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<LeadDto>> GetByIdAndTenantAsync(int id, int tenantId)
        {
            try
            {
                var lead = await _leadRepository.GetByIdAndTenantAsync(id, tenantId);
                if (lead == null)
                {
                    return new BaseResponseDto<LeadDto>
                    {
                        Success = false,
                        Message = "Lead no encontrado en su organización."
                    };
                }

                var leadDto = MapToResponseDto(lead);
                return new BaseResponseDto<LeadDto>
                {
                    Success = true,
                    Data = leadDto,
                    Message = "Lead encontrado"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lead por ID: {Id} en tenant: {TenantId}", id, tenantId);
                return new BaseResponseDto<LeadDto>
                {
                    Success = false,
                    Message = "Error al buscar el lead.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<LeadDto>> CreateLeadAsync(CreateLeadDto createDto, int tenantId, int? usuarioCreadorId = null)
        {
            try
            {
                _logger.LogInformation("Creando lead: {Nombre} en tenant: {TenantId}", createDto.NombreCompleto, tenantId);

                // Validar propiedad si se especifica
                if (createDto.IdPropiedad.HasValue)
                {
                    var propiedadValida = await _leadRepository.ValidatePropiedadInTenantAsync(createDto.IdPropiedad.Value, tenantId);
                    if (!propiedadValida)
                    {
                        return new BaseResponseDto<LeadDto>
                        {
                            Success = false,
                            Message = "La propiedad especificada no pertenece a su organización."
                        };
                    }
                }

                // Validar usuario asignado si se especifica
                if (createDto.IdUsuarioAsignado.HasValue && createDto.IdUsuarioAsignado.Value > 0)
                {
                    var usuarioValido = await _leadRepository.ValidateUsuarioInTenantAsync(createDto.IdUsuarioAsignado.Value, tenantId);
                    if (!usuarioValido)
                    {
                        return new BaseResponseDto<LeadDto>
                        {
                            Success = false,
                            Message = "El usuario asignado no pertenece a su organización o no está activo."
                        };
                    }
                }

                // Validar que la fuente de contacto existe
                var fuenteExists = await _context.FuenteContacto
                    .AnyAsync(f => f.Id == createDto.IdFuente);
                if (!fuenteExists)
                {
                    return new BaseResponseDto<LeadDto>
                    {
                        Success = false,
                        Message = "La fuente de contacto especificada no es válida."
                    };
                }

                var lead = new Lead
                {
                    IdPropiedad = createDto.IdPropiedad,
                    IdInmobiliaria = tenantId,
                    IdUsuarioAsignado = createDto.IdUsuarioAsignado,
                    NombreCompleto = createDto.NombreCompleto.Trim(),
                    Email = createDto.Email?.Trim(),
                    Telefono = createDto.Telefono?.Trim(),
                    Mensaje = createDto.Mensaje?.Trim(),
                    IdFuente = createDto.IdFuente,
                    IdEstado = createDto.IdEstado,
                    Activo = true, // Por defecto activo
                    CreadoEn = DateTime.UtcNow,
                    ActualizadoEn = DateTime.UtcNow
                };

                var result = await _leadRepository.AddAsync(lead);

                // Incrementar contador mensual
                await _usoMensualService.IncrementarLeadsMesActualAsync(tenantId);

                // Crear historial de estado inicial
                if (usuarioCreadorId.HasValue)
                {
                    var historial = new LeadEstadoHistorial
                    {
                        IdLead = result.Id,
                        IdEstadoAnterior = 0,
                        IdEstadoNuevo = result.IdEstado,
                        IdUsuario = usuarioCreadorId.Value,
                        Comentario = "Lead creado",
                        CreadoEn = DateTime.UtcNow
                    };

                    _context.LeadEstadoHistorial.Add(historial);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Lead creado exitosamente con ID: {Id}", result.Id);

                var leadConDetalles = await _leadRepository.GetByIdAndTenantAsync(result.Id, tenantId);
                var responseDto = MapToResponseDto(leadConDetalles ?? result);

                return new BaseResponseDto<LeadDto>
                {
                    Success = true,
                    Data = responseDto,
                    Message = $"Lead {createDto.NombreCompleto} creado correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear lead: {Nombre} en tenant: {TenantId}", createDto?.NombreCompleto, tenantId);
                return new BaseResponseDto<LeadDto>
                {
                    Success = false,
                    Message = "No se pudo crear el lead.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<LeadDto>> UpdateLeadAsync(UpdateLeadDto updateDto, int tenantId)
        {
            try
            {
                var existingLead = await _leadRepository.GetByIdAndTenantAsync(updateDto.Id, tenantId);
                if (existingLead == null)
                {
                    return new BaseResponseDto<LeadDto>
                    {
                        Success = false,
                        Message = "Lead no encontrado en su organización."
                    };
                }

                // Validar propiedad si se cambia
                if (updateDto.IdPropiedad.HasValue && updateDto.IdPropiedad.Value != existingLead.IdPropiedad)
                {
                    var propiedadValida = await _leadRepository.ValidatePropiedadInTenantAsync(updateDto.IdPropiedad.Value, tenantId);
                    if (!propiedadValida)
                    {
                        return new BaseResponseDto<LeadDto>
                        {
                            Success = false,
                            Message = "La propiedad especificada no pertenece a su organización."
                        };
                    }
                }

                // Validar usuario asignado si se cambia
                if (updateDto.IdUsuarioAsignado.HasValue && updateDto.IdUsuarioAsignado.Value > 0)
                {
                    var usuarioValido = await _leadRepository.ValidateUsuarioInTenantAsync(updateDto.IdUsuarioAsignado.Value, tenantId);
                    if (!usuarioValido)
                    {
                        return new BaseResponseDto<LeadDto>
                        {
                            Success = false,
                            Message = "El usuario asignado no pertenece a su organización o no está activo."
                        };
                    }
                }

                // Actualizar campos si vienen en el DTO
                if (updateDto.IdPropiedad.HasValue)
                    existingLead.IdPropiedad = updateDto.IdPropiedad.Value == 0 ? null : updateDto.IdPropiedad.Value;

                if (!string.IsNullOrWhiteSpace(updateDto.NombreCompleto))
                    existingLead.NombreCompleto = updateDto.NombreCompleto.Trim();

                if (updateDto.Email != null)
                    existingLead.Email = string.IsNullOrWhiteSpace(updateDto.Email) ? null : updateDto.Email.Trim();

                if (updateDto.Telefono != null)
                    existingLead.Telefono = string.IsNullOrWhiteSpace(updateDto.Telefono) ? null : updateDto.Telefono.Trim();

                if (updateDto.Mensaje != null)
                    existingLead.Mensaje = string.IsNullOrWhiteSpace(updateDto.Mensaje) ? null : updateDto.Mensaje.Trim();

                if (updateDto.IdFuente.HasValue)
                    existingLead.IdFuente = updateDto.IdFuente.Value;

                if (updateDto.IdUsuarioAsignado.HasValue)
                    existingLead.IdUsuarioAsignado = updateDto.IdUsuarioAsignado.Value == 0 ? null : updateDto.IdUsuarioAsignado.Value;

                if (updateDto.IdEstado.HasValue)
                    existingLead.IdEstado = updateDto.IdEstado.Value;

                if (updateDto.Activo.HasValue)
                    existingLead.Activo = updateDto.Activo.Value;

                // NO permitir cambio de inmobiliaria
                existingLead.IdInmobiliaria = tenantId;
                existingLead.ActualizadoEn = DateTime.UtcNow;

                await _leadRepository.UpdateAsync(existingLead);

                var updatedLead = await _leadRepository.GetByIdAndTenantAsync(existingLead.Id, tenantId);
                var responseDto = MapToResponseDto(updatedLead ?? existingLead);

                return new BaseResponseDto<LeadDto>
                {
                    Success = true,
                    Data = responseDto,
                    Message = "Lead actualizado correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar lead: {Id} en tenant: {TenantId}", updateDto.Id, tenantId);
                return new BaseResponseDto<LeadDto>
                {
                    Success = false,
                    Message = "No se pudo actualizar el lead.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<object>> CambiarEstadoAsync(CambiarEstadoLeadDto cambioDto, int tenantId, int usuarioId)
        {
            try
            {
                var lead = await _leadRepository.GetByIdAndTenantAsync(cambioDto.IdLead, tenantId);
                if (lead == null)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Lead no encontrado en su organización."
                    };
                }

                var estadoAnterior = lead.IdEstado;

                if (estadoAnterior == cambioDto.IdEstadoNuevo)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "El lead ya se encuentra en ese estado."
                    };
                }

                lead.IdEstado = cambioDto.IdEstadoNuevo;
                lead.ActualizadoEn = DateTime.UtcNow;

                await _leadRepository.UpdateAsync(lead);

                // Crear historial de cambio de estado
                var historial = new LeadEstadoHistorial
                {
                    IdLead = lead.Id,
                    IdEstadoAnterior = estadoAnterior,
                    IdEstadoNuevo = cambioDto.IdEstadoNuevo,
                    IdUsuario = usuarioId,
                    Comentario = cambioDto.Comentario ?? "Cambio de estado",
                    CreadoEn = DateTime.UtcNow
                };

                _context.LeadEstadoHistorial.Add(historial);
                await _context.SaveChangesAsync();

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Message = "Estado del lead cambiado correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado del lead: {IdLead} en tenant: {TenantId}", cambioDto.IdLead, tenantId);
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error al cambiar estado del lead.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<object>> AsignarUsuarioAsync(AsignarLeadDto asignacionDto, int tenantId, int usuarioAsignadorId)
        {
            try
            {
                var lead = await _leadRepository.GetByIdAndTenantAsync(asignacionDto.IdLead, tenantId);
                if (lead == null)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Lead no encontrado en su organización."
                    };
                }

                // Validar usuario asignado
                var usuarioValido = await _leadRepository.ValidateUsuarioInTenantAsync(asignacionDto.IdUsuarioAsignado, tenantId);
                if (!usuarioValido)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "El usuario asignado no pertenece a su organización o no está activo."
                    };
                }

                lead.IdUsuarioAsignado = asignacionDto.IdUsuarioAsignado;
                lead.ActualizadoEn = DateTime.UtcNow;

                await _leadRepository.UpdateAsync(lead);

                // Crear historial de asignación
                var historial = new LeadEstadoHistorial
                {
                    IdLead = lead.Id,
                    IdEstadoAnterior = lead.IdEstado,
                    IdEstadoNuevo = lead.IdEstado,
                    IdUsuario = usuarioAsignadorId,
                    Comentario = $"Lead asignado a usuario. {asignacionDto.Comentario}",
                    CreadoEn = DateTime.UtcNow
                };

                _context.LeadEstadoHistorial.Add(historial);
                await _context.SaveChangesAsync();

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Message = "Lead asignado correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar lead: {IdLead} en tenant: {TenantId}", asignacionDto.IdLead, tenantId);
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error al asignar lead.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<object>> DeleteAsync(int id, int tenantId)
        {
            try
            {
                var lead = await _leadRepository.GetByIdAndTenantAsync(id, tenantId);
                if (lead == null)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Lead no encontrado en su organización."
                    };
                }

                // Eliminación lógica: cambiar estado administrativo
                if (!lead.Activo) // Ya eliminado
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "El lead ya se encuentra eliminado."
                    };
                }

                lead.Activo = false; // Estado eliminado
                lead.ActualizadoEn = DateTime.UtcNow;

                await _leadRepository.UpdateAsync(lead);

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Message = "Lead eliminado correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar lead: {Id} en tenant: {TenantId}", id, tenantId);
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error al eliminar lead.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }
    }
}