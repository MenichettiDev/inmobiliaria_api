using Microsoft.Extensions.Logging;
using inmobiliariaApi.DTOs.Cliente;
using inmobiliariaApi.DTOs.Common;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;

namespace inmobiliariaApi.Services
{
    // ...new file...
    public class ClienteService
    {
        private readonly ClienteRepository _repo;
        private readonly ILogger<ClienteService> _logger;

        public ClienteService(ClienteRepository repo, ILogger<ClienteService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        private ClienteResponseDto Map(Cliente c) =>
            new ClienteResponseDto
            {
                Id = c.Id,
                IdInmobiliaria = c.IdInmobiliaria,
                NombreCompleto = c.NombreCompleto,
                Dni = c.Dni,
                Email = c.Email,
                Telefono = c.Telefono,
                Activo = c.Activo,
                CreadoEn = c.CreadoEn,
                ActualizadoEn = c.ActualizadoEn
            };

        public async Task<BaseResponseDto<PaginatedResponseDto<ClienteResponseDto>>> GetAllPagedAsync(int page, int pageSize, int tenantId, string? nombre = null, bool? activo = null)
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;

                var (data, total) = await _repo.GetAllPagedByTenantAsync(page, pageSize, tenantId, nombre, activo);
                var dtos = data.Select(Map).ToList();
                var totalPages = (int)Math.Ceiling((double)total / pageSize);

                var pag = new PaginatedResponseDto<ClienteResponseDto>
                {
                    Data = dtos,
                    Page = page,
                    PageSize = pageSize,
                    TotalRecords = total,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };

                return new BaseResponseDto<PaginatedResponseDto<ClienteResponseDto>> { Success = true, Data = pag };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar clientes tenant {Tenant}", tenantId);
                return new BaseResponseDto<PaginatedResponseDto<ClienteResponseDto>> { Success = false, Message = "Error interno." };
            }
        }

        public async Task<BaseResponseDto<ClienteResponseDto>> GetByIdAsync(int id, int tenantId)
        {
            try
            {
                var c = await _repo.GetByIdAndTenantAsync(id, tenantId);
                if (c == null) return new BaseResponseDto<ClienteResponseDto> { Success = false, Message = "Cliente no encontrado." };
                return new BaseResponseDto<ClienteResponseDto> { Success = true, Data = Map(c) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cliente {Id} tenant {Tenant}", id, tenantId);
                return new BaseResponseDto<ClienteResponseDto> { Success = false, Message = "Error interno." };
            }
        }

        public async Task<BaseResponseDto<ClienteResponseDto>> CreateAsync(CreateClienteDto dto)
        {
            try
            {
                var model = new Cliente
                {
                    NombreCompleto = dto.NombreCompleto.Trim(),
                    Dni = dto.Dni,
                    Email = dto.Email,
                    Telefono = dto.Telefono,
                    IdInmobiliaria = dto.IdInmobiliaria,
                    Activo = true,
                    CreadoEn = DateTime.UtcNow,
                    ActualizadoEn = DateTime.UtcNow
                };

                var created = await _repo.AddAsync(model);
                return new BaseResponseDto<ClienteResponseDto> { Success = true, Data = Map(created) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cliente en tenant {Tenant}", dto.IdInmobiliaria);
                return new BaseResponseDto<ClienteResponseDto> { Success = false, Message = "No se pudo crear el cliente." };
            }
        }

        public async Task<BaseResponseDto<ClienteResponseDto>> UpdateAsync(UpdateClienteDto dto, int tenantId)
        {
            try
            {
                var existing = await _repo.GetByIdAndTenantAsync(dto.Id, tenantId);
                if (existing == null) return new BaseResponseDto<ClienteResponseDto> { Success = false, Message = "Cliente no encontrado." };

                if (!string.IsNullOrWhiteSpace(dto.NombreCompleto)) existing.NombreCompleto = dto.NombreCompleto.Trim();
                if (dto.Dni != null) existing.Dni = dto.Dni;
                if (dto.Email != null) existing.Email = dto.Email;
                if (dto.Telefono != null) existing.Telefono = dto.Telefono;
                if (dto.Activo.HasValue) existing.Activo = dto.Activo.Value;

                existing.ActualizadoEn = DateTime.UtcNow;

                await _repo.UpdateAsync(existing);
                return new BaseResponseDto<ClienteResponseDto> { Success = true, Data = Map(existing) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cliente {Id} tenant {Tenant}", dto.Id, tenantId);
                return new BaseResponseDto<ClienteResponseDto> { Success = false, Message = "No se pudo actualizar el cliente." };
            }
        }

        public async Task<BaseResponseDto<object>> DeleteAsync(int id, int tenantId)
        {
            try
            {
                var existing = await _repo.GetByIdAndTenantAsync(id, tenantId);
                if (existing == null) return new BaseResponseDto<object> { Success = false, Message = "Cliente no encontrado." };

                if (!existing.Activo) return new BaseResponseDto<object> { Success = false, Message = "Cliente ya inactivo." };

                existing.Activo = false;
                existing.ActualizadoEn = DateTime.UtcNow;
                await _repo.UpdateAsync(existing);

                return new BaseResponseDto<object> { Success = true, Message = "Cliente desactivado correctamente." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cliente {Id} tenant {Tenant}", id, tenantId);
                return new BaseResponseDto<object> { Success = false, Message = "No se pudo eliminar el cliente." };
            }
        }
    }
}
