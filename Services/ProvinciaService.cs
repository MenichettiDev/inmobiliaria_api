using inmobiliariaApi.DTOs.Common;
using inmobiliariaApi.DTOs.Provincia;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;

namespace inmobiliariaApi.Services
{
    public class ProvinciaService
    {
        private readonly ProvinciaRepository _repo;
        private readonly ILogger<ProvinciaService> _logger;

        public ProvinciaService(ProvinciaRepository repo, ILogger<ProvinciaService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        private static ProvinciaDto Map(Provincia p) => new()
        {
            Id = p.Id,
            Nombre = p.Nombre,
            CodigoIndec = p.CodigoIndec,
            Activo = p.Activo
        };

        public async Task<BaseResponseDto<IEnumerable<ProvinciaDto>>> GetAllAsync(bool soloActivas = true)
        {
            try
            {
                var data = soloActivas
                    ? await _repo.GetAllActivasAsync()
                    : await _repo.GetAllAsync();

                return new BaseResponseDto<IEnumerable<ProvinciaDto>>
                {
                    Success = true,
                    Data = data.Select(Map),
                    Message = "Provincias obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar provincias");
                return new BaseResponseDto<IEnumerable<ProvinciaDto>> { Success = false, Message = "Error interno." };
            }
        }

        public async Task<BaseResponseDto<ProvinciaDto>> GetByIdAsync(long id)
        {
            try
            {
                var p = await _repo.GetByIdLongAsync(id);
                if (p == null)
                    return new BaseResponseDto<ProvinciaDto> { Success = false, Message = "Provincia no encontrada." };

                return new BaseResponseDto<ProvinciaDto> { Success = true, Data = Map(p) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener provincia {Id}", id);
                return new BaseResponseDto<ProvinciaDto> { Success = false, Message = "Error interno." };
            }
        }

        public async Task<BaseResponseDto<ProvinciaDto>> CreateAsync(CreateProvinciaDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                    return new BaseResponseDto<ProvinciaDto> { Success = false, Message = "El nombre es requerido." };

                if (await _repo.ExisteNombreAsync(dto.Nombre))
                    return new BaseResponseDto<ProvinciaDto> { Success = false, Message = "Ya existe una provincia con ese nombre." };

                var entity = new Provincia
                {
                    Nombre = dto.Nombre.Trim(),
                    CodigoIndec = dto.CodigoIndec?.Trim(),
                    Activo = true
                };

                var created = await _repo.AddAsync(entity);
                return new BaseResponseDto<ProvinciaDto> { Success = true, Data = Map(created), Message = "Provincia creada correctamente." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear provincia");
                return new BaseResponseDto<ProvinciaDto> { Success = false, Message = "No se pudo crear la provincia." };
            }
        }

        public async Task<BaseResponseDto<ProvinciaDto>> UpdateAsync(UpdateProvinciaDto dto)
        {
            try
            {
                var existing = await _repo.GetByIdLongAsync(dto.Id);
                if (existing == null)
                    return new BaseResponseDto<ProvinciaDto> { Success = false, Message = "Provincia no encontrada." };

                if (!string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    if (await _repo.ExisteNombreAsync(dto.Nombre, dto.Id))
                        return new BaseResponseDto<ProvinciaDto> { Success = false, Message = "Ya existe otra provincia con ese nombre." };

                    existing.Nombre = dto.Nombre.Trim();
                }

                if (dto.CodigoIndec != null) existing.CodigoIndec = dto.CodigoIndec.Trim();
                if (dto.Activo.HasValue) existing.Activo = dto.Activo.Value;

                await _repo.UpdateAsync(existing);
                return new BaseResponseDto<ProvinciaDto> { Success = true, Data = Map(existing), Message = "Provincia actualizada correctamente." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar provincia {Id}", dto.Id);
                return new BaseResponseDto<ProvinciaDto> { Success = false, Message = "No se pudo actualizar la provincia." };
            }
        }

        public async Task<BaseResponseDto<object>> DeleteAsync(long id)
        {
            try
            {
                var existing = await _repo.GetByIdLongAsync(id);
                if (existing == null)
                    return new BaseResponseDto<object> { Success = false, Message = "Provincia no encontrada." };

                existing.Activo = false;
                await _repo.UpdateAsync(existing);
                return new BaseResponseDto<object> { Success = true, Message = "Provincia desactivada correctamente." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar provincia {Id}", id);
                return new BaseResponseDto<object> { Success = false, Message = "No se pudo eliminar la provincia." };
            }
        }
    }
}
