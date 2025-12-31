using Microsoft.Extensions.Logging;
using inmobiliariaApi.DTOs.Common;
using inmobiliariaApi.DTOs.Inmobiliaria;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;

namespace inmobiliariaApi.Services
{
    public class InmobiliariaService : GenericService<Inmobiliaria>
    {
        private readonly InmobiliariaRepository _inmobiliariaRepository;
        private readonly ILogger<InmobiliariaService> _logger;

        public InmobiliariaService(InmobiliariaRepository inmobiliariaRepository, ILogger<InmobiliariaService> logger)
            : base(inmobiliariaRepository)
        {
            _inmobiliariaRepository = inmobiliariaRepository;
            _logger = logger;
        }

        private async Task<InmobiliariaDto> MapToResponseDto(Inmobiliaria inmobiliaria)
        {
            var stats = await _inmobiliariaRepository.GetStatsAsync(inmobiliaria.Id);

            return new InmobiliariaDto
            {
                Id = inmobiliaria.Id,
                Nombre = inmobiliaria.Nombre,
                Subdominio = inmobiliaria.Subdominio,
                DominioPersonalizado = inmobiliaria.DominioPersonalizado,
                IdPlan = inmobiliaria.IdPlan,
                IdEstado = inmobiliaria.IdEstado,
                CreadoEn = inmobiliaria.CreadoEn,
                ActualizadoEn = inmobiliaria.ActualizadoEn,
                PlanNombre = inmobiliaria.Plan?.Nombre,
                EstadoDescripcion = inmobiliaria.Estado?.Descripcion,
                TotalUsuarios = stats.TotalUsuarios,
                TotalPropiedades = stats.TotalPropiedades,
                TotalLeads = stats.TotalLeads
            };
        }

        public async Task<BaseResponseDto<PaginatedResponseDto<InmobiliariaDto>>> GetInmobiliariasAsync(
            int page, int pageSize, string? nombre = null, int? planId = null, int? estadoId = null)
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;

                var (inmobiliarias, totalRecords) = await _inmobiliariaRepository.GetPagedWithDetailsAsync(
                    page, pageSize, nombre, planId, estadoId);

                var inmobiliariasDto = new List<InmobiliariaDto>();
                foreach (var inmobiliaria in inmobiliarias)
                {
                    inmobiliariasDto.Add(await MapToResponseDto(inmobiliaria));
                }

                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var paginatedResponse = new PaginatedResponseDto<InmobiliariaDto>
                {
                    Data = inmobiliariasDto,
                    Page = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };

