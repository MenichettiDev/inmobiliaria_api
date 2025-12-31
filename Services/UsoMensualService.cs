using Microsoft.Extensions.Logging;
using inmobiliariaApi.DTOs.Common;
using inmobiliariaApi.DTOs.UsoMensual;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;
using System.Globalization;

namespace inmobiliariaApi.Services
{
    public class UsoMensualService : GenericService<UsoMensual>
    {
        private readonly UsoMensualRepository _usoMensualRepository;
        private readonly ILogger<UsoMensualService> _logger;

        public UsoMensualService(UsoMensualRepository usoMensualRepository, ILogger<UsoMensualService> logger)
            : base(usoMensualRepository)
        {
            _usoMensualRepository = usoMensualRepository;
            _logger = logger;
        }

        private UsoMensualDto MapToResponseDto(UsoMensual uso)
        {
            return new UsoMensualDto
            {
                Id = uso.Id,
                IdInmobiliaria = uso.IdInmobiliaria,
                Mes = uso.Mes,
                LeadsGenerados = uso.LeadsGenerados,
                CreadoEn = uso.CreadoEn,
                ActualizadoEn = uso.ActualizadoEn,
                InmobiliariaNombre = uso.Inmobiliaria?.Nombre,
                MesTexto = ConvertirMesATexto(uso.Mes)
            };
        }

