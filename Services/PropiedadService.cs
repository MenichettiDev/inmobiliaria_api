using Microsoft.Extensions.Logging;
using inmobiliariaApi.DTOs.Common;
using inmobiliariaApi.DTOs.Propiedad;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;

namespace inmobiliariaApi.Services
{
    public class PropiedadService : GenericService<Propiedad>
    {
        private readonly PropiedadRepository _propiedadRepository;
        private readonly ILogger<PropiedadService> _logger;

        public PropiedadService(PropiedadRepository propiedadRepository, ILogger<PropiedadService> logger)
            : base(propiedadRepository)
        {
            _propiedadRepository = propiedadRepository;
            _logger = logger;
        }

        private PropiedadResponseDto MapToResponseDto(Propiedad propiedad)
        {
            return new PropiedadResponseDto
            {
                Id = propiedad.Id,
                Titulo = propiedad.Titulo,
                Descripcion = propiedad.Descripcion,
                Precio = propiedad.Precio,
                Direccion = propiedad.Direccion,
                Latitud = propiedad.Latitud,
                Longitud = propiedad.Longitud,
                PublicadaEn = propiedad.PublicadaEn,
                CreadoEn = propiedad.CreadoEn,
                ActualizadoEn = propiedad.ActualizadoEn,
                IdInmobiliaria = propiedad.IdInmobiliaria,
                IdAgenteResponsable = propiedad.IdAgenteResponsable,
                AgenteResponsableNombre = propiedad.AgenteResponsable?.Nombre,
                IdEstadoAdmin = propiedad.IdEstadoAdmin,
                IdEstadoOperativo = propiedad.IdEstadoOperativo,
                EstadoAdminNombre = propiedad.EstadoAdmin?.Descripcion,
                EstadoOperativoNombre = propiedad.EstadoOperativo?.Descripcion
            };
        }

        public async Task<BaseResponseDto<PaginatedResponseDto<PropiedadResponseDto>>> GetAllPropiedadesPaginatedAsync(
            int page, int pageSize, int tenantId, string? titulo = null, int? agenteId = null,
            int? estadoAdmin = null, int? estadoOperativo = null, decimal? precioMin = null, decimal? precioMax = null)
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;

                var (propiedades, totalRecords) = await _propiedadRepository.GetAllWithDetailsPagedByTenantAsync(
                    page, pageSize, tenantId, titulo, agenteId, estadoAdmin, estadoOperativo, precioMin, precioMax);

                var propiedadesDto = propiedades.Select(MapToResponseDto).ToList();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var paginatedResponse = new PaginatedResponseDto<PropiedadResponseDto>
                {
                    Data = propiedadesDto,
                    Page = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };

