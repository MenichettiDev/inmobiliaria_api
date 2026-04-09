using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using inmobiliariaApi.DTOs.Propiedad;
using inmobiliariaApi.Services;
using inmobiliariaApi.DTOs.ImagenPropiedad; // añadida
using inmobiliariaApi.Data; // añadida
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System;
using System.Text.Json; // añadida
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<PropiedadController> _logger; // nueva dependencia

        // Constructor actualizado para recibir ApplicationDbContext e ILogger
        public PropiedadController(PropiedadService propiedadService, ImagenPropiedadService imagenService, ApplicationDbContext dbContext, ILogger<PropiedadController> logger)
        {
            _propiedadService = propiedadService;
            _imagenService = imagenService;
            _dbContext = dbContext;
            _logger = logger;
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
        [Consumes("application/json", "multipart/form-data")]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> Create()
        {
            var tenantId = GetTenantId();
            if (tenantId <= 0)
                return BadRequest(new { Success = false, Message = "Tenant no válido." });

            // Parsear DTO: acepta JSON puro o multipart con campo "propiedadJson"
            CreatePropiedadDto? createDto = null;
            List<IFormFile> archivos = new();

            if (Request.HasJsonContentType())
            {
                createDto = await Request.ReadFromJsonAsync<CreatePropiedadDto>();
            }
            else if (Request.HasFormContentType)
            {
                var form = await Request.ReadFormAsync();
                var json = form["propiedadJson"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(json))
                    return BadRequest(new { Success = false, Message = "Falta el campo 'propiedadJson' en el formulario." });

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                createDto = JsonSerializer.Deserialize<CreatePropiedadDto>(json, options);
                archivos = form.Files.Where(f => f.Length > 0).ToList();
            }

            if (createDto == null)
                return BadRequest(new { Success = false, Message = "No se pudo leer el cuerpo de la solicitud." });

            if (string.IsNullOrWhiteSpace(createDto.Titulo))
                return BadRequest(new { Success = false, Message = "El título es obligatorio." });
            if (string.IsNullOrWhiteSpace(createDto.Descripcion))
                return BadRequest(new { Success = false, Message = "La descripción es obligatoria." });
            if (string.IsNullOrWhiteSpace(createDto.Direccion))
                return BadRequest(new { Success = false, Message = "La dirección es obligatoria." });
            if (createDto.Precio.HasValue && createDto.Precio.Value <= 0)
                return BadRequest(new { Success = false, Message = "El precio debe ser mayor a 0." });

            createDto.IdInmobiliaria = tenantId;

            // Transacción: si falla cualquier imagen, se hace rollback de la BD
            // (las imágenes ya subidas a R2 se eliminan manualmente)
            var r2KeysSubidas = new List<string>(); // Almacena tanto full keys como thumb keys
            IActionResult? actionResult = null;

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    // Limpiar archivos R2 de un intento previo (si la estrategia reintenta)
                    await RollbackR2(r2KeysSubidas);
                    r2KeysSubidas.Clear();

                    using var transaction = await _dbContext.Database.BeginTransactionAsync();
                    try
                    {
                        var response = await _propiedadService.CreatePropiedadAsync(createDto);
                        if (!response.Success)
                        {
                            await transaction.RollbackAsync();
                            actionResult = BadRequest(response);
                            return;
                        }

                        var propiedadId = response.Data!.Id;
                        var imagenesCreadas = new List<object>();

                        if (archivos.Any())
                        {
                            var tiposPermitidos = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
                            const long maxSize = 5 * 1024 * 1024;
                            int orden = 1;

                            foreach (var archivo in archivos)
                            {
                                if (!tiposPermitidos.Contains(archivo.ContentType?.ToLowerInvariant() ?? ""))
                                {
                                    await RollbackR2(r2KeysSubidas);
                                    await transaction.RollbackAsync();
                                    actionResult = BadRequest(new { Success = false, Message = $"Archivo '{archivo.FileName}': formato no permitido (jpg, png, webp)." });
                                    return;
                                }
                                if (archivo.Length > maxSize)
                                {
                                    await RollbackR2(r2KeysSubidas);
                                    await transaction.RollbackAsync();
                                    actionResult = BadRequest(new { Success = false, Message = $"Archivo '{archivo.FileName}': supera el límite de 5MB." });
                                    return;
                                }

                                var esPrincipal = orden == 1 && !imagenesCreadas.Any();

                                await using (var stream = archivo.OpenReadStream())
                                {
                                    // Procesar y subir imagen (full + thumbnail)
                                    var (fullUrl, fullKey, thumbUrl, thumbKey) = await _imagenService.ProcesarYSubirImagenAsync(
                                        stream, archivo.ContentType!, tenantId, propiedadId, archivo.FileName);

                                    r2KeysSubidas.Add(fullKey);
                                    r2KeysSubidas.Add(thumbKey);

                                    var imgResult = await _imagenService.CrearRegistroImagenAsync(
                                        propiedadId, fullUrl, fullKey, orden++, esPrincipal, thumbUrl, thumbKey);

                                    if (!imgResult.Success)
                                    {
                                        await RollbackR2(r2KeysSubidas);
                                        await transaction.RollbackAsync();
                                        actionResult = BadRequest(new { Success = false, Message = "Error al guardar imagen en BD.", Detalle = imgResult.Message });
                                        return;
                                    }
                                    imagenesCreadas.Add(new { imgResult.Data!.Id, imgResult.Data.Url, imgResult.Data.ThumbnailUrl });
                                }
                            }
                        }

                        await transaction.CommitAsync();
                        actionResult = CreatedAtAction(nameof(GetById), new { id = propiedadId }, new
                        {
                            Property = response,
                            Images = new { Created = imagenesCreadas, Errors = new List<string>() }
                        });
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                await RollbackR2(r2KeysSubidas);
                return StatusCode(500, new { Success = false, Message = "Error interno al crear propiedad.", Detalle = ex.Message });
            }

            return actionResult ?? StatusCode(500, new { Success = false, Message = "Error interno al crear propiedad." });
        }

        private async Task RollbackR2(List<string> keys)
        {
            foreach (var key in keys)
            {
                try { await _imagenService.EliminarArchivoR2Async(key); } catch { /* ignorar errores de cleanup */ }
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

                if (updateDto == null)
                {
                    return BadRequest(new { Success = false, Message = "Datos no válidos." });
                }

                // Attach uploaded files (if any) to the DTO - estos se procesarán igual que en POST
                if (form.Files?.Count > 0)
                {
                    updateDto.ImagenesFiles = form.Files.ToList();
                    // Limpiar ImagenesParaAgregar para no procesar data URLs
                    updateDto.ImagenesParaAgregar = null;
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

            // Los archivos en ImagenesFiles se procesarán directamente como en POST (no convertir a base64)
            // Forzar que mantenga el mismo tenant
            updateDto.IdInmobiliaria = tenantId;

            // Iniciar transacción para actualizar propiedad + imágenes
            IActionResult? updateResult = null;
            var updateStrategy = _dbContext.Database.CreateExecutionStrategy();
            try
            {
                await updateStrategy.ExecuteAsync(async () =>
                {
                    var createdImageUrls = new List<string>();
                    var oldUrlsToDeleteAfterCommit = new List<string>();

                    using var transaction = await _dbContext.Database.BeginTransactionAsync();
                    try
                    {
                        var propResponse = await _propiedadService.UpdatePropiedadAsync(updateDto, tenantId);
                        if (!propResponse.Success)
                        {
                            await transaction.RollbackAsync();
                            updateResult = BadRequest(propResponse);
                            return;
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
                                    updateResult = BadRequest(new { Success = false, Message = $"No se pudo eliminar la imagen ID {imgId}.", Detalle = delResp.Message });
                                    return;
                                }
                                if (!string.IsNullOrWhiteSpace(delResp.Data as string))
                                    oldUrlsToDeleteAfterCommit.Add(delResp.Data as string);
                            }
                        }

                        // 2) Actualizar imágenes
                        if (updateDto.ImagenesParaActualizar != null && updateDto.ImagenesParaActualizar.Any())
                        {
                            foreach (var imgUp in updateDto.ImagenesParaActualizar)
                            {
                                var existing = await _imagenService.GetByIdAndTenantAsync(imgUp.Id, tenantId);
                                if (!existing.Success)
                                {
                                    await transaction.RollbackAsync();
                                    updateResult = BadRequest(new { Success = false, Message = $"No se encontró la imagen ID {imgUp.Id} para actualizar." });
                                    return;
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
                                    await transaction.RollbackAsync();
                                    updateResult = BadRequest(new { Success = false, Message = $"No se pudo actualizar la imagen ID {imgUp.Id}.", Detalle = updResp.Message });
                                    return;
                                }

                                var newUrl = updResp.Data?.Url;
                                if (!string.IsNullOrWhiteSpace(newUrl) && !string.Equals(newUrl, oldUrl, StringComparison.OrdinalIgnoreCase))
                                {
                                    createdImageUrls.Add(newUrl);
                                    if (!string.IsNullOrWhiteSpace(oldUrl) && oldUrl.StartsWith("/uploads/"))
                                        oldUrlsToDeleteAfterCommit.Add(oldUrl);
                                }
                            }
                        }

                        // 3) Agregar nuevas imágenes (directamente de los archivos, igual que en POST)
                        if (updateDto.ImagenesFiles != null && updateDto.ImagenesFiles.Any())
                        {
                            int orden = 1;
                            try
                            {
                                var existingImgsResp = await _imagenService.GetImagenesByPropiedadAsync(propResponse.Data!.Id, tenantId);
                                if (existingImgsResp.Success && existingImgsResp.Data != null)
                                    orden = existingImgsResp.Data.Count() + 1;
                            }
                            catch { orden = 1; }

                            var tiposPermitidos = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
                            const long maxSize = 5 * 1024 * 1024;

                            foreach (var archivo in updateDto.ImagenesFiles)
                            {
                                if (archivo == null || archivo.Length == 0) continue;

                                if (!tiposPermitidos.Contains(archivo.ContentType?.ToLowerInvariant() ?? ""))
                                {
                                    await transaction.RollbackAsync();
                                    updateResult = BadRequest(new { Success = false, Message = $"Archivo '{archivo.FileName}': formato no permitido (jpg, png, webp)." });
                                    return;
                                }
                                if (archivo.Length > maxSize)
                                {
                                    await transaction.RollbackAsync();
                                    updateResult = BadRequest(new { Success = false, Message = $"Archivo '{archivo.FileName}': supera el límite de 5MB." });
                                    return;
                                }

                                // Procesar y subir a R2 igual que en POST
                                await using (var stream = archivo.OpenReadStream())
                                {
                                    var (fullUrl, fullKey, thumbUrl, thumbKey) = await _imagenService.ProcesarYSubirImagenAsync(
                                        stream, archivo.ContentType!, tenantId, propResponse.Data!.Id, archivo.FileName);

                                    // Crear registro en BD
                                    var imgResult = await _imagenService.CrearRegistroImagenAsync(
                                        propResponse.Data!.Id, fullUrl, fullKey, orden++, false, thumbUrl, thumbKey);

                                    if (!imgResult.Success)
                                    {
                                        await transaction.RollbackAsync();
                                        updateResult = BadRequest(new { Success = false, Message = "Error al guardar imagen en BD.", Detalle = imgResult.Message });
                                        return;
                                    }
                                    createdImageUrls.Add(fullUrl);
                                }
                            }
                        }

                        await transaction.CommitAsync();
                        updateResult = Ok(propResponse);
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Success = false, Message = "Error interno al actualizar propiedad e imágenes." });
            }

            return updateResult ?? StatusCode(500, new { Success = false, Message = "Error interno al actualizar propiedad." });
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

        // ===== ENDPOINTS DE PUBLICACIÓN =====

        [HttpPatch("{id}/publicar")]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> Publicar(int id)
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

            var response = await _propiedadService.PublicarAsync(id, tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpPatch("{id}/despublicar")]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> Despublicar(int id)
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

            var response = await _propiedadService.DespublicarAsync(id, tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }
    }
}