        private string ConvertirMesATexto(string mes)
        {
            try
            {
                if (DateTime.TryParseExact($"{mes}-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
                {
                    return fecha.ToString("MMMM yyyy", new CultureInfo("es-ES"));
                }
                return mes;
            }
            catch
            {
                return mes;
            }
        }

        public async Task<BaseResponseDto<PaginatedResponseDto<UsoMensualDto>>> GetUsoMensualPaginatedAsync(
            int page, int pageSize, int tenantId, string? mesDesde = null, string? mesHasta = null)
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;

                var (usoMensual, totalRecords) = await _usoMensualRepository.GetPagedByTenantAsync(
                    page, pageSize, tenantId, mesDesde, mesHasta);

                var usoMensualDto = usoMensual.Select(MapToResponseDto).ToList();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var paginatedResponse = new PaginatedResponseDto<UsoMensualDto>
                {
                    Data = usoMensualDto,
                    Page = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };

                return new BaseResponseDto<PaginatedResponseDto<UsoMensualDto>>
                {
                    Success = true,
                    Data = paginatedResponse,
                    Message = "Uso mensual obtenido correctamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener uso mensual paginado para tenant: {TenantId}", tenantId);
                return new BaseResponseDto<PaginatedResponseDto<UsoMensualDto>>
                {
                    Success = false,
                    Message = "No se pudo cargar el uso mensual.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<UsoMensualDto>> GetByIdAndTenantAsync(int id, int tenantId)
        {
            try
            {
                var uso = await _usoMensualRepository.GetByIdAndTenantAsync(id, tenantId);
                if (uso == null)
                {
                    return new BaseResponseDto<UsoMensualDto>
                    {
                        Success = false,
                        Message = "Registro de uso mensual no encontrado en su organización."
                    };
                }

                var usoDto = MapToResponseDto(uso);
                return new BaseResponseDto<UsoMensualDto>
                {
                    Success = true,
                    Data = usoDto,
                    Message = "Registro encontrado"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener uso mensual por ID: {Id} en tenant: {TenantId}", id, tenantId);
                return new BaseResponseDto<UsoMensualDto>
                {
                    Success = false,
                    Message = "Error al buscar el registro.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<UsoMensualDto>>> GetUltimosSeisMesesAsync(int tenantId)
        {
            try
            {
                var usoMensual = await _usoMensualRepository.GetUltimosSeisMesesAsync(tenantId);
                var usoDto = usoMensual.Select(MapToResponseDto).ToList();

                return new BaseResponseDto<IEnumerable<UsoMensualDto>>
                {
                    Success = true,
                    Data = usoDto,
                    Message = "Últimos 6 meses obtenidos correctamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener últimos 6 meses para tenant: {TenantId}", tenantId);
                return new BaseResponseDto<IEnumerable<UsoMensualDto>>
                {
                    Success = false,
                    Message = "Error al cargar estadísticas.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<UsoMensualDto>> CreateUsoMensualAsync(CreateUsoMensualDto createDto, int tenantId)
        {
            try
            {
                _logger.LogInformation("Creando uso mensual para mes: {Mes} en tenant: {TenantId}", createDto.Mes, tenantId);

                // Validar que no exista ya un registro para ese mes
                var existingUso = await _usoMensualRepository.GetByMesAndTenantAsync(createDto.Mes, tenantId);
                if (existingUso != null)
                {
                    return new BaseResponseDto<UsoMensualDto>
                    {
                        Success = false,
                        Message = $"Ya existe un registro para el mes {createDto.Mes}. Use la opción de actualizar."
                    };
                }

                var uso = new UsoMensual
                {
                    IdInmobiliaria = tenantId,
                    Mes = createDto.Mes,
                    LeadsGenerados = createDto.LeadsGenerados,
                    CreadoEn = DateTime.UtcNow,
                    ActualizadoEn = DateTime.UtcNow
                };

                var result = await _usoMensualRepository.AddAsync(uso);

                _logger.LogInformation("Uso mensual creado exitosamente con ID: {Id}", result.Id);

                var usoConDetalles = await _usoMensualRepository.GetByIdAndTenantAsync(result.Id, tenantId);
                var responseDto = MapToResponseDto(usoConDetalles ?? result);

                return new BaseResponseDto<UsoMensualDto>
                {
                    Success = true,
                    Data = responseDto,
                    Message = $"Registro de uso mensual para {ConvertirMesATexto(createDto.Mes)} creado correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear uso mensual para mes: {Mes} en tenant: {TenantId}", createDto?.Mes, tenantId);
                return new BaseResponseDto<UsoMensualDto>
                {
                    Success = false,
                    Message = "No se pudo crear el registro de uso mensual.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<UsoMensualDto>> UpdateUsoMensualAsync(UpdateUsoMensualDto updateDto, int tenantId)
        {
            try
            {
                var existingUso = await _usoMensualRepository.GetByIdAndTenantAsync(updateDto.Id, tenantId);
                if (existingUso == null)
                {
                    return new BaseResponseDto<UsoMensualDto>
                    {
                        Success = false,
                        Message = "Registro de uso mensual no encontrado en su organización."
                    };
                }

                // Validar que no exista otro registro para el nuevo mes (si se cambia)
                if (updateDto.Mes != existingUso.Mes)
                {
                    var existingMes = await _usoMensualRepository.GetByMesAndTenantAsync(updateDto.Mes, tenantId);
                    if (existingMes != null)
                    {
                        return new BaseResponseDto<UsoMensualDto>
                        {
                            Success = false,
                            Message = $"Ya existe un registro para el mes {updateDto.Mes}."
                        };
                    }
                }

                // Actualizar campos
                existingUso.Mes = updateDto.Mes;
                existingUso.LeadsGenerados = updateDto.LeadsGenerados;
                existingUso.ActualizadoEn = DateTime.UtcNow;

                // NO permitir cambio de inmobiliaria
                existingUso.IdInmobiliaria = tenantId;

                await _usoMensualRepository.UpdateAsync(existingUso);

                var updatedUso = await _usoMensualRepository.GetByIdAndTenantAsync(existingUso.Id, tenantId);
                var responseDto = MapToResponseDto(updatedUso ?? existingUso);

                return new BaseResponseDto<UsoMensualDto>
                {
                    Success = true,
                    Data = responseDto,
                    Message = $"Registro de uso mensual para {ConvertirMesATexto(updateDto.Mes)} actualizado correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar uso mensual: {Id} en tenant: {TenantId}", updateDto.Id, tenantId);
                return new BaseResponseDto<UsoMensualDto>
                {
                    Success = false,
                    Message = "No se pudo actualizar el registro.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<object>> GetEstadisticasAsync(int tenantId, string? mesDesde = null, string? mesHasta = null)
        {
            try
            {
                var totalLeads = await _usoMensualRepository.GetTotalLeadsEnPeriodoAsync(tenantId, mesDesde ?? "", mesHasta ?? "");
                var ultimosSeisMeses = await _usoMensualRepository.GetUltimosSeisMesesAsync(tenantId);

                var estadisticas = new
                {
                    TotalLeadsEnPeriodo = totalLeads,
                    PromedioMensual = ultimosSeisMeses.Any() ? ultimosSeisMeses.Average(u => u.LeadsGenerados) : 0,
                    MesConMasLeads = ultimosSeisMeses.OrderByDescending(u => u.LeadsGenerados).FirstOrDefault(),
                    Tendencia = ultimosSeisMeses.Count() >= 2 ?
                        (ultimosSeisMeses.First().LeadsGenerados > ultimosSeisMeses.Skip(1).First().LeadsGenerados ? "Creciente" : "Decreciente") :
                        "Insuficientes datos"
                };

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Data = estadisticas,
                    Message = "Estadísticas obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas para tenant: {TenantId}", tenantId);
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error al cargar estadísticas.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<object>> IncrementarLeadsMesActualAsync(int tenantId)
        {
            try
            {
                var mesActual = DateTime.Now.ToString("yyyy-MM");
                var usoMensual = await _usoMensualRepository.GetByMesAndTenantAsync(mesActual, tenantId);

                if (usoMensual == null)
                {
                    // Crear registro para el mes actual
                    usoMensual = new UsoMensual
                    {
                        IdInmobiliaria = tenantId,
                        Mes = mesActual,
                        LeadsGenerados = 1,
                        CreadoEn = DateTime.UtcNow,
                        ActualizadoEn = DateTime.UtcNow
                    };
                    await _usoMensualRepository.AddAsync(usoMensual);
                }
                else
                {
                    // Incrementar contador
                    usoMensual.LeadsGenerados++;
                    usoMensual.ActualizadoEn = DateTime.UtcNow;
                    await _usoMensualRepository.UpdateAsync(usoMensual);
                }

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Message = $"Contador de leads incrementado para {ConvertirMesATexto(mesActual)}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al incrementar leads del mes actual para tenant: {TenantId}", tenantId);
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error al actualizar contador de leads.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }
    }
}