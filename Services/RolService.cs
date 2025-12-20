using pyreApi.DTOs.Common;
using pyreApi.DTOs.Rol;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
{
    public class RolService : GenericService<Rol>
    {
        public RolService(GenericRepository<Rol> repository) : base(repository)
        {
        }

        public async Task<BaseResponseDto<IEnumerable<RolDto>>> GetAllRolesAsync()
        {
            try
            {
                var roles = await _repository.GetAllAsync();
                var rolDtos = roles.Select(MapToDto);

                return new BaseResponseDto<IEnumerable<RolDto>>
                {
                    Success = true,
                    Data = rolDtos,
                    Message = "Roles obtenidos correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<RolDto>>
                {
                    Success = false,
                    Message = "Error al obtener los roles",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<RolDto>> GetRolByIdAsync(int id)
        {
            try
            {
                var rol = await _repository.GetByIdAsync(id);
                if (rol == null)
                {
                    return new BaseResponseDto<RolDto>
                    {
                        Success = false,
                        Message = "Rol no encontrado"
                    };
                }

                return new BaseResponseDto<RolDto>
                {
                    Success = true,
                    Data = MapToDto(rol),
                    Message = "Rol encontrado"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<RolDto>
                {
                    Success = false,
                    Message = "Error al buscar el rol",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<RolDto>> CreateRolAsync(CreateRolDto createDto)
        {
            try
            {
                var rol = MapFromCreateDto(createDto);
                var result = await _repository.AddAsync(rol);

                return new BaseResponseDto<RolDto>
                {
                    Success = true,
                    Data = MapToDto(result),
                    Message = "Rol creado correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<RolDto>
                {
                    Success = false,
                    Message = "Error al crear el rol",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<BaseResponseDto<RolDto>> UpdateRolAsync(UpdateRolDto updateDto)
        {
            try
            {
                var existingRol = await _repository.GetByIdAsync(updateDto.IdRol);
                if (existingRol == null)
                {
                    return new BaseResponseDto<RolDto>
                    {
                        Success = false,
                        Message = "Rol no encontrado"
                    };
                }

                MapFromUpdateDto(updateDto, existingRol);
                await _repository.UpdateAsync(existingRol);

                return new BaseResponseDto<RolDto>
                {
                    Success = true,
                    Data = MapToDto(existingRol),
                    Message = "Rol actualizado correctamente"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<RolDto>
                {
                    Success = false,
                    Message = "Error al actualizar el rol",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private RolDto MapToDto(Rol rol)
        {
            return new RolDto
            {
                IdRol = rol.Id,
                NombreRol = rol.NombreRol
            };
        }

        private Rol MapFromCreateDto(CreateRolDto createDto)
        {
            return new Rol
            {
                NombreRol = createDto.NombreRol
            };
        }

        private void MapFromUpdateDto(UpdateRolDto updateDto, Rol rol)
        {
            rol.NombreRol = updateDto.NombreRol;
        }
    }
}
