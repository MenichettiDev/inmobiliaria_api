using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using inmobiliariaApi.DTOs.Common;
using inmobiliariaApi.DTOs.ImagenPropiedad;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;
using System.IO;
using System.Text.RegularExpressions;

namespace inmobiliariaApi.Services
{
    public class ImagenPropiedadService : GenericService<ImagenPropiedad>
    {
        private readonly ImagenPropiedadRepository _imagenRepository;
        private readonly ILogger<ImagenPropiedadService> _logger;
        private readonly IWebHostEnvironment _env; // nueva dependencia

        public ImagenPropiedadService(ImagenPropiedadRepository imagenRepository, ILogger<ImagenPropiedadService> logger, IWebHostEnvironment env)
            : base(imagenRepository)
        {
            _imagenRepository = imagenRepository;
            _logger = logger;
            _env = env;
        }

        private ImagenPropiedadDto MapToResponseDto(ImagenPropiedad imagen)
        {
            return new ImagenPropiedadDto
            {
                Id = imagen.Id,
                IdPropiedad = imagen.IdPropiedad,
                Url = imagen.Url,
                Orden = imagen.Orden,
                CreadoEn = imagen.CreadoEn,
                PropiedadTitulo = imagen.Propiedad?.Titulo
            };
        }

        public async Task<BaseResponseDto<IEnumerable<ImagenPropiedadDto>>> GetImagenesByPropiedadAsync(int propiedadId, int tenantId)
        {
            try
            {
                // Validar que la propiedad pertenezca al tenant
                var propiedadExists = await _imagenRepository.ValidatePropiedadInTenantAsync(propiedadId, tenantId);
                if (!propiedadExists)
                {
                    return new BaseResponseDto<IEnumerable<ImagenPropiedadDto>>
                    {
                        Success = false,
                        Message = "Propiedad no encontrada en su organización."
                    };
                }

                var imagenes = await _imagenRepository.GetByPropiedadAndTenantAsync(propiedadId, tenantId);
                var imagenesDto = imagenes.Select(MapToResponseDto).ToList();

                return new BaseResponseDto<IEnumerable<ImagenPropiedadDto>>
                {
                    Success = true,
                    Data = imagenesDto,
                    Message = "Imágenes obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener imágenes de propiedad: {PropiedadId} en tenant: {TenantId}", propiedadId, tenantId);
                return new BaseResponseDto<IEnumerable<ImagenPropiedadDto>>
                {
                    Success = false,
                    Message = "Error al cargar imágenes de la propiedad.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<PaginatedResponseDto<ImagenPropiedadDto>>> GetImagenesPaginatedAsync(
            int page, int pageSize, int tenantId, int? propiedadId = null)
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;

                var (imagenes, totalRecords) = await _imagenRepository.GetPagedByTenantAsync(
                    page, pageSize, tenantId, propiedadId);

                var imagenesDto = imagenes.Select(MapToResponseDto).ToList();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var paginatedResponse = new PaginatedResponseDto<ImagenPropiedadDto>
                {
                    Data = imagenesDto,
                    Page = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };

                return new BaseResponseDto<PaginatedResponseDto<ImagenPropiedadDto>>
                {
                    Success = true,
                    Data = paginatedResponse,
                    Message = "Imágenes obtenidas correctamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener imágenes paginadas para tenant: {TenantId}", tenantId);
                return new BaseResponseDto<PaginatedResponseDto<ImagenPropiedadDto>>
                {
                    Success = false,
                    Message = "No se pudieron cargar las imágenes.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<ImagenPropiedadDto>> GetByIdAndTenantAsync(int id, int tenantId)
        {
            try
            {
                var imagen = await _imagenRepository.GetByIdAndTenantAsync(id, tenantId);
                if (imagen == null)
                {
                    return new BaseResponseDto<ImagenPropiedadDto>
                    {
                        Success = false,
                        Message = "Imagen no encontrada en su organización."
                    };
                }

                var imagenDto = MapToResponseDto(imagen);
                return new BaseResponseDto<ImagenPropiedadDto>
                {
                    Success = true,
                    Data = imagenDto,
                    Message = "Imagen encontrada"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener imagen por ID: {Id} en tenant: {TenantId}", id, tenantId);
                return new BaseResponseDto<ImagenPropiedadDto>
                {
                    Success = false,
                    Message = "Error al buscar la imagen.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<ImagenPropiedadDto>> CreateImagenAsync(CreateImagenPropiedadDto createDto, int tenantId)
        {
            try
            {
                _logger.LogInformation("Creando imagen para propiedad: {PropiedadId} en tenant: {TenantId}", createDto.IdPropiedad, tenantId);

                // Validar que la propiedad pertenezca al tenant
                var propiedadExists = await _imagenRepository.ValidatePropiedadInTenantAsync(createDto.IdPropiedad, tenantId);
                if (!propiedadExists)
                {
                    return new BaseResponseDto<ImagenPropiedadDto>
                    {
                        Success = false,
                        Message = "Propiedad no encontrada en su organización."
                    };
                }

                // Validar límite de imágenes por propiedad (máximo 20)
                var countImagenes = await _imagenRepository.GetCountByPropiedadAsync(createDto.IdPropiedad);
                if (countImagenes >= 20)
                {
                    return new BaseResponseDto<ImagenPropiedadDto>
                    {
                        Success = false,
                        Message = "Se ha alcanzado el límite máximo de 20 imágenes por propiedad."
                    };
                }

                // Si la URL viene como data:image/...;base64,... -> guardarla en disco y reemplazar createDto.Url por la ruta pública
                if (!string.IsNullOrWhiteSpace(createDto.Url) && createDto.Url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        createDto.Url = await SaveBase64ImageAsync(createDto.Url, tenantId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al guardar imagen base64 para propiedad {PropiedadId}", createDto.IdPropiedad);
                        return new BaseResponseDto<ImagenPropiedadDto>
                        {
                            Success = false,
                            Message = "No se pudo guardar la imagen enviada en base64."
                        };
                    }
                }

                // Si no se especifica orden, asignar el siguiente disponible
                if (createDto.Orden == 0)
                {
                    var maxOrden = await _imagenRepository.GetMaxOrdenByPropiedadAsync(createDto.IdPropiedad);
                    createDto.Orden = maxOrden + 1;
                }

                var imagen = new ImagenPropiedad
                {
                    IdPropiedad = createDto.IdPropiedad,
                    Url = createDto.Url.Trim(),
                    Orden = createDto.Orden,
                    CreadoEn = DateTime.UtcNow
                };

                var result = await _imagenRepository.AddAsync(imagen);

                _logger.LogInformation("Imagen creada exitosamente con ID: {Id}", result.Id);

                var imagenConDetalles = await _imagenRepository.GetByIdWithDetailsAsync(result.Id);
                var responseDto = MapToResponseDto(imagenConDetalles ?? result);

                return new BaseResponseDto<ImagenPropiedadDto>
                {
                    Success = true,
                    Data = responseDto,
                    Message = "Imagen agregada correctamente a la propiedad."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear imagen para propiedad: {PropiedadId} en tenant: {TenantId}", createDto?.IdPropiedad, tenantId);
                return new BaseResponseDto<ImagenPropiedadDto>
                {
                    Success = false,
                    Message = "No se pudo agregar la imagen a la propiedad.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        // Guarda una imagen enviada como data URL base64 y devuelve la ruta pública (/uploads/...)
        private async Task<string> SaveBase64ImageAsync(string dataUrl, int tenantId)
        {
            var m = Regex.Match(dataUrl, @"data:(?<mime>[\w\/\-\+\.]+);base64,(?<data>.+)");
            if (!m.Success) throw new InvalidDataException("Formato de data URL inválido.");

            var mime = m.Groups["mime"].Value.ToLowerInvariant();
            var base64 = m.Groups["data"].Value;
            string ext = mime switch
            {
                "image/png" => ".png",
                "image/jpeg" => ".jpg",
                "image/jpg" => ".jpg",
                "image/gif" => ".gif",
                _ => ".bin"
            };

            var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "propiedades", tenantId.ToString());
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            var bytes = Convert.FromBase64String(base64);
            await File.WriteAllBytesAsync(filePath, bytes);

            // Ruta pública relativa
            var publicUrl = $"/uploads/propiedades/{tenantId}/{fileName}";
            return publicUrl;
        }

        // Borra un archivo guardado si existe (acepta rutas públicas /uploads/...)
        public void DeleteFileByUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            try
            {
                var relative = url.StartsWith("/") ? url.Substring(1) : url;
                var possible = Path.Combine(_env.WebRootPath ?? "wwwroot", relative.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(possible)) File.Delete(possible);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo eliminar archivo por URL: {Url}", url);
            }
        }

        public async Task<BaseResponseDto<ImagenPropiedadDto>> UpdateImagenAsync(UpdateImagenPropiedadDto updateDto, int tenantId, bool deleteOldFile = true)
        {
            try
            {
                var existingImagen = await _imagenRepository.GetByIdAndTenantAsync(updateDto.Id, tenantId);
                if (existingImagen == null)
                {
                    return new BaseResponseDto<ImagenPropiedadDto>
                    {
                        Success = false,
                        Message = "Imagen no encontrada en su organización."
                    };
                }

                var oldUrl = existingImagen.Url;
                string? newUrl = null;

                // Si la URL viene como data:, guardarla y usar la ruta pública
                if (!string.IsNullOrWhiteSpace(updateDto.Url) && updateDto.Url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        newUrl = await SaveBase64ImageAsync(updateDto.Url, tenantId);
                        existingImagen.Url = newUrl;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al guardar imagen base64 para actualización imagen {Id}", updateDto.Id);
                        return new BaseResponseDto<ImagenPropiedadDto>
                        {
                            Success = false,
                            Message = "No se pudo guardar la imagen enviada en base64."
                        };
                    }
                }
                else if (!string.IsNullOrWhiteSpace(updateDto.Url))
                {
                    existingImagen.Url = updateDto.Url.Trim();
                }

                existingImagen.Orden = updateDto.Orden;

                await _imagenRepository.UpdateAsync(existingImagen);

                // Si se permite eliminar el archivo antiguo y es distinto, borrarlo
                if (deleteOldFile && !string.IsNullOrWhiteSpace(oldUrl) && !string.IsNullOrWhiteSpace(existingImagen.Url)
                    && !string.Equals(oldUrl, existingImagen.Url, StringComparison.OrdinalIgnoreCase)
                    && oldUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
                {
                    DeleteFileByUrl(oldUrl);
                }

                var updatedImagen = await _imagenRepository.GetByIdAndTenantAsync(existingImagen.Id, tenantId);
                var responseDto = MapToResponseDto(updatedImagen ?? existingImagen);

                return new BaseResponseDto<ImagenPropiedadDto>
                {
                    Success = true,
                    Data = responseDto,
                    Message = "Imagen actualizada correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar imagen: {Id} en tenant: {TenantId}", updateDto.Id, tenantId);
                return new BaseResponseDto<ImagenPropiedadDto>
                {
                    Success = false,
                    Message = "No se pudo actualizar la imagen.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<object>> DeleteAsync(int id, int tenantId, bool deleteFile = true)
        {
            try
            {
                var imagen = await _imagenRepository.GetByIdAndTenantAsync(id, tenantId);
                if (imagen == null)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Imagen no encontrada en su organización."
                    };
                }

                var urlToDelete = imagen.Url;

                // Eliminación física de registro (DB)
                await _imagenRepository.DeleteAsync(id);

                // Si se solicita, eliminar archivo físico
                if (deleteFile && !string.IsNullOrWhiteSpace(urlToDelete) && urlToDelete.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
                {
                    DeleteFileByUrl(urlToDelete);
                }

                _logger.LogInformation("Imagen ID: {Id} eliminada en tenant: {TenantId}", id, tenantId);

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Data = urlToDelete,
                    Message = "Imagen eliminada correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar imagen: {Id} en tenant: {TenantId}", id, tenantId);
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error al eliminar imagen.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<object>> ReordenarImagenesAsync(int propiedadId, List<int> nuevosOrdenes, int tenantId)
        {
            try
            {
                // Validar que la propiedad pertenezca al tenant
                var propiedadExists = await _imagenRepository.ValidatePropiedadInTenantAsync(propiedadId, tenantId);
                if (!propiedadExists)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Propiedad no encontrada en su organización."
                    };
                }

                await _imagenRepository.ReordenarImagenesAsync(propiedadId, nuevosOrdenes);

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Message = "Orden de las imágenes actualizado correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reordenar imágenes de propiedad: {PropiedadId} en tenant: {TenantId}", propiedadId, tenantId);
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error al reordenar las imágenes.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }
    }
}
