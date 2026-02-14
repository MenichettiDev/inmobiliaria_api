using Microsoft.Extensions.Logging;
using inmobiliariaApi.DTOs.Common;
using inmobiliariaApi.DTOs.TransaccionHistorial;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;

namespace inmobiliariaApi.Services
{
    public class TransaccionHistorialService : GenericService<TransaccionHistorial>
    {
        private readonly TransaccionHistorialRepository _repo;
        private readonly ILogger<TransaccionHistorialService> _logger;

        public TransaccionHistorialService(TransaccionHistorialRepository repo, ILogger<TransaccionHistorialService> logger)
            : base(repo)
        {
            _repo = repo;
            _logger = logger;
        }

        private TransaccionHistorialResponseDto MapToResponseDto(TransaccionHistorial t)
        {
            return new TransaccionHistorialResponseDto
            {
                Id = t.Id,
                IdCliente = t.IdCliente,
                ClienteNombre = t.Cliente?.NombreCompleto,
                IdPropiedad = t.IdPropiedad,
                PropiedadTitulo = t.Propiedad?.Titulo,
                IdAgente = t.IdAgente,
                AgenteNombre = t.Agente?.Nombre,
                IdTipoTransaccion = t.IdTipoTransaccion,
                TipoTransaccionDescripcion = t.TipoTransaccion?.Descripcion,
                Precio = t.Precio,
                FechaOperacion = t.FechaOperacion,
                Observaciones = t.Observaciones,
                CreadoEn = t.CreadoEn
            };
        }

