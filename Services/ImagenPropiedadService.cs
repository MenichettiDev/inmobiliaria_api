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
        private readonly CloudflareR2Service _r2Service;
        private readonly PlanGateService _planGateService;
        private readonly ImageProcessingService _imageProcessingService;

        public ImagenPropiedadService(
            ImagenPropiedadRepository imagenRepository,
            ILogger<ImagenPropiedadService> logger,
            CloudflareR2Service r2Service,
            PlanGateService planGateService,
            ImageProcessingService imageProcessingService)
            : base(imagenRepository)
        {
            _imagenRepository = imagenRepository;
            _logger = logger;
            _r2Service = r2Service;
            _planGateService = planGateService;
            _imageProcessingService = imageProcessingService;
        }

        private ImagenPropiedadDto MapToResponseDto(ImagenPropiedad imagen)
        {
            return new ImagenPropiedadDto
            {
                Id = imagen.Id,
                IdPropiedad = imagen.IdPropiedad,
                Url = imagen.Url,
                ThumbnailUrl = imagen.ThumbnailUrl,
                Orden = imagen.Orden,
                EsPrincipal = imagen.EsPrincipal,
                R2Key = imagen.R2Key,
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

        // ── Helpers para uso desde PropiedadController (transacción unificada) ──────

        public string GenerateR2Key(int tenantId, int propiedadId, string fileName)
            => _r2Service.GenerateKey(tenantId, propiedadId, fileName);

        public async Task<string> SubirArchivoR2Async(Stream stream, string key, string contentType)
            => await _r2Service.UploadAsync(stream, key, contentType);

        public async Task EliminarArchivoR2Async(string key)
            => await _r2Service.DeleteAsync(key);

        public async Task<BaseResponseDto<ImagenPropiedadDto>> CrearRegistroImagenAsync(
            int propiedadId, string url, string r2Key, int orden, bool esPrincipal, string? thumbnailUrl = null, string? thumbnailKey = null)
        {
            try
            {
                var imagen = new ImagenPropiedad
                {
                    IdPropiedad = propiedadId,
                    Url = url,
                    R2Key = r2Key,
                    ThumbnailUrl = thumbnailUrl,
                    ThumbnailKey = thumbnailKey,
                    Orden = orden,
                    EsPrincipal = esPrincipal,
                    CreadoEn = DateTime.UtcNow
                };
                var result = await _imagenRepository.AddAsync(imagen);
                return new BaseResponseDto<ImagenPropiedadDto>
                {
                    Success = true,
                    Data = MapToResponseDto(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear registro de imagen en BD");
                return new BaseResponseDto<ImagenPropiedadDto> { Success = false, Message = ex.Message };
            }
        }

        /// <summary>
        /// Procesa la imagen (comprime y genera thumbnail), y sube ambas a R2
        /// </summary>
        public async Task<(string fullUrl, string fullKey, string thumbUrl, string thumbKey)> ProcesarYSubirImagenAsync(
            Stream input, string contentType, int tenantId, int propiedadId, string fileName)
        {
            try
            {
                // Leer el archivo en memoria para procesarlo dos veces
                using var memoryStream = new MemoryStream();
                await input.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                // Comprimir imagen full
                using var compressedStream = await _imageProcessingService.ComprimirAsync(memoryStream, contentType);

                // Generar key para full image
                var fullKey = _r2Service.GenerateKey(tenantId, propiedadId, fileName);

                // Subir imagen comprimida
                var fullUrl = await _r2Service.UploadAsync(compressedStream, fullKey, "image/webp");

                // Generar thumbnail desde el mismo memory stream reseteado
                memoryStream.Position = 0;
                using var thumbStream = await _imageProcessingService.GenerarThumbnailAsync(memoryStream);

                // Generar key para thumbnail
                var thumbFileName = $"thumb_{Path.GetFileNameWithoutExtension(fileName)}";
                var thumbKey = _r2Service.GenerateKey(tenantId, propiedadId, thumbFileName);

                // Subir thumbnail
                var thumbUrl = await _r2Service.UploadAsync(thumbStream, thumbKey, "image/webp");

                _logger.LogInformation("Imagen procesada y subida: Full={FullKey}, Thumb={ThumbKey}", fullKey, thumbKey);

                return (fullUrl, fullKey, thumbUrl, thumbKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar y subir imagen");
                throw;
            }
        }

        /// <summary>
        /// Sube una imagen desde un IFormFile a Cloudflare R2
        /// </summary>
        public async Task<BaseResponseDto<ImagenPropiedadDto>> UploadImagenAsync(UploadImagenDto uploadDto, int tenantId)
        {
            try
            {
                _logger.LogInformation("Subiendo imagen para propiedad: {PropiedadId} en tenant: {TenantId}", uploadDto.IdPropiedad, tenantId);

                // Validar que la propiedad pertenezca al tenant y obtenerla en un solo query
                var propiedad = await _imagenRepository.GetPropiedadByIdAndTenantAsync(uploadDto.IdPropiedad, tenantId);
                _logger.LogInformation("GetPropiedadByIdAndTenant - propiedadId:{PropiedadId} tenantId:{TenantId} encontrada:{Found}", uploadDto.IdPropiedad, tenantId, propiedad != null);
                if (propiedad == null)
                {
                    return new BaseResponseDto<ImagenPropiedadDto>
                    {
                        Success = false,
                        Message = "Propiedad no encontrada en su organización."
                    };
                }

                // Validar límite de imágenes por plan
                var gateResult = await _planGateService.PuedeSubirImagenAsync(uploadDto.IdPropiedad, propiedad.IdInmobiliaria);
                if (!gateResult.Allowed)
                {
                    return new BaseResponseDto<ImagenPropiedadDto>
                    {
                        Success = false,
                        Message = gateResult.Reason ?? "No puedes subir más imágenes en tu plan actual.",
                        Errors = new List<string> { gateResult.Reason ?? "Límite alcanzado." }
                    };
                }

                // Validar archivo
                if (uploadDto.Archivo == null || uploadDto.Archivo.Length == 0)
                {
                    return new BaseResponseDto<ImagenPropiedadDto>
                    {
                        Success = false,
                        Message = "Debes seleccionar un archivo."
                    };
                }

                // Validar tipo de archivo
                var tiposPermitidos = new[] { "image/jpeg", "image/png", "image/webp" };
                if (!tiposPermitidos.Contains(uploadDto.Archivo.ContentType?.ToLowerInvariant() ?? ""))
                {
                    return new BaseResponseDto<ImagenPropiedadDto>
                    {
                        Success = false,
                        Message = "Solo se permiten imágenes JPG, PNG o WEBP."
                    };
                }

                // Validar tamaño máximo (5MB)
                const long maxSize = 5 * 1024 * 1024;
                if (uploadDto.Archivo.Length > maxSize)
                {
                    return new BaseResponseDto<ImagenPropiedadDto>
                    {
                        Success = false,
                        Message = "El archivo no debe exceder 5MB."
                    };
                }

                try
                {
                    await using (var stream = uploadDto.Archivo.OpenReadStream())
                    {
                        // Procesar y subir imagen (full + thumbnail)
                        var (fullUrl, fullKey, thumbUrl, thumbKey) = await ProcesarYSubirImagenAsync(
                            stream, uploadDto.Archivo.ContentType, propiedad.IdInmobiliaria, uploadDto.IdPropiedad, uploadDto.Archivo.FileName);

                        // Verificar si es la primera imagen (marcar como principal)
                        var countImagenes = await _imagenRepository.GetCountByPropiedadAsync(uploadDto.IdPropiedad);
                        var esPrincipal = countImagenes == 0;

                        // Obtener próximo orden
                        var maxOrden = await _imagenRepository.GetMaxOrdenByPropiedadAsync(uploadDto.IdPropiedad);
                        var nuevoOrden = maxOrden + 1;

                        // Crear registro en BD con ambas URLs
                        var imagen = new ImagenPropiedad
                        {
                            IdPropiedad = uploadDto.IdPropiedad,
                            Url = fullUrl,
                            R2Key = fullKey,
                            ThumbnailUrl = thumbUrl,
                            ThumbnailKey = thumbKey,
                            Orden = nuevoOrden,
                            EsPrincipal = esPrincipal,
                            CreadoEn = DateTime.UtcNow
                        };

                        var result = await _imagenRepository.AddAsync(imagen);
                        _logger.LogInformation("Imagen subida exitosamente con ID: {Id}, Key: {R2Key}, ThumbKey: {ThumbKey}", result.Id, fullKey, thumbKey);

                        var imagenConDetalles = await _imagenRepository.GetByIdWithDetailsAsync(result.Id);
                        var responseDto = MapToResponseDto(imagenConDetalles ?? result);

                        return new BaseResponseDto<ImagenPropiedadDto>
                        {
                            Success = true,
                            Data = responseDto,
                            Message = "Imagen subida correctamente."
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al subir archivo a R2 para propiedad {PropiedadId}", uploadDto.IdPropiedad);
                    return new BaseResponseDto<ImagenPropiedadDto>
                    {
                        Success = false,
                        Message = "No se pudo subir la imagen. Intenta de nuevo."
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar subida de imagen para propiedad: {PropiedadId} en tenant: {TenantId}", uploadDto?.IdPropiedad, tenantId);
                return new BaseResponseDto<ImagenPropiedadDto>
                {
                    Success = false,
                    Message = "Error al subir la imagen.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        /// <summary>
        /// Mantiene compatibilidad con CreateImagenAsync para URLs externas
        /// </summary>
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

                // No permitir más base64 - debe usarse el endpoint /upload
                if (!string.IsNullOrWhiteSpace(createDto.Url) && createDto.Url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                {
                    return new BaseResponseDto<ImagenPropiedadDto>
                    {
                        Success = false,
                        Message = "Use el endpoint /api/imagenpropiedad/upload para subir imágenes con archivo."
                    };
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

        /// <summary>
        /// Marcar una imagen como principal (solo puede haber una por propiedad)
        /// </summary>
        public async Task<BaseResponseDto<ImagenPropiedadDto>> HacerPrincipalAsync(int id, int tenantId)
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

                // Si ya es principal, no hacer nada
                if (imagen.EsPrincipal)
                {
                    var responseDto = MapToResponseDto(imagen);
                    return new BaseResponseDto<ImagenPropiedadDto>
                    {
                        Success = true,
                        Data = responseDto,
                        Message = "Esta imagen ya es la principal."
                    };
                }

                // Obtener todas las imágenes de la propiedad
                var imagenes = await _imagenRepository.GetByPropiedadAndTenantAsync(imagen.IdPropiedad, tenantId);

                // Desmarcar la actual principal
                foreach (var img in imagenes.Where(i => i.EsPrincipal))
                {
                    img.EsPrincipal = false;
                    await _imagenRepository.UpdateAsync(img);
                }

                // Marcar la nueva como principal
                imagen.EsPrincipal = true;
                await _imagenRepository.UpdateAsync(imagen);

                _logger.LogInformation("Imagen {Id} marcada como principal para propiedad {PropiedadId}", id, imagen.IdPropiedad);

                var responseDtoResult = MapToResponseDto(imagen);
                return new BaseResponseDto<ImagenPropiedadDto>
                {
                    Success = true,
                    Data = responseDtoResult,
                    Message = "Imagen marcada como principal."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar imagen como principal: {Id} en tenant: {TenantId}", id, tenantId);
                return new BaseResponseDto<ImagenPropiedadDto>
                {
                    Success = false,
                    Message = "Error al actualizar la imagen.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
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

                // No permitir base64 para actualizaciones - debe usarse el endpoint /upload
                if (!string.IsNullOrWhiteSpace(updateDto.Url) && updateDto.Url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                {
                    return new BaseResponseDto<ImagenPropiedadDto>
                    {
                        Success = false,
                        Message = "No se pueden actualizar imágenes con base64. Use el endpoint /upload para subir nuevas imágenes."
                    };
                }

                // Solo actualizar orden si se proporciona URL
                if (!string.IsNullOrWhiteSpace(updateDto.Url))
                {
                    existingImagen.Url = updateDto.Url.Trim();
                }

                existingImagen.Orden = updateDto.Orden;

                await _imagenRepository.UpdateAsync(existingImagen);

                // R2 cleanup: no necesario, R2 maneja su propio almacenamiento
                // if (deleteOldFile && !string.IsNullOrWhiteSpace(oldUrl) && existingImagen.R2Key != null)
                // {
                //     await _r2Service.DeleteAsync(existingImagen.R2Key);
                // }

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

                var r2Key = imagen.R2Key;
                var thumbKey = imagen.ThumbnailKey;

                // Eliminación de registro en BD
                await _imagenRepository.DeleteAsync(id);

                // Eliminar de R2 si existen keys
                if (deleteFile)
                {
                    if (!string.IsNullOrWhiteSpace(r2Key))
                    {
                        try
                        {
                            await _r2Service.DeleteAsync(r2Key);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error al eliminar archivo de R2: {R2Key}", r2Key);
                            // No fallar la operación si R2 falla, ya que el registro fue eliminado de BD
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(thumbKey))
                    {
                        try
                        {
                            await _r2Service.DeleteAsync(thumbKey);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error al eliminar thumbnail de R2: {ThumbKey}", thumbKey);
                        }
                    }
                }

                _logger.LogInformation("Imagen ID: {Id} eliminada en tenant: {TenantId} (R2Key: {R2Key}, ThumbKey: {ThumbKey})", id, tenantId, r2Key, thumbKey);

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Data = new { id, r2Key, thumbKey },
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