                return new BaseResponseDto<PaginatedResponseDto<InmobiliariaDto>>
                {
                    Success = true,
                    Data = paginatedResponse,
                    Message = "Inmobiliarias obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inmobiliarias paginadas");
                return new BaseResponseDto<PaginatedResponseDto<InmobiliariaDto>>
                {
                    Success = false,
                    Message = "No se pudieron cargar las inmobiliarias.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<InmobiliariaDto>> GetByIdAsync(int id)
        {
            try
            {
                var inmobiliaria = await _inmobiliariaRepository.GetByIdWithDetailsAsync(id);
                if (inmobiliaria == null)
                {
                    return new BaseResponseDto<InmobiliariaDto>
                    {
                        Success = false,
                        Message = "Inmobiliaria no encontrada."
                    };
                }

                var inmobiliariaDto = await MapToResponseDto(inmobiliaria);
                return new BaseResponseDto<InmobiliariaDto>
                {
                    Success = true,
                    Data = inmobiliariaDto,
                    Message = "Inmobiliaria encontrada"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inmobiliaria por ID: {Id}", id);
                return new BaseResponseDto<InmobiliariaDto>
                {
                    Success = false,
                    Message = "Error al buscar la inmobiliaria.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<InmobiliariaDto>> CreateInmobiliariaAsync(CreateInmobiliariaDto createDto)
        {
            try
            {
                _logger.LogInformation("Creando inmobiliaria: {Nombre}", createDto.Nombre);

                // Validar plan existe y está activo
                var planValido = await _inmobiliariaRepository.ValidatePlanAsync(createDto.IdPlan);
                if (!planValido)
                {
                    return new BaseResponseDto<InmobiliariaDto>
                    {
                        Success = false,
                        Message = "El plan especificado no existe o no está activo."
                    };
                }

                // Validar subdominio único
                var subdominioExiste = await _inmobiliariaRepository.ExisteSubdominioAsync(createDto.Subdominio);
                if (subdominioExiste)
                {
                    return new BaseResponseDto<InmobiliariaDto>
                    {
                        Success = false,
                        Message = $"El subdominio '{createDto.Subdominio}' ya está en uso."
                    };
                }

                // Validar dominio personalizado único (si se proporciona)
                if (!string.IsNullOrEmpty(createDto.DominioPersonalizado))
                {
                    var dominioExiste = await _inmobiliariaRepository.ExisteDominioPersonalizadoAsync(createDto.DominioPersonalizado);
                    if (dominioExiste)
                    {
                        return new BaseResponseDto<InmobiliariaDto>
                        {
                            Success = false,
                            Message = $"El dominio personalizado '{createDto.DominioPersonalizado}' ya está en uso."
                        };
                    }
                }

                var inmobiliaria = new Inmobiliaria
                {
                    Nombre = createDto.Nombre.Trim(),
                    Subdominio = createDto.Subdominio.Trim().ToLower(),
                    DominioPersonalizado = createDto.DominioPersonalizado?.Trim().ToLower(),
                    IdPlan = createDto.IdPlan,
                    IdEstado = createDto.IdEstado,
                    CreadoEn = DateTime.UtcNow,
                    ActualizadoEn = DateTime.UtcNow
                };

                var result = await _inmobiliariaRepository.AddAsync(inmobiliaria);

                _logger.LogInformation("Inmobiliaria creada exitosamente con ID: {Id}", result.Id);

                var inmobiliariaConDetalles = await _inmobiliariaRepository.GetByIdWithDetailsAsync(result.Id);
                var responseDto = await MapToResponseDto(inmobiliariaConDetalles ?? result);

                return new BaseResponseDto<InmobiliariaDto>
                {
                    Success = true,
                    Data = responseDto,
                    Message = $"Inmobiliaria '{createDto.Nombre}' creada correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear inmobiliaria: {Nombre}", createDto?.Nombre);
                return new BaseResponseDto<InmobiliariaDto>
                {
                    Success = false,
                    Message = "No se pudo crear la inmobiliaria.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<InmobiliariaDto>> UpdateInmobiliariaAsync(UpdateInmobiliariaDto updateDto)
        {
            try
            {
                var existingInmobiliaria = await _inmobiliariaRepository.GetByIdWithDetailsAsync(updateDto.Id);
                if (existingInmobiliaria == null)
                {
                    return new BaseResponseDto<InmobiliariaDto>
                    {
                        Success = false,
                        Message = "Inmobiliaria no encontrada."
                    };
                }

                // Validar plan si se cambia
                if (updateDto.IdPlan.HasValue)
                {
                    var planValido = await _inmobiliariaRepository.ValidatePlanAsync(updateDto.IdPlan.Value);
                    if (!planValido)
                    {
                        return new BaseResponseDto<InmobiliariaDto>
                        {
                            Success = false,
                            Message = "El plan especificado no existe o no está activo."
                        };
                    }
                }

                // Validar subdominio único si se cambia
                if (!string.IsNullOrEmpty(updateDto.Subdominio) &&
                    updateDto.Subdominio.ToLower() != existingInmobiliaria.Subdominio.ToLower())
                {
                    var subdominioExiste = await _inmobiliariaRepository.ExisteSubdominioAsync(updateDto.Subdominio, updateDto.Id);
                    if (subdominioExiste)
                    {
                        return new BaseResponseDto<InmobiliariaDto>
                        {
                            Success = false,
                            Message = $"El subdominio '{updateDto.Subdominio}' ya está en uso."
                        };
                    }
                }

                // Validar dominio personalizado único si se cambia
                if (updateDto.DominioPersonalizado != null &&
                    updateDto.DominioPersonalizado.ToLower() != existingInmobiliaria.DominioPersonalizado?.ToLower())
                {
                    if (!string.IsNullOrEmpty(updateDto.DominioPersonalizado))
                    {
                        var dominioExiste = await _inmobiliariaRepository.ExisteDominioPersonalizadoAsync(updateDto.DominioPersonalizado, updateDto.Id);
                        if (dominioExiste)
                        {
                            return new BaseResponseDto<InmobiliariaDto>
                            {
                                Success = false,
                                Message = $"El dominio personalizado '{updateDto.DominioPersonalizado}' ya está en uso."
                            };
                        }
                    }
                }

                // Actualizar campos si vienen en el DTO
                if (!string.IsNullOrEmpty(updateDto.Nombre))
                    existingInmobiliaria.Nombre = updateDto.Nombre.Trim();

                if (!string.IsNullOrEmpty(updateDto.Subdominio))
                    existingInmobiliaria.Subdominio = updateDto.Subdominio.Trim().ToLower();

                if (updateDto.DominioPersonalizado != null)
                    existingInmobiliaria.DominioPersonalizado = string.IsNullOrEmpty(updateDto.DominioPersonalizado)
                        ? null
                        : updateDto.DominioPersonalizado.Trim().ToLower();

                if (updateDto.IdPlan.HasValue)
                    existingInmobiliaria.IdPlan = updateDto.IdPlan.Value;

                if (updateDto.IdEstado.HasValue)
                    existingInmobiliaria.IdEstado = updateDto.IdEstado.Value;

                existingInmobiliaria.ActualizadoEn = DateTime.UtcNow;

                await _inmobiliariaRepository.UpdateAsync(existingInmobiliaria);

                var updatedInmobiliaria = await _inmobiliariaRepository.GetByIdWithDetailsAsync(existingInmobiliaria.Id);
                var responseDto = await MapToResponseDto(updatedInmobiliaria ?? existingInmobiliaria);

                return new BaseResponseDto<InmobiliariaDto>
                {
                    Success = true,
                    Data = responseDto,
                    Message = "Inmobiliaria actualizada correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar inmobiliaria: {Id}", updateDto.Id);
                return new BaseResponseDto<InmobiliariaDto>
                {
                    Success = false,
                    Message = "No se pudo actualizar la inmobiliaria.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<object>> ChangeStateAsync(int id, int newState)
        {
            try
            {
                var inmobiliaria = await _inmobiliariaRepository.GetByIdWithDetailsAsync(id);
                if (inmobiliaria == null)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Inmobiliaria no encontrada."
                    };
                }

                if (newState < 1 || newState > 3)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Estado no válido. Use 1 (activa), 2 (suspendida) o 3 (cancelada)."
                    };
                }

                inmobiliaria.IdEstado = newState;
                inmobiliaria.ActualizadoEn = DateTime.UtcNow;

                await _inmobiliariaRepository.UpdateAsync(inmobiliaria);

                string estadoTexto = newState switch
                {
                    1 => "activa",
                    2 => "suspendida",
                    3 => "cancelada",
                    _ => "desconocido"
                };

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Message = $"Estado de la inmobiliaria cambiado a {estadoTexto}."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de inmobiliaria: {Id}", id);
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error al cambiar estado de la inmobiliaria.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }
    }
}