        public async Task<BaseResponseDto<PaginatedResponseDto<TransaccionHistorialResponseDto>>> GetAllPaginatedAsync(
            int page, int pageSize, int tenantId, int? clienteId = null, int? agenteId = null, byte? tipoTransaccion = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;

                var (data, total) = await _repo.GetAllWithDetailsPagedByTenantAsync(page, pageSize, tenantId, clienteId, agenteId, tipoTransaccion, fechaDesde, fechaHasta);
                var dto = data.Select(MapToResponseDto).ToList();
                var totalPages = (int)Math.Ceiling((double)total / pageSize);

                var pag = new PaginatedResponseDto<TransaccionHistorialResponseDto>
                {
                    Data = dto,
                    Page = page,
                    PageSize = pageSize,
                    TotalRecords = total,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };

                return new BaseResponseDto<PaginatedResponseDto<TransaccionHistorialResponseDto>>
                {
                    Success = true,
                    Data = pag
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transacciones paginadas tenant {TenantId}", tenantId);
                return new BaseResponseDto<PaginatedResponseDto<TransaccionHistorialResponseDto>>
                {
                    Success = false,
                    Message = "Error interno al obtener transacciones.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<TransaccionHistorialResponseDto>> GetByIdAndTenantAsync(int id, int tenantId)
        {
            try
            {
                var t = await _repo.GetByIdWithDetailsAndTenantAsync(id, tenantId);
                if (t == null)
                    return new BaseResponseDto<TransaccionHistorialResponseDto> { Success = false, Message = $"No se encontró transacción {id} en su organización." };

                return new BaseResponseDto<TransaccionHistorialResponseDto> { Success = true, Data = MapToResponseDto(t) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener transacción {Id} tenant {TenantId}", id, tenantId);
                return new BaseResponseDto<TransaccionHistorialResponseDto> { Success = false, Message = "Error al obtener la transacción.", Errors = new List<string> { "Error interno del servidor." } };
            }
        }

        public async Task<BaseResponseDto<TransaccionHistorialResponseDto>> CreateAsync(CreateTransaccionHistorialDto dto)
        {
            try
            {
                var entity = new TransaccionHistorial
                {
                    IdCliente = dto.IdCliente,
                    IdPropiedad = dto.IdPropiedad,
                    IdAgente = dto.IdAgente,
                    IdTipoTransaccion = dto.IdTipoTransaccion,
                    Precio = dto.Precio,
                    FechaOperacion = dto.FechaOperacion,
                    Observaciones = dto.Observaciones,
                    IdInmobiliaria = dto.IdInmobiliaria,
                    CreadoEn = DateTime.UtcNow
                };

                var res = await _repo.AddAsync(entity);
                var withDetails = await _repo.GetByIdWithDetailsAndTenantAsync(res.Id, res.IdInmobiliaria ?? 0);
                return new BaseResponseDto<TransaccionHistorialResponseDto> { Success = true, Data = MapToResponseDto(withDetails ?? res) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear transacción");
                return new BaseResponseDto<TransaccionHistorialResponseDto> { Success = false, Message = "No se pudo crear la transacción.", Errors = new List<string> { "Error interno del servidor." } };
            }
        }

        public async Task<BaseResponseDto<TransaccionHistorialResponseDto>> UpdateAsync(UpdateTransaccionHistorialDto dto, int tenantId)
        {
            try
            {
                var existing = await _repo.GetByIdWithDetailsAndTenantAsync(dto.Id, tenantId);
                if (existing == null)
                    return new BaseResponseDto<TransaccionHistorialResponseDto> { Success = false, Message = "Transacción no encontrada en su organización." };

                if (dto.IdCliente.HasValue) existing.IdCliente = dto.IdCliente;
                if (dto.IdPropiedad.HasValue) existing.IdPropiedad = dto.IdPropiedad;
                if (dto.IdAgente.HasValue) existing.IdAgente = dto.IdAgente;
                if (dto.IdTipoTransaccion.HasValue) existing.IdTipoTransaccion = dto.IdTipoTransaccion;
                if (dto.Precio.HasValue) existing.Precio = dto.Precio;
                if (dto.FechaOperacion.HasValue) existing.FechaOperacion = dto.FechaOperacion;
                if (dto.Observaciones != null) existing.Observaciones = dto.Observaciones;

                // Mantener tenant
                existing.IdInmobiliaria = tenantId;

                await _repo.UpdateAsync(existing);
                var updated = await _repo.GetByIdWithDetailsAndTenantAsync(existing.Id, tenantId);
                return new BaseResponseDto<TransaccionHistorialResponseDto> { Success = true, Data = MapToResponseDto(updated ?? existing) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar transacción {Id}", dto.Id);
                return new BaseResponseDto<TransaccionHistorialResponseDto> { Success = false, Message = "No se pudo actualizar la transacción.", Errors = new List<string> { "Error interno del servidor." } };
            }
        }

        public async Task<BaseResponseDto<object>> DeleteAsync(int id, int tenantId)
        {
            try
            {
                var existing = await _repo.GetByIdWithDetailsAndTenantAsync(id, tenantId);
                if (existing == null)
                    return new BaseResponseDto<object> { Success = false, Message = "Transacción no encontrada en su organización." };

                await _repo.DeleteAsync(existing.Id);

                return new BaseResponseDto<object> { Success = true, Message = "Transacción eliminada correctamente." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar transacción {Id}", id);
                return new BaseResponseDto<object> { Success = false, Message = "No se pudo eliminar la transacción.", Errors = new List<string> { "Error interno del servidor." } };
            }
        }

        public async Task<BaseResponseDto<List<TransaccionHistorialComboDto>>> GetComboAsync(int tenantId)
        {
            try
            {
                var list = await _repo.GetTransaccionesComboAsync(tenantId);
                var dto = list.Select(t => new TransaccionHistorialComboDto
                {
                    Id = t.Id,
                    FechaOperacion = t.FechaOperacion,
                    Precio = t.Precio,
                    Observaciones = t.Observaciones
                }).ToList();

                return new BaseResponseDto<List<TransaccionHistorialComboDto>> { Success = true, Data = dto };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener combo transacciones tenant {TenantId}", tenantId);
                return new BaseResponseDto<List<TransaccionHistorialComboDto>> { Success = false, Message = "No se pudo obtener combo.", Errors = new List<string> { "Error interno del servidor." } };
            }
        }
    }
}
