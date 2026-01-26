using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using inmobiliariaApi.DTOs.ImagenPropiedad;
using inmobiliariaApi.Services;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ImagenPropiedadController : ControllerBase
    {
        private readonly ImagenPropiedadService _imagenService;

        public ImagenPropiedadController(ImagenPropiedadService imagenService)
        {
            _imagenService = imagenService;
        }

        private int GetTenantId()
        {
            var tenantClaim = User.FindFirst("IdInmobiliaria");
            return tenantClaim != null ? int.Parse(tenantClaim.Value) : 0;
        }

        // GET: api/imagenpropiedad (Imágenes paginadas)
        [HttpGet]
        [Authorize(Roles = "Administrador,Supervisor,Agente")]
        public async Task<IActionResult> GetImagenes(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? propiedadId = null)
        {
            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            var response = await _imagenService.GetImagenesPaginatedAsync(page, pageSize, tenantId, propiedadId);

            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // GET: api/imagenpropiedad/{id} (Imagen específica)
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Supervisor,Agente")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "El ID debe ser mayor a 0." });
            }

            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            var response = await _imagenService.GetByIdAndTenantAsync(id, tenantId);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        // GET: api/imagenpropiedad/propiedad/{propiedadId} (Imágenes de una propiedad específica)
        [HttpGet("propiedad/{propiedadId}")]
        [Authorize(Roles = "Administrador,Supervisor,Agente")]
        public async Task<IActionResult> GetImagenesByPropiedad(int propiedadId)
        {
            if (propiedadId <= 0)
            {
                return BadRequest(new { Success = false, Message = "El ID de la propiedad debe ser mayor a 0." });
            }

            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            var response = await _imagenService.GetImagenesByPropiedadAsync(propiedadId, tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // POST: api/imagenpropiedad (Crear imagen)
        [HttpPost]
        [Authorize(Roles = "Administrador,Supervisor,Agente")]
        public async Task<IActionResult> Create([FromBody] CreateImagenPropiedadDto createDto)
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
            if (createDto.IdPropiedad <= 0)
            {
                return BadRequest(new { Success = false, Message = "El ID de la propiedad debe ser mayor a 0." });
            }

            if (string.IsNullOrWhiteSpace(createDto.Url))
            {
                return BadRequest(new { Success = false, Message = "La URL de la imagen es obligatoria." });
            }

            var response = await _imagenService.CreateImagenAsync(createDto, tenantId);
            if (response.Success)
                return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
            return BadRequest(response);
        }

        // PUT: api/imagenpropiedad/{id} (Actualizar imagen)
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Supervisor,Agente")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateImagenPropiedadDto updateDto)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Success = false, Message = "Datos no válidos.", Errors = errors });
            }

            if (id != updateDto.Id)
                return BadRequest(new { Success = false, Message = "ID no coincide." });

            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            // Validaciones adicionales
            if (string.IsNullOrWhiteSpace(updateDto.Url))
            {
                return BadRequest(new { Success = false, Message = "La URL de la imagen es obligatoria." });
            }

            var response = await _imagenService.UpdateImagenAsync(updateDto, tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // DELETE: api/imagenpropiedad/{id} (Eliminar imagen)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador,Supervisor,Agente")]
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

            var response = await _imagenService.DeleteAsync(id, tenantId); // borra DB y archivo
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // PUT: api/imagenpropiedad/propiedad/{propiedadId}/reordenar (Reordenar imágenes)
        [HttpPut("propiedad/{propiedadId}/reordenar")]
        [Authorize(Roles = "Administrador,Supervisor,Agente")]
        public async Task<IActionResult> ReordenarImagenes(int propiedadId, [FromBody] List<int> nuevosOrdenes)
        {
            if (propiedadId <= 0)
            {
                return BadRequest(new { Success = false, Message = "El ID de la propiedad debe ser mayor a 0." });
            }

            if (nuevosOrdenes == null || !nuevosOrdenes.Any())
            {
                return BadRequest(new { Success = false, Message = "Debe proporcionar al menos un orden." });
            }

            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            var response = await _imagenService.ReordenarImagenesAsync(propiedadId, nuevosOrdenes, tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }
    }
}
