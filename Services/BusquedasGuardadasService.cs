using Microsoft.Extensions.Logging;
using inmobiliariaApi.DTOs.Common;
using inmobiliariaApi.DTOs.BusquedasGuardadas;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;
using System.Text.Json;

namespace inmobiliariaApi.Services
{
    public class BusquedasGuardadasService : GenericService<BusquedasGuardadas>
    {
        private readonly BusquedasGuardadasRepository _busquedasRepository;
        private readonly ILogger<BusquedasGuardadasService> _logger;

        public BusquedasGuardadasService(BusquedasGuardadasRepository busquedasRepository, ILogger<BusquedasGuardadasService> logger)
            : base(busquedasRepository)
        {
            _busquedasRepository = busquedasRepository;
            _logger = logger;
        }

        private BusquedasGuardadasDto MapToResponseDto(BusquedasGuardadas busqueda)
        {
            object? filtrosParsed = null;
            try
            {
                if (!string.IsNullOrEmpty(busqueda.FiltrosJson))
                {
                    filtrosParsed = JsonSerializer.Deserialize<object>(busqueda.FiltrosJson);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Error al parsear filtros JSON para búsqueda ID: {Id}", busqueda.Id);
                filtrosParsed = null;
            }

            return new BusquedasGuardadasDto
            {
                Id = busqueda.Id,
                Email = busqueda.Email,
                FiltrosJson = busqueda.FiltrosJson,
                UltimoEnvio = busqueda.UltimoEnvio,
                CreadoEn = busqueda.CreadoEn,
                ActualizadoEn = busqueda.ActualizadoEn,
                IdInmobiliaria = busqueda.IdInmobiliaria,
                InmobiliariaNombre = busqueda.Inmobiliaria?.Nombre,
                FiltrosParsed = filtrosParsed
            };
        }

        public async Task<BaseResponseDto<PaginatedResponseDto<BusquedasGuardadasDto>>> GetBusquedasPaginatedAsync(
            int page, int pageSize, int tenantId, string? email = null)
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;

                var (busquedas, totalRecords) = await _busquedasRepository.GetPagedByTenantAsync(
                    page, pageSize, tenantId, email);

                var busquedasDto = busquedas.Select(MapToResponseDto).ToList();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var paginatedResponse = new PaginatedResponseDto<BusquedasGuardadasDto>
                {
                    Data = busquedasDto,
                    Page = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };

                return new BaseResponseDto<PaginatedResponseDto<BusquedasGuardadasDto>>
                {
                    Success = true,
                    Data = paginatedResponse,
                    Message = "Búsquedas guardadas obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener búsquedas paginadas para tenant: {TenantId}", tenantId);
                return new BaseResponseDto<PaginatedResponseDto<BusquedasGuardadasDto>>
                {
                    Success = false,
                    Message = "No se pudieron cargar las búsquedas guardadas.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<BusquedasGuardadasDto>>> GetByEmailAndTenantAsync(string email, int tenantId)
        {
            try
            {
                var busquedas = await _busquedasRepository.GetByEmailAndTenantAsync(email, tenantId);
                var busquedasDto = busquedas.Select(MapToResponseDto).ToList();

                return new BaseResponseDto<IEnumerable<BusquedasGuardadasDto>>
                {
                    Success = true,
                    Data = busquedasDto,
                    Message = "Búsquedas del email obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener búsquedas por email: {Email} en tenant: {TenantId}", email, tenantId);
                return new BaseResponseDto<IEnumerable<BusquedasGuardadasDto>>
                {
                    Success = false,
                    Message = "Error al cargar búsquedas del email.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<BusquedasGuardadasDto>> GetByIdAndTenantAsync(int id, int tenantId)
        {
            try
            {
                var busqueda = await _busquedasRepository.GetByIdAndTenantAsync(id, tenantId);
                if (busqueda == null)
                {
                    return new BaseResponseDto<BusquedasGuardadasDto>
                    {
                        Success = false,
                        Message = "Búsqueda guardada no encontrada en su organización."
                    };
                }

                var busquedaDto = MapToResponseDto(busqueda);
                return new BaseResponseDto<BusquedasGuardadasDto>
                {
                    Success = true,
                    Data = busquedaDto,
                    Message = "Búsqueda guardada encontrada"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener búsqueda por ID: {Id} en tenant: {TenantId}", id, tenantId);
                return new BaseResponseDto<BusquedasGuardadasDto>
                {
                    Success = false,
                    Message = "Error al buscar la búsqueda guardada.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<BusquedasGuardadasDto>> CreateBusquedaAsync(CreateBusquedasGuardadasDto createDto, int tenantId)
        {
            try
            {
                _logger.LogInformation("Creando búsqueda guardada para email: {Email} en tenant: {TenantId}", createDto.Email, tenantId);

                // Validar JSON de filtros
                try
                {
                    JsonDocument.Parse(createDto.FiltrosJson);
                }
                catch (JsonException)
                {
                    return new BaseResponseDto<BusquedasGuardadasDto>
                    {
                        Success = false,
                        Message = "Los filtros deben estar en formato JSON válido."
                    };
                }

                // Validar límite de búsquedas por email (máximo 10)
                var countBusquedas = await _busquedasRepository.GetCountByEmailAndTenantAsync(createDto.Email, tenantId);
                if (countBusquedas >= 10)
                {
                    return new BaseResponseDto<BusquedasGuardadasDto>
                    {
                        Success = false,
                        Message = "Se ha alcanzado el límite máximo de 10 búsquedas guardadas por email."
                    };
                }

                // Validar que no exista la misma combinación email + filtros
                var existeBusqueda = await _busquedasRepository.ExistsEmailAndFiltersAsync(createDto.Email, createDto.FiltrosJson, tenantId);
                if (existeBusqueda)
                {
                    return new BaseResponseDto<BusquedasGuardadasDto>
                    {
                        Success = false,
                        Message = "Ya existe una búsqueda guardada con esos filtros para este email."
                    };
                }

                var busqueda = new BusquedasGuardadas
                {
                    Email = createDto.Email.Trim().ToLower(),
                    FiltrosJson = createDto.FiltrosJson.Trim(),
                    IdInmobiliaria = tenantId,
                    CreadoEn = DateTime.UtcNow,
                    ActualizadoEn = DateTime.UtcNow
                };

                var result = await _busquedasRepository.AddAsync(busqueda);

                _logger.LogInformation("Búsqueda guardada creada exitosamente con ID: {Id}", result.Id);

                var busquedaConDetalles = await _busquedasRepository.GetByIdAndTenantAsync(result.Id, tenantId);
                var responseDto = MapToResponseDto(busquedaConDetalles ?? result);

                return new BaseResponseDto<BusquedasGuardadasDto>
                {
                    Success = true,
                    Data = responseDto,
                    Message = "Búsqueda guardada creada correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear búsqueda guardada para email: {Email} en tenant: {TenantId}", createDto?.Email, tenantId);
                return new BaseResponseDto<BusquedasGuardadasDto>
                {
                    Success = false,
                    Message = "No se pudo crear la búsqueda guardada.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<BusquedasGuardadasDto>> UpdateBusquedaAsync(UpdateBusquedasGuardadasDto updateDto, int tenantId)
        {
            try
            {
                var existingBusqueda = await _busquedasRepository.GetByIdAndTenantAsync(updateDto.Id, tenantId);
                if (existingBusqueda == null)
                {
                    return new BaseResponseDto<BusquedasGuardadasDto>
                    {
                        Success = false,
                        Message = "Búsqueda guardada no encontrada en su organización."
                    };
                }

                // Validar JSON de filtros
                try
                {
                    JsonDocument.Parse(updateDto.FiltrosJson);
                }
                catch (JsonException)
                {
                    return new BaseResponseDto<BusquedasGuardadasDto>
                    {
                        Success = false,
                        Message = "Los filtros deben estar en formato JSON válido."
                    };
                }

                // Validar que no exista otra búsqueda con la misma combinación email + filtros
                if (updateDto.Email.ToLower() != existingBusqueda.Email.ToLower() || updateDto.FiltrosJson != existingBusqueda.FiltrosJson)
                {
                    var existeBusqueda = await _busquedasRepository.ExistsEmailAndFiltersAsync(updateDto.Email, updateDto.FiltrosJson, tenantId);
                    if (existeBusqueda)
                    {
                        return new BaseResponseDto<BusquedasGuardadasDto>
                        {
                            Success = false,
                            Message = "Ya existe otra búsqueda guardada con esos filtros para este email."
                        };
                    }
                }

                // Actualizar campos
                existingBusqueda.Email = updateDto.Email.Trim().ToLower();
                existingBusqueda.FiltrosJson = updateDto.FiltrosJson.Trim();
                existingBusqueda.ActualizadoEn = DateTime.UtcNow;

                // NO permitir cambio de inmobiliaria
                existingBusqueda.IdInmobiliaria = tenantId;

                await _busquedasRepository.UpdateAsync(existingBusqueda);

                var updatedBusqueda = await _busquedasRepository.GetByIdAndTenantAsync(existingBusqueda.Id, tenantId);
                var responseDto = MapToResponseDto(updatedBusqueda ?? existingBusqueda);

                return new BaseResponseDto<BusquedasGuardadasDto>
                {
                    Success = true,
                    Data = responseDto,
                    Message = "Búsqueda guardada actualizada correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar búsqueda guardada: {Id} en tenant: {TenantId}", updateDto.Id, tenantId);
                return new BaseResponseDto<BusquedasGuardadasDto>
                {
                    Success = false,
                    Message = "No se pudo actualizar la búsqueda guardada.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<object>> DeleteAsync(int id, int tenantId)
        {
            try
            {
                var busqueda = await _busquedasRepository.GetByIdAndTenantAsync(id, tenantId);
                if (busqueda == null)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Búsqueda guardada no encontrada en su organización."
                    };
                }

                // Eliminación física (no lógica) ya que son solo búsquedas guardadas
                await _busquedasRepository.DeleteAsync(id);

                _logger.LogInformation("Búsqueda guardada ID: {Id} eliminada en tenant: {TenantId}", id, tenantId);

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Message = "Búsqueda guardada eliminada correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar búsqueda guardada: {Id} en tenant: {TenantId}", id, tenantId);
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error al eliminar búsqueda guardada.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<object>> MarcarComoEnviadaAsync(int id, int tenantId)
        {
            try
            {
                var busqueda = await _busquedasRepository.GetByIdAndTenantAsync(id, tenantId);
                if (busqueda == null)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Búsqueda guardada no encontrada en su organización."
                    };
                }

                await _busquedasRepository.ActualizarUltimoEnvioAsync(id);

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Message = "Búsqueda marcada como enviada correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar como enviada búsqueda: {Id} en tenant: {TenantId}", id, tenantId);
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error al actualizar el estado de envío.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }
    }
}
