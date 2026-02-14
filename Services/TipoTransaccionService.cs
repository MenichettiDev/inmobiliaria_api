using Microsoft.Extensions.Logging;
using inmobiliariaApi.DTOs.TipoTransaccion;
using inmobiliariaApi.DTOs.Common;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;

namespace inmobiliariaApi.Services
{
    public class TipoTransaccionService : GenericService<TipoTransaccion>
    {
        private readonly TipoTransaccionRepository _repo;
        private readonly ILogger<TipoTransaccionService> _logger;

        public TipoTransaccionService(TipoTransaccionRepository repo, ILogger<TipoTransaccionService> logger) : base(repo)
        {
            _repo = repo;
            _logger = logger;
        }

        private TipoTransaccionResponseDto MapToDto(TipoTransaccion t) =>
            new TipoTransaccionResponseDto { Id = t.Id, Descripcion = t.Descripcion };

        public async Task<BaseResponseDto<List<TipoTransaccionResponseDto>>> GetAllAsync(int tenantId)
        {
            try
            {
                var list = await _repo.GetAllByTenantAsync(tenantId);
                var dto = list.Select(MapToDto).ToList();
                return new BaseResponseDto<List<TipoTransaccionResponseDto>> { Success = true, Data = dto };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipos de transacción tenant {TenantId}", tenantId);
                return new BaseResponseDto<List<TipoTransaccionResponseDto>> { Success = false, Message = "Error interno.", Errors = new List<string> { "Error interno del servidor." } };
            }
        }

        public async Task<BaseResponseDto<TipoTransaccionResponseDto>> GetByIdAsync(int id, int tenantId)
        {
            try
            {
                var t = await _repo.GetByIdAndTenantAsync(id, tenantId);
                if (t == null) return new BaseResponseDto<TipoTransaccionResponseDto> { Success = false, Message = "No encontrado." };
                return new BaseResponseDto<TipoTransaccionResponseDto> { Success = true, Data = MapToDto(t) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipo transacción {Id}", id);
                return new BaseResponseDto<TipoTransaccionResponseDto> { Success = false, Message = "Error interno.", Errors = new List<string> { "Error interno del servidor." } };
            }
        }

        public async Task<BaseResponseDto<TipoTransaccionResponseDto>> CreateAsync(CreateTipoTransaccionDto dto)
        {
            try
            {
                var entity = new TipoTransaccion
                {
                    Descripcion = dto.Descripcion?.Trim() ?? string.Empty,
                };

                var res = await _repo.AddAsync(entity);
                return new BaseResponseDto<TipoTransaccionResponseDto> { Success = true, Data = MapToDto(res) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear tipo transacción");
                return new BaseResponseDto<TipoTransaccionResponseDto> { Success = false, Message = "No se pudo crear.", Errors = new List<string> { "Error interno del servidor." } };
            }
        }

        public async Task<BaseResponseDto<TipoTransaccionResponseDto>> UpdateAsync(UpdateTipoTransaccionDto dto, int tenantId)
        {
            try
            {
                var existing = await _repo.GetByIdAndTenantAsync(dto.Id, tenantId);
                if (existing == null) return new BaseResponseDto<TipoTransaccionResponseDto> { Success = false, Message = "No encontrado o fuera de su organización." };

                if (!string.IsNullOrWhiteSpace(dto.Descripcion)) existing.Descripcion = dto.Descripcion.Trim();

                await _repo.UpdateAsync(existing);
                var updated = await _repo.GetByIdAndTenantAsync(existing.Id, tenantId);
                return new BaseResponseDto<TipoTransaccionResponseDto> { Success = true, Data = MapToDto(updated ?? existing) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar tipo transacción {Id}", dto.Id);
                return new BaseResponseDto<TipoTransaccionResponseDto> { Success = false, Message = "No se pudo actualizar.", Errors = new List<string> { "Error interno del servidor." } };
            }
        }

        public async Task<BaseResponseDto<object>> DeleteAsync(int id, int tenantId)
        {
            try
            {
                var existing = await _repo.GetByIdAndTenantAsync(id, tenantId);
                if (existing == null) return new BaseResponseDto<object> { Success = false, Message = "No encontrado o fuera de su organización." };

                await _repo.DeleteAsync(existing.Id);
                return new BaseResponseDto<object> { Success = true, Message = "Eliminado correctamente." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar tipo transacción {Id}", id);
                return new BaseResponseDto<object> { Success = false, Message = "No se pudo eliminar.", Errors = new List<string> { "Error interno del servidor." } };
            }
        }

        public async Task<BaseResponseDto<List<TipoTransaccionComboDto>>> GetComboAsync(int tenantId)
        {
            try
            {
                var list = await _repo.GetComboAsync(tenantId);
                var dto = list.Select(t => new TipoTransaccionComboDto { Id = t.Id, Descripcion = t.Descripcion }).ToList();
                return new BaseResponseDto<List<TipoTransaccionComboDto>> { Success = true, Data = dto };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener combo tipos transacción tenant {TenantId}", tenantId);
                return new BaseResponseDto<List<TipoTransaccionComboDto>> { Success = false, Message = "No se pudo obtener combo.", Errors = new List<string> { "Error interno del servidor." } };
            }
        }
    }
}
