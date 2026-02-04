using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using inmobiliariaApi.DTOs.Propiedad;
using inmobiliariaApi.Services;
using inmobiliariaApi.DTOs.ImagenPropiedad; // añadida
using inmobiliariaApi.Data; // añadida
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System;
using System.Text.Json; // añadida

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PropiedadController : ControllerBase
    {
        private readonly PropiedadService _propiedadService;
        private readonly ImagenPropiedadService _imagenService; // nueva dependencia
        private readonly ApplicationDbContext _dbContext; // nueva dependencia

        // Constructor actualizado para recibir ApplicationDbContext
        public PropiedadController(PropiedadService propiedadService, ImagenPropiedadService imagenService, ApplicationDbContext dbContext)
        {
            _propiedadService = propiedadService;
            _imagenService = imagenService;
            _dbContext = dbContext;
        }

        private int GetTenantId()
        {
            var tenantClaim = User.FindFirst("IdInmobiliaria");
            return tenantClaim != null ? int.Parse(tenantClaim.Value) : 0;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Supervisor,Agente")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? titulo = null,
            [FromQuery] int? agenteId = null,
            [FromQuery] int? estadoAdmin = null,
            [FromQuery] int? estadoOperativo = null,
            [FromQuery] decimal? precioMin = null,
            [FromQuery] decimal? precioMax = null)
        {
            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            var response = await _propiedadService.GetAllPropiedadesPaginatedAsync(
                page, pageSize, tenantId, titulo, agenteId, estadoAdmin, estadoOperativo, precioMin, precioMax);

            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Supervisor,Agente")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "El ID de la propiedad debe ser mayor a 0." });
            }

            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            var response = await _propiedadService.GetPropiedadByIdAndTenantAsync(id, tenantId);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> Create([FromBody] CreatePropiedadDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Success = false, Message = "Datos no válidos.", Errors = errors });
            }

            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            // Validaciones adicionales
            if (string.IsNullOrWhiteSpace(createDto.Titulo))
            {
                return BadRequest(new { Success = false, Message = "El título es obligatorio." });
            }

            if (string.IsNullOrWhiteSpace(createDto.Descripcion))
            {
                return BadRequest(new { Success = false, Message = "La descripción es obligatoria." });
            }

            if (string.IsNullOrWhiteSpace(createDto.Direccion))
            {
                return BadRequest(new { Success = false, Message = "La dirección es obligatoria." });
            }

            // Validación de precio: siempre debe ser mayor a 0 si se proporciona
            if (createDto.Precio.HasValue && createDto.Precio.Value <= 0)
            {
                return BadRequest(new { Success = false, Message = "El precio debe ser mayor a 0." });
            }

            // Validar latitud y longitud si se proporcionan
            if (createDto.Latitud.HasValue && (createDto.Latitud.Value < -90 || createDto.Latitud.Value > 90))
            {
                return BadRequest(new { Success = false, Message = "La latitud debe estar entre -90 y 90." });
            }

            if (createDto.Longitud.HasValue && (createDto.Longitud.Value < -180 || createDto.Longitud.Value > 180))
            {
                return BadRequest(new { Success = false, Message = "La longitud debe estar entre -180 y 180." });
            }

            // Asignar automáticamente el tenant del usuario autenticado
            // NO tomar el IdInmobiliaria del DTO, siempre usar el del tenant
            createDto.IdInmobiliaria = tenantId;

            // Iniciar transacción: si falla la creación de cualquier imagen, hacemos rollback de todo
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                // lista de urls físicas públicas creadas para limpieza en caso de error
                var createdImageUrls = new List<string>();

                try
                {
                    var response = await _propiedadService.CreatePropiedadAsync(createDto);
                    if (!response.Success)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(response);
                    }

                    // Procesar imágenes si vienen
                    var imagesResult = new
                    {
                        Created = new List<object>(),
                        Errors = new List<string>()
                    };

                    var createdPropiedadId = response.Data?.Id ?? 0;
                    if (createdPropiedadId > 0 && createDto.Imagenes != null && createDto.Imagenes.Any())
                    {
                        int orden = 1;
                        foreach (var url in createDto.Imagenes)
                        {
                            if (string.IsNullOrWhiteSpace(url))
                            {
                                imagesResult.Errors.Add("URL de imagen vacía ignorada.");
                                continue;
                            }

                            var createImgDto = new CreateImagenPropiedadDto
                            {
                                IdPropiedad = createdPropiedadId,
                                Url = url.Trim(),
                                Orden = orden++
                            };

                            var imgResp = await _imagenService.CreateImagenAsync(createImgDto, tenantId);
                            if (imgResp.Success)
                            {
                                var savedUrl = imgResp.Data?.Url;
                                if (!string.IsNullOrWhiteSpace(savedUrl))
                                    createdImageUrls.Add(savedUrl); // track para limpieza en caso de fallo posterior

                                imagesResult.Created.Add(new { Id = imgResp.Data?.Id, Url = imgResp.Data?.Url });
                            }
                            else
                            {
                                // limpiar archivos creados hasta ahora
                                foreach (var u in createdImageUrls) _imagenService.DeleteFileByUrl(u);
                                await transaction.RollbackAsync();
                                return BadRequest(new
                                {
                                    Success = false,
                                    Message = "No se pudieron guardar las imágenes. Operación cancelada.",
                                    ImagenError = imgResp.Message,
                                    Detalles = imgResp.Errors
                                });
                            }
                        }
                    }

                    // Commit si todo salió bien
                    await transaction.CommitAsync();

                    // Devolver la creación de la propiedad y resultado de imágenes
                    return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, new
                    {
                        Property = response,
                        Images = imagesResult
                    });
                }
                catch (Exception ex)
                {
                    // limpiar archivos creados si hay excepción
                    foreach (var u in createdImageUrls) _imagenService.DeleteFileByUrl(u);
                    await transaction.RollbackAsync();
                    return StatusCode(500, new { Success = false, Message = "Error interno al crear propiedad e imágenes.", Detalle = ex.Message });
                }
            }
        }

        [HttpPut("{id}")]
        [Consumes("application/json", "multipart/form-data")]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            var tenantId = GetTenantId();
            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            UpdatePropiedadDto updateDto;

            try
            {
                var form = await Request.ReadFormAsync();

                // Find the JSON content in form data
                string jsonContent = null;
                if (form.TryGetValue("updateDto", out var values) && values.Count > 0)
                {
                    jsonContent = values[0];
                }

                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    return BadRequest(new { Success = false, Message = "No se encontraron datos válidos en la solicitud." });
                }

                // Deserialize JSON into DTO
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                updateDto = JsonSerializer.Deserialize<UpdatePropiedadDto>(jsonContent, options);

                // Attach uploaded files (if any) to the DTO
                if (form.Files?.Count > 0)
                {
                    updateDto.ImagenesFiles = form.Files.ToList();
                }
            }
            catch (JsonException)
            {
                return BadRequest(new { Success = false, Message = "Formato JSON no válido." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = "Error al procesar la solicitud.", Detalle = ex.Message });
            }

            if (updateDto == null)
            {
                return BadRequest(new { Success = false, Message = "Datos no válidos." });
            }

            if (id != updateDto.Id)
                return BadRequest(new { Success = false, Message = "ID no coincide." });

            // If files were uploaded, convert to data-URL base64 and add to ImagenesParaAgregar
            if (updateDto.ImagenesFiles != null && updateDto.ImagenesFiles.Any())
            {
                updateDto.ImagenesParaAgregar ??= new List<string>();
                foreach (var formFile in updateDto.ImagenesFiles)
                {
                    if (formFile == null || formFile.Length == 0) continue;
                    using var ms = new MemoryStream();
                    await formFile.CopyToAsync(ms);
                    var bytes = ms.ToArray();
                    var base64 = Convert.ToBase64String(bytes);
                    var dataUrl = $"data:{formFile.ContentType};base64,{base64}";
                    updateDto.ImagenesParaAgregar.Add(dataUrl);
                }
            }

            // Forzar que mantenga el mismo tenant
            updateDto.IdInmobiliaria = tenantId;

            // Iniciar transacción para actualizar propiedad + imágenes
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                // listas para limpieza/acciones post-commit
                var createdImageUrls = new List<string>(); // nuevos archivos creados (para limpiar en rollback)
                var oldUrlsToDeleteAfterCommit = new List<string>(); // rutas antiguas a borrar tras commit

                try
                {
                    var propResponse = await _propiedadService.UpdatePropiedadAsync(updateDto, tenantId);
                    if (!propResponse.Success)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(propResponse);
                    }

                    // 1) Eliminar imágenes (solo DB por ahora; devolver URL antigua para borrado post-commit)
                    if (updateDto.ImagenesParaEliminar != null && updateDto.ImagenesParaEliminar.Any())
                    {
                        foreach (var imgId in updateDto.ImagenesParaEliminar)
                        {
                            var delResp = await _imagenService.DeleteAsync(imgId, tenantId, deleteFile: false);
                            if (!delResp.Success)
                            {
                                await transaction.RollbackAsync();
                                return BadRequest(new { Success = false, Message = $"No se pudo eliminar la imagen ID {imgId}.", Detalle = delResp.Message });
                            }
                            if (!string.IsNullOrWhiteSpace(delResp.Data as string))
                                oldUrlsToDeleteAfterCommit.Add(delResp.Data as string);
                        }
                    }

                    // 2) Actualizar imágenes (no borrar aún el archivo antiguo; recoger antiguas y nuevas)
                    if (updateDto.ImagenesParaActualizar != null && updateDto.ImagenesParaActualizar.Any())
                    {
                        foreach (var imgUp in updateDto.ImagenesParaActualizar)
                        {
                            // obtener URL antigua
                            var existing = await _imagenService.GetByIdAndTenantAsync(imgUp.Id, tenantId);
                            if (!existing.Success)
                            {
                                await transaction.RollbackAsync();
                                return BadRequest(new { Success = false, Message = $"No se encontró la imagen ID {imgUp.Id} para actualizar." });
                            }
                            var oldUrl = existing.Data?.Url;

                            var updateImgDto = new UpdateImagenPropiedadDto
                            {
                                Id = imgUp.Id,
                                Url = imgUp.Url ?? string.Empty,
                                Orden = imgUp.Orden
                            };

                            var updResp = await _imagenService.UpdateImagenAsync(updateImgDto, tenantId, deleteOldFile: false);
                            if (!updResp.Success)
                            {
                                // limpiar archivos creados hasta ahora
                                foreach (var u in createdImageUrls) _imagenService.DeleteFileByUrl(u);
                                await transaction.RollbackAsync();
                                return BadRequest(new { Success = false, Message = $"No se pudo actualizar la imagen ID {imgUp.Id}.", Detalle = updResp.Message });
                            }

                            // si se generó nueva URL guardarla para limpiar en rollback y programar borrado del oldUrl post-commit
                            var newUrl = updResp.Data?.Url;
                            if (!string.IsNullOrWhiteSpace(newUrl) && !string.Equals(newUrl, oldUrl, StringComparison.OrdinalIgnoreCase))
                            {
                                createdImageUrls.Add(newUrl);
                                if (!string.IsNullOrWhiteSpace(oldUrl) && oldUrl.StartsWith("/uploads/"))
                                    oldUrlsToDeleteAfterCommit.Add(oldUrl);
                            }
                        }
                    }

                    // 3) Agregar nuevas imágenes
                    if (updateDto.ImagenesParaAgregar != null && updateDto.ImagenesParaAgregar.Any())
                    {
                        // obtener la cantidad actual de imágenes para calcular el orden inicial
                        int orden = 1;
                        try
                        {
                            var existingImgsResp = await _imagenService.GetImagenesByPropiedadAsync(propResponse.Data!.Id, tenantId);
                            if (existingImgsResp.Success && existingImgsResp.Data != null)
                                orden = existingImgsResp.Data.Count() + 1;
                        }
                        catch
                        {
                            // si falla al obtener, iniciamos en 1 (no bloqueamos la operación)
                            orden = 1;
                        }

                        foreach (var url in updateDto.ImagenesParaAgregar)
                        {
                            if (string.IsNullOrWhiteSpace(url)) continue;
                            var createImgDto = new CreateImagenPropiedadDto
                            {
                                IdPropiedad = propResponse.Data!.Id,
                                Url = url.Trim(),
                                Orden = orden++
                            };

                            var imgResp = await _imagenService.CreateImagenAsync(createImgDto, tenantId);
                            if (!imgResp.Success)
                            {
                                // limpiar archivos creados hasta ahora
                                foreach (var u in createdImageUrls) _imagenService.DeleteFileByUrl(u);
                                await transaction.RollbackAsync();
                                return BadRequest(new { Success = false, Message = "No se pudieron agregar las imágenes.", Detalle = imgResp.Message });
                            }
                            var savedUrl = imgResp.Data?.Url;
                            if (!string.IsNullOrWhiteSpace(savedUrl))
                                createdImageUrls.Add(savedUrl);
                        }
                    }

                    // Commit si todo OK
                    await transaction.CommitAsync();

                    // Borrar físicamente las URLs antiguas (si correspondiera)
                    foreach (var oldUrl in oldUrlsToDeleteAfterCommit)
                    {
                        _imagenService.DeleteFileByUrl(oldUrl);
                    }

                    return Ok(propResponse);
                }
                catch (Exception)
                {
                    // limpiar archivos creados si hay excepción
                    await transaction.RollbackAsync();
                    foreach (var u in createdImageUrls) _imagenService.DeleteFileByUrl(u);
                    return StatusCode(500, new { Success = false, Message = "Error interno al actualizar propiedad e imágenes." });
                }
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            // El service maneja la eliminación lógica
            var response = await _propiedadService.DeleteAsync(id, tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // Nuevo endpoint: reactivar propiedad (solo Administrador)
        [HttpPost("{id}/reactivar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Reactivate(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            var response = await _propiedadService.ReactivateAsync(id, tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("combo")]
        [Authorize(Roles = "Administrador,Supervisor,Agente")]
        public async Task<IActionResult> GetPropiedadesCombo()
        {
            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            var response = await _propiedadService.GetPropiedadesComboAsync(tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }
    }
}
