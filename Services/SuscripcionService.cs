using Microsoft.Extensions.Logging;
using inmobiliariaApi.DTOs.Common;
using inmobiliariaApi.DTOs.Suscripcion;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;

namespace inmobiliariaApi.Services
{
    public class SuscripcionService : GenericService<Suscripcion>
    {
        private readonly SuscripcionRepository _suscripcionRepository;
        private readonly ILogger<SuscripcionService> _logger;

        public SuscripcionService(SuscripcionRepository suscripcionRepository, ILogger<SuscripcionService> logger)
            : base(suscripcionRepository)
        {
            _suscripcionRepository = suscripcionRepository;
            _logger = logger;
        }

        private SuscripcionDto MapToResponseDto(Suscripcion suscripcion)
        {
            var ahora = DateTime.UtcNow;
            var vigente = suscripcion.Inicio <= ahora && suscripcion.Fin >= ahora && suscripcion.IdEstado == 1;
            var diasRestantes = vigente ? (int)(suscripcion.Fin - ahora).TotalDays : 0;
            var porVencer = vigente && diasRestantes <= 30;

            return new SuscripcionDto
            {
                Id = suscripcion.Id,
                IdInmobiliaria = suscripcion.IdInmobiliaria,
                IdPlan = suscripcion.IdPlan,
                Inicio = suscripcion.Inicio,
                Fin = suscripcion.Fin,
                RenovacionAutomatica = suscripcion.RenovacionAutomatica,
                CreadoEn = suscripcion.CreadoEn,
                ActualizadoEn = suscripcion.ActualizadoEn,
                IdEstado = suscripcion.IdEstado,
                InmobiliariaNombre = suscripcion.Inmobiliaria?.Nombre,
                EstadoDescripcion = suscripcion.Estado?.Descripcion,
                Vigente = vigente,
                DiasRestantes = Math.Max(0, diasRestantes),
                PorVencer = porVencer
            };
        }

        public async Task<BaseResponseDto<PaginatedResponseDto<SuscripcionDto>>> GetSuscripcionesAsync(
            int page, int pageSize, int? inmobiliariaId = null, int? planId = null, byte? estadoId = null, bool? vigente = null)
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;

                var (suscripciones, totalRecords) = await _suscripcionRepository.GetPagedWithDetailsAsync(
                    page, pageSize, inmobiliariaId, planId, estadoId, vigente);

                var suscripcionesDto = suscripciones.Select(MapToResponseDto).ToList();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var paginatedResponse = new PaginatedResponseDto<SuscripcionDto>
                {
                    Data = suscripcionesDto,
                    Page = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };

                return new BaseResponseDto<PaginatedResponseDto<SuscripcionDto>>
                {
                    Success = true,
                    Data = paginatedResponse,
                    Message = "Suscripciones obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener suscripciones paginadas");
                return new BaseResponseDto<PaginatedResponseDto<SuscripcionDto>>
                {
                    Success = false,
                    Message = "No se pudieron cargar las suscripciones.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<SuscripcionDto>> GetByIdAsync(int id)
        {
            try
            {
                var suscripcion = await _suscripcionRepository.GetByIdWithDetailsAsync(id);
                if (suscripcion == null)
                {
                    return new BaseResponseDto<SuscripcionDto>
                    {
                        Success = false,
                        Message = "Suscripción no encontrada."
                    };
                }

                var suscripcionDto = MapToResponseDto(suscripcion);
                return new BaseResponseDto<SuscripcionDto>
                {
                    Success = true,
                    Data = suscripcionDto,
                    Message = "Suscripción encontrada"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener suscripción por ID: {Id}", id);
                return new BaseResponseDto<SuscripcionDto>
                {
                    Success = false,
                    Message = "Error al buscar la suscripción.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<SuscripcionDto>> GetActiveByInmobiliariaAsync(int inmobiliariaId)
        {
            try
            {
                var suscripcion = await _suscripcionRepository.GetActiveByInmobiliariaAsync(inmobiliariaId);
                if (suscripcion == null)
                {
                    return new BaseResponseDto<SuscripcionDto>
                    {
                        Success = false,
                        Message = "No se encontró una suscripción activa para la inmobiliaria."
                    };
                }

                var suscripcionDto = MapToResponseDto(suscripcion);
                return new BaseResponseDto<SuscripcionDto>
                {
                    Success = true,
                    Data = suscripcionDto,
                    Message = "Suscripción activa encontrada"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener suscripción activa para inmobiliaria: {InmobiliariaId}", inmobiliariaId);
                return new BaseResponseDto<SuscripcionDto>
                {
                    Success = false,
                    Message = "Error al buscar la suscripción activa.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<SuscripcionDto>> CreateSuscripcionAsync(CreateSuscripcionDto createDto)
        {
            try
            {
                _logger.LogInformation("Creando suscripción para inmobiliaria: {InmobiliariaId}", createDto.IdInmobiliaria);

                // Validar fechas
                if (createDto.Fin <= createDto.Inicio)
                {
                    return new BaseResponseDto<SuscripcionDto>
                    {
                        Success = false,
                        Message = "La fecha de fin debe ser posterior a la fecha de inicio."
                    };
                }

                // Validar inmobiliaria
                var inmobiliariaValida = await _suscripcionRepository.ValidateInmobiliariaAsync(createDto.IdInmobiliaria);
                if (!inmobiliariaValida)
                {
                    return new BaseResponseDto<SuscripcionDto>
                    {
                        Success = false,
                        Message = "La inmobiliaria especificada no existe o no está activa."
                    };
                }

                // Validar plan
                var planValido = await _suscripcionRepository.ValidatePlanAsync(createDto.IdPlan);
                if (!planValido)
                {
                    return new BaseResponseDto<SuscripcionDto>
                    {
                        Success = false,
                        Message = "El plan especificado no existe o no está activo."
                    };
                }

                // Validar que no haya otra suscripción activa en el período
                var hasActiveSuscripcion = await _suscripcionRepository.HasActiveSuscripcionAsync(createDto.IdInmobiliaria);
                if (hasActiveSuscripcion)
                {
                    return new BaseResponseDto<SuscripcionDto>
                    {
                        Success = false,
                        Message = "La inmobiliaria ya tiene una suscripción activa. Debe finalizar o pausar la actual antes de crear una nueva."
                    };
                }

                var suscripcion = new Suscripcion
                {
                    IdInmobiliaria = createDto.IdInmobiliaria,
                    IdPlan = createDto.IdPlan,
                    Inicio = createDto.Inicio.ToUniversalTime(),
                    Fin = createDto.Fin.ToUniversalTime(),
                    RenovacionAutomatica = createDto.RenovacionAutomatica,
                    IdEstado = createDto.IdEstado,
                    CreadoEn = DateTime.UtcNow,
                    ActualizadoEn = DateTime.UtcNow
                };

                var result = await _suscripcionRepository.AddAsync(suscripcion);

                _logger.LogInformation("Suscripción creada exitosamente con ID: {Id}", result.Id);

                var suscripcionConDetalles = await _suscripcionRepository.GetByIdWithDetailsAsync(result.Id);
                var responseDto = MapToResponseDto(suscripcionConDetalles ?? result);

                return new BaseResponseDto<SuscripcionDto>
                {
                    Success = true,
                    Data = responseDto,
                    Message = "Suscripción creada correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear suscripción para inmobiliaria: {InmobiliariaId}", createDto?.IdInmobiliaria);
                return new BaseResponseDto<SuscripcionDto>
                {
                    Success = false,
                    Message = "No se pudo crear la suscripción.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<SuscripcionDto>> UpdateSuscripcionAsync(UpdateSuscripcionDto updateDto)
        {
            try
            {
                var existingSuscripcion = await _suscripcionRepository.GetByIdWithDetailsAsync(updateDto.Id);
                if (existingSuscripcion == null)
                {
                    return new BaseResponseDto<SuscripcionDto>
                    {
                        Success = false,
                        Message = "Suscripción no encontrada."
                    };
                }

                // Validar fechas si se modifican
                var inicio = updateDto.Inicio ?? existingSuscripcion.Inicio;
                var fin = updateDto.Fin ?? existingSuscripcion.Fin;

                if (fin <= inicio)
                {
                    return new BaseResponseDto<SuscripcionDto>
                    {
                        Success = false,
                        Message = "La fecha de fin debe ser posterior a la fecha de inicio."
                    };
                }

                // Validar plan si se cambia
                if (updateDto.IdPlan.HasValue)
                {
                    var planValido = await _suscripcionRepository.ValidatePlanAsync(updateDto.IdPlan.Value);
                    if (!planValido)
                    {
                        return new BaseResponseDto<SuscripcionDto>
                        {
                            Success = false,
                            Message = "El plan especificado no existe o no está activo."
                        };
                    }
                }

                // Actualizar campos si vienen en el DTO
                if (updateDto.Inicio.HasValue)
                    existingSuscripcion.Inicio = updateDto.Inicio.Value.ToUniversalTime();

                if (updateDto.Fin.HasValue)
                    existingSuscripcion.Fin = updateDto.Fin.Value.ToUniversalTime();

                if (updateDto.RenovacionAutomatica.HasValue)
                    existingSuscripcion.RenovacionAutomatica = updateDto.RenovacionAutomatica.Value;

                if (updateDto.IdEstado.HasValue)
                    existingSuscripcion.IdEstado = updateDto.IdEstado.Value;

                if (updateDto.IdPlan.HasValue)
                    existingSuscripcion.IdPlan = updateDto.IdPlan.Value;

                // NO permitir cambio de inmobiliaria
                existingSuscripcion.ActualizadoEn = DateTime.UtcNow;

                await _suscripcionRepository.UpdateAsync(existingSuscripcion);

                var updatedSuscripcion = await _suscripcionRepository.GetByIdWithDetailsAsync(existingSuscripcion.Id);
                var responseDto = MapToResponseDto(updatedSuscripcion ?? existingSuscripcion);

                return new BaseResponseDto<SuscripcionDto>
                {
                    Success = true,
                    Data = responseDto,
                    Message = "Suscripción actualizada correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar suscripción: {Id}", updateDto.Id);
                return new BaseResponseDto<SuscripcionDto>
                {
                    Success = false,
                    Message = "No se pudo actualizar la suscripción.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<SuscripcionDto>> RenovarSuscripcionAsync(RenovarSuscripcionDto renovarDto)
        {
            try
            {
                var suscripcion = await _suscripcionRepository.GetByIdWithDetailsAsync(renovarDto.IdSuscripcion);
                if (suscripcion == null)
                {
                    return new BaseResponseDto<SuscripcionDto>
                    {
                        Success = false,
                        Message = "Suscripción no encontrada."
                    };
                }

                // Validar que la nueva fecha sea posterior a la actual
                if (renovarDto.NuevaFechaFin <= suscripcion.Fin)
                {
                    return new BaseResponseDto<SuscripcionDto>
                    {
                        Success = false,
                        Message = "La nueva fecha de fin debe ser posterior a la fecha de fin actual."
                    };
                }

                // Validar plan si se cambia
                if (renovarDto.NuevoIdPlan.HasValue)
                {
                    var planValido = await _suscripcionRepository.ValidatePlanAsync(renovarDto.NuevoIdPlan.Value);
                    if (!planValido)
                    {
                        return new BaseResponseDto<SuscripcionDto>
                        {
                            Success = false,
                            Message = "El nuevo plan especificado no existe o no está activo."
                        };
                    }
                    suscripcion.IdPlan = renovarDto.NuevoIdPlan.Value;
                }

                // Renovar suscripción
                suscripcion.Fin = renovarDto.NuevaFechaFin.ToUniversalTime();

                if (renovarDto.RenovacionAutomatica.HasValue)
                    suscripcion.RenovacionAutomatica = renovarDto.RenovacionAutomatica.Value;

                suscripcion.IdEstado = 1; // Activar si estaba en otro estado
                suscripcion.ActualizadoEn = DateTime.UtcNow;

                await _suscripcionRepository.UpdateAsync(suscripcion);

                var renewedSuscripcion = await _suscripcionRepository.GetByIdWithDetailsAsync(suscripcion.Id);
                var responseDto = MapToResponseDto(renewedSuscripcion ?? suscripcion);

                _logger.LogInformation("Suscripción ID: {Id} renovada hasta {FechaFin}", suscripcion.Id, renovarDto.NuevaFechaFin);

                return new BaseResponseDto<SuscripcionDto>
                {
                    Success = true,
                    Data = responseDto,
                    Message = $"Suscripción renovada correctamente hasta {renovarDto.NuevaFechaFin:dd/MM/yyyy}."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al renovar suscripción: {Id}", renovarDto.IdSuscripcion);
                return new BaseResponseDto<SuscripcionDto>
                {
                    Success = false,
                    Message = "No se pudo renovar la suscripción.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<object>> CambiarEstadoAsync(int id, byte nuevoEstado)
        {
            try
            {
                var suscripcion = await _suscripcionRepository.GetByIdWithDetailsAsync(id);
                if (suscripcion == null)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Suscripción no encontrada."
                    };
                }

                if (nuevoEstado < 1 || nuevoEstado > 4)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Estado no válido. Use 1 (activa), 2 (pausada), 3 (vencida) o 4 (cancelada)."
                    };
                }

                suscripcion.IdEstado = nuevoEstado;
                suscripcion.ActualizadoEn = DateTime.UtcNow;

                await _suscripcionRepository.UpdateAsync(suscripcion);

                string estadoTexto = nuevoEstado switch
                {
                    1 => "activa",
                    2 => "pausada",
                    3 => "vencida",
                    4 => "cancelada",
                    _ => "desconocido"
                };

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Message = $"Estado de la suscripción cambiado a {estadoTexto}."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de suscripción: {Id}", id);
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error al cambiar estado de la suscripción.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<SuscripcionDto>>> GetVencenSoonAsync(int dias = 30)
        {
            try
            {
                var suscripciones = await _suscripcionRepository.GetVencenSoonAsync(dias);
                var suscripcionesDto = suscripciones.Select(MapToResponseDto).ToList();

                return new BaseResponseDto<IEnumerable<SuscripcionDto>>
                {
                    Success = true,
                    Data = suscripcionesDto,
                    Message = $"Suscripciones que vencen en los próximos {dias} días obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener suscripciones que vencen pronto");
                return new BaseResponseDto<IEnumerable<SuscripcionDto>>
                {
                    Success = false,
                    Message = "Error al cargar suscripciones próximas a vencer.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }
    }
}
