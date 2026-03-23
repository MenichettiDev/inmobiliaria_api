using inmobiliariaApi.DTOs.Common;
using inmobiliariaApi.DTOs.Localidad;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;

namespace inmobiliariaApi.Services
{
    public class LocalidadService
    {
        private readonly LocalidadRepository _repo;
        private readonly ProvinciaRepository _provinciaRepo;
        private readonly ILogger<LocalidadService> _logger;

        public LocalidadService(LocalidadRepository repo, ProvinciaRepository provinciaRepo, ILogger<LocalidadService> logger)
        {
            _repo = repo;
            _provinciaRepo = provinciaRepo;
            _logger = logger;
        }

        private static LocalidadDto Map(Localidad l) => new()
        {
            Id = l.Id,
            IdProvincia = l.IdProvincia,
            ProvinciaNombre = l.Provincia?.Nombre ?? string.Empty,
            Nombre = l.Nombre,
            CodigoPostal = l.CodigoPostal,
            Latitud = l.Latitud,
            Longitud = l.Longitud,
            Municipio = l.Municipio,
            IdPartido = l.IdPartido
        };

        public async Task<BaseResponseDto<IEnumerable<LocalidadDto>>> GetAllAsync()
        {
            try
            {
                var data = await _repo.GetAllWithProvinciaAsync();
                return new BaseResponseDto<IEnumerable<LocalidadDto>>
                {
                    Success = true,
                    Data = data.Select(Map),
                    Message = "Localidades obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar localidades");
                return new BaseResponseDto<IEnumerable<LocalidadDto>> { Success = false, Message = "Error interno." };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<LocalidadDto>>> GetByProvinciaAsync(long idProvincia)
        {
            try
            {
                var data = await _repo.GetByProvinciaAsync(idProvincia);
                return new BaseResponseDto<IEnumerable<LocalidadDto>>
                {
                    Success = true,
                    Data = data.Select(Map),
                    Message = "Localidades obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar localidades de provincia {Id}", idProvincia);
                return new BaseResponseDto<IEnumerable<LocalidadDto>> { Success = false, Message = "Error interno." };
            }
        }

        public async Task<BaseResponseDto<LocalidadDto>> GetByIdAsync(long id)
        {
            try
            {
                var l = await _repo.GetByIdWithProvinciaAsync(id);
                if (l == null)
                    return new BaseResponseDto<LocalidadDto> { Success = false, Message = "Localidad no encontrada." };

                return new BaseResponseDto<LocalidadDto> { Success = true, Data = Map(l) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener localidad {Id}", id);
                return new BaseResponseDto<LocalidadDto> { Success = false, Message = "Error interno." };
            }
        }

        public async Task<BaseResponseDto<LocalidadDto>> CreateAsync(CreateLocalidadDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                    return new BaseResponseDto<LocalidadDto> { Success = false, Message = "El nombre es requerido." };

                var provincia = await _provinciaRepo.GetByIdLongAsync(dto.IdProvincia);
                if (provincia == null)
                    return new BaseResponseDto<LocalidadDto> { Success = false, Message = "Provincia no encontrada." };

                if (await _repo.ExisteEnProvinciaAsync(dto.Nombre, dto.IdProvincia))
                    return new BaseResponseDto<LocalidadDto> { Success = false, Message = "Ya existe una localidad con ese nombre en la provincia." };

                var entity = new Localidad
                {
                    IdProvincia = dto.IdProvincia,
                    Nombre = dto.Nombre.Trim(),
                    CodigoPostal = dto.CodigoPostal?.Trim(),
                    Latitud = dto.Latitud?.Trim(),
                    Longitud = dto.Longitud?.Trim(),
                    Municipio = dto.Municipio?.Trim(),
                    IdPartido = dto.IdPartido
                };

                await _repo.AddAsync(entity);
                var created = await _repo.GetByIdWithProvinciaAsync(entity.Id);
                return new BaseResponseDto<LocalidadDto> { Success = true, Data = Map(created!), Message = "Localidad creada correctamente." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear localidad");
                return new BaseResponseDto<LocalidadDto> { Success = false, Message = "No se pudo crear la localidad." };
            }
        }

        public async Task<BaseResponseDto<LocalidadDto>> UpdateAsync(UpdateLocalidadDto dto)
        {
            try
            {
                var existing = await _repo.GetByIdWithProvinciaAsync(dto.Id);
                if (existing == null)
                    return new BaseResponseDto<LocalidadDto> { Success = false, Message = "Localidad no encontrada." };

                if (dto.IdProvincia.HasValue && dto.IdProvincia.Value != existing.IdProvincia)
                {
                    var provincia = await _provinciaRepo.GetByIdLongAsync(dto.IdProvincia.Value);
                    if (provincia == null)
                        return new BaseResponseDto<LocalidadDto> { Success = false, Message = "Provincia no encontrada." };
                    existing.IdProvincia = dto.IdProvincia.Value;
                    existing.Provincia = provincia;
                }

                if (!string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    if (await _repo.ExisteEnProvinciaAsync(dto.Nombre, existing.IdProvincia, dto.Id))
                        return new BaseResponseDto<LocalidadDto> { Success = false, Message = "Ya existe otra localidad con ese nombre en la provincia." };

                    existing.Nombre = dto.Nombre.Trim();
                }

                if (dto.CodigoPostal != null) existing.CodigoPostal = dto.CodigoPostal.Trim();
                if (dto.Latitud != null) existing.Latitud = dto.Latitud.Trim();
                if (dto.Longitud != null) existing.Longitud = dto.Longitud.Trim();
                if (dto.Municipio != null) existing.Municipio = dto.Municipio.Trim();
                if (dto.IdPartido.HasValue) existing.IdPartido = dto.IdPartido.Value;

                await _repo.UpdateAsync(existing);
                return new BaseResponseDto<LocalidadDto> { Success = true, Data = Map(existing), Message = "Localidad actualizada correctamente." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar localidad {Id}", dto.Id);
                return new BaseResponseDto<LocalidadDto> { Success = false, Message = "No se pudo actualizar la localidad." };
            }
        }

        public async Task<BaseResponseDto<object>> DeleteAsync(long id)
        {
            try
            {
                var existing = await _repo.GetByIdLongAsync(id);
                if (existing == null)
                    return new BaseResponseDto<object> { Success = false, Message = "Localidad no encontrada." };

                await _repo.DeleteLongAsync(id);
                return new BaseResponseDto<object> { Success = true, Message = "Localidad eliminada correctamente." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar localidad {Id}", id);
                return new BaseResponseDto<object> { Success = false, Message = "No se pudo eliminar la localidad." };
            }
        }
    }
}