                return new BaseResponseDto<PaginatedResponseDto<PropiedadResponseDto>>
                {
                    Success = true,
                    Data = paginatedResponse,
                    Message = "Propiedades obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener propiedades paginadas para tenant: {TenantId}", tenantId);
                return new BaseResponseDto<PaginatedResponseDto<PropiedadResponseDto>>
                {
                    Success = false,
                    Message = "No se pudieron cargar las propiedades.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<PropiedadResponseDto>> GetPropiedadByIdAndTenantAsync(int id, int tenantId)
        {
            try
            {
                var propiedad = await _propiedadRepository.GetByIdWithDetailsAndTenantAsync(id, tenantId);
                if (propiedad == null)
                {
                    return new BaseResponseDto<PropiedadResponseDto>
                    {
                        Success = false,
                        Message = $"No se encontró una propiedad con el ID {id} en su organización."
                    };
                }

                var propiedadDto = MapToResponseDto(propiedad);
                return new BaseResponseDto<PropiedadResponseDto>
                {
                    Success = true,
                    Data = propiedadDto,
                    Message = "Propiedad encontrada"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener propiedad por ID: {Id} y Tenant: {TenantId}", id, tenantId);
                return new BaseResponseDto<PropiedadResponseDto>
                {
                    Success = false,
                    Message = "Error al buscar la propiedad.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<PropiedadResponseDto>> CreatePropiedadAsync(CreatePropiedadDto createDto)
        {
            try
            {
                _logger.LogInformation("Iniciando creación de propiedad: {Titulo} para inmobiliaria: {IdInmobiliaria}",
                    createDto.Titulo, createDto.IdInmobiliaria);

                // Validar que el agente responsable pertenezca a la misma inmobiliaria (si se proporciona)
                if (createDto.IdAgenteResponsable.HasValue && createDto.IdAgenteResponsable.Value > 0)
                {
                    var agenteValido = await _propiedadRepository.ValidateAgenteInTenantAsync(
                        createDto.IdAgenteResponsable.Value, createDto.IdInmobiliaria);

                    if (!agenteValido)
                    {
                        return new BaseResponseDto<PropiedadResponseDto>
                        {
                            Success = false,
                            Message = "El agente responsable seleccionado no pertenece a su inmobiliaria o no está activo."
                        };
                    }
                }

                var propiedad = new Propiedad
                {
                    Titulo = createDto.Titulo,
                    Descripcion = createDto.Descripcion,
                    Precio = createDto.Precio,
                    Direccion = createDto.Direccion,
                    Latitud = createDto.Latitud,
                    Longitud = createDto.Longitud,
                    IdInmobiliaria = createDto.IdInmobiliaria, // Viene del tenant del controller
                    IdAgenteResponsable = createDto.IdAgenteResponsable,
                    IdEstadoAdmin = 1, // Por defecto activa
                    IdEstadoOperativo = 1, // Por defecto disponible
                    CreadoEn = DateTime.UtcNow,
                    ActualizadoEn = DateTime.UtcNow
                };

                var result = await _propiedadRepository.AddAsync(propiedad);

                _logger.LogInformation("Propiedad creada exitosamente con ID: {Id}", result.Id);

                var propiedadConDetalles = await _propiedadRepository.GetByIdWithDetailsAsync(result.Id);
                var responseDto = MapToResponseDto(propiedadConDetalles ?? result);

                return new BaseResponseDto<PropiedadResponseDto>
                {
                    Success = true,
                    Data = responseDto,
                    Message = $"La propiedad {createDto.Titulo} ha sido creada exitosamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear propiedad: {Titulo}", createDto?.Titulo);
                return new BaseResponseDto<PropiedadResponseDto>
                {
                    Success = false,
                    Message = "No se pudo crear la propiedad.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<PropiedadResponseDto>> UpdatePropiedadAsync(UpdatePropiedadDto updateDto, int tenantId)
        {
            try
            {
                _logger.LogInformation("Iniciando actualización de propiedad ID: {Id} en tenant: {TenantId}", updateDto.Id, tenantId);

                var existingPropiedad = await _propiedadRepository.GetByIdWithDetailsAndTenantAsync(updateDto.Id, tenantId);
                if (existingPropiedad == null)
                {
                    return new BaseResponseDto<PropiedadResponseDto>
                    {
                        Success = false,
                        Message = $"No se encontró una propiedad con el ID {updateDto.Id} en su organización."
                    };
                }

                // Validar que el agente responsable pertenezca a la misma inmobiliaria (si se modifica)
                if (updateDto.IdAgenteResponsable.HasValue && updateDto.IdAgenteResponsable.Value > 0)
                {
                    var agenteValido = await _propiedadRepository.ValidateAgenteInTenantAsync(
                        updateDto.IdAgenteResponsable.Value, tenantId);

                    if (!agenteValido)
                    {
                        return new BaseResponseDto<PropiedadResponseDto>
                        {
                            Success = false,
                            Message = "El agente responsable seleccionado no pertenece a su inmobiliaria o no está activo."
                        };
                    }
                }

                // Actualizar campos solo si vienen en el DTO
                if (!string.IsNullOrWhiteSpace(updateDto.Titulo))
                    existingPropiedad.Titulo = updateDto.Titulo.Trim();

                if (!string.IsNullOrWhiteSpace(updateDto.Descripcion))
                    existingPropiedad.Descripcion = updateDto.Descripcion.Trim();

                if (updateDto.Precio.HasValue)
                    existingPropiedad.Precio = updateDto.Precio.Value;

                if (!string.IsNullOrWhiteSpace(updateDto.Direccion))
                    existingPropiedad.Direccion = updateDto.Direccion.Trim();

                if (updateDto.Latitud.HasValue)
                    existingPropiedad.Latitud = updateDto.Latitud.Value;

                if (updateDto.Longitud.HasValue)
                    existingPropiedad.Longitud = updateDto.Longitud.Value;

                // Para IdAgenteResponsable, permitir asignar null (sin agente) o 0 para quitar asignación
                if (updateDto.IdAgenteResponsable.HasValue)
                {
                    if (updateDto.IdAgenteResponsable.Value == 0)
                        existingPropiedad.IdAgenteResponsable = null; // Sin agente asignado
                    else
                        existingPropiedad.IdAgenteResponsable = updateDto.IdAgenteResponsable.Value;
                }

                if (updateDto.IdEstadoAdmin.HasValue)
                    existingPropiedad.IdEstadoAdmin = updateDto.IdEstadoAdmin.Value;

                if (updateDto.IdEstadoOperativo.HasValue)
                    existingPropiedad.IdEstadoOperativo = updateDto.IdEstadoOperativo.Value;

                // NO permitir cambio de inmobiliaria - mantener siempre el tenant original
                existingPropiedad.IdInmobiliaria = tenantId;
                existingPropiedad.ActualizadoEn = DateTime.UtcNow;

                await _propiedadRepository.UpdateAsync(existingPropiedad);

                _logger.LogInformation("Propiedad ID: {Id} actualizada exitosamente en tenant: {TenantId}", updateDto.Id, tenantId);

                // Cargar la propiedad actualizada con todos los detalles
                var updatedPropiedad = await _propiedadRepository.GetByIdWithDetailsAndTenantAsync(existingPropiedad.Id, tenantId);
                var responseDto = MapToResponseDto(updatedPropiedad ?? existingPropiedad);

                return new BaseResponseDto<PropiedadResponseDto>
                {
                    Success = true,
                    Data = responseDto,
                    Message = $"La propiedad {existingPropiedad.Titulo} ha sido actualizada correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar propiedad: {Id} en tenant: {TenantId}", updateDto.Id, tenantId);
                return new BaseResponseDto<PropiedadResponseDto>
                {
                    Success = false,
                    Message = "No se pudo actualizar la propiedad.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<object>> DeleteAsync(int id, int tenantId)
        {
            try
            {
                var propiedad = await _propiedadRepository.GetByIdWithDetailsAndTenantAsync(id, tenantId);
                if (propiedad == null)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Propiedad no encontrada en su organización."
                    };
                }

                // Verificar que la propiedad no esté ya inactiva
                if (propiedad.IdEstadoAdmin == 0)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "La propiedad ya se encuentra inactiva."
                    };
                }

                // ELIMINACIÓN LÓGICA ÚNICAMENTE - cambiar estado administrativo a inactivo (0)
                propiedad.IdEstadoAdmin = 0; // Estado inactivo
                propiedad.ActualizadoEn = DateTime.UtcNow;

                // NO usar el método DeleteAsync del repositorio base, solo UpdateAsync
                await _propiedadRepository.UpdateAsync(propiedad);

                _logger.LogInformation("Propiedad ID: {Id} marcada como inactiva (eliminación lógica) en tenant: {TenantId}", id, tenantId);

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Message = "Propiedad desactivada correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar propiedad: {Id} en tenant: {TenantId}", id, tenantId);
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error al desactivar propiedad.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        // Nuevo método: reactivar propiedad (forzar estado administrativo activo = 1)
        public async Task<BaseResponseDto<object>> ReactivateAsync(int id, int tenantId)
        {
            try
            {
                var propiedad = await _propiedadRepository.GetByIdWithDetailsAndTenantAsync(id, tenantId);
                if (propiedad == null)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Propiedad no encontrada en su organización."
                    };
                }

                // Si ya está activa, retornar mensaje adecuado
                if (propiedad.IdEstadoAdmin == 1)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = true,
                        Message = "La propiedad ya se encuentra activa."
                    };
                }

                propiedad.IdEstadoAdmin = 1; // Activar
                propiedad.ActualizadoEn = DateTime.UtcNow;

                await _propiedadRepository.UpdateAsync(propiedad);

                _logger.LogInformation("Propiedad ID: {Id} reactivada en tenant: {TenantId}", id, tenantId);

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Message = "Propiedad reactivada correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reactivar propiedad: {Id} en tenant: {TenantId}", id, tenantId);
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error al reactivar la propiedad.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<List<PropiedadComboDto>>> GetPropiedadesComboAsync(int tenantId)
        {
            try
            {
                var propiedades = await _propiedadRepository.GetPropiedadesComboAsync(tenantId);

                var dto = propiedades.Select(p => new PropiedadComboDto
                {
                    Id = p.Id,
                    Titulo = p.Titulo,
                    Direccion = p.Direccion,
                    Precio = p.Precio
                }).ToList();

                return new BaseResponseDto<List<PropiedadComboDto>>
                {
                    Success = true,
                    Data = dto,
                    Message = "Propiedades para combo obtenidas correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener propiedades para combo del tenant {TenantId}", tenantId);
                return new BaseResponseDto<List<PropiedadComboDto>>
                {
                    Success = false,
                    Message = "No se pudieron cargar las propiedades para el combo.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }
    }
}
