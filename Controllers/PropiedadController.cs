using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using inmobiliariaApi.DTOs.Propiedad;
using inmobiliariaApi.Services;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PropiedadController : ControllerBase
    {
        private readonly PropiedadService _propiedadService;

        public PropiedadController(PropiedadService propiedadService)
        {
            _propiedadService = propiedadService;
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

            // Asignar automáticamente el tenant del usuario autenticado
            // NO tomar el IdInmobiliaria del DTO, siempre usar el del tenant
            createDto.IdInmobiliaria = tenantId;

            var response = await _propiedadService.CreatePropiedadAsync(createDto);
            if (response.Success)
                return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
            return BadRequest(response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePropiedadDto updateDto)
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

            // Validar que no se intente cambiar de inmobiliaria
            if (updateDto.IdInmobiliaria.HasValue && updateDto.IdInmobiliaria.Value != tenantId)
            {
                return BadRequest(new { Success = false, Message = "No puede cambiar la propiedad a otra inmobiliaria." });
            }

            // Validaciones adicionales
            if (!string.IsNullOrEmpty(updateDto.Titulo) && updateDto.Titulo.Trim().Length == 0)
            {
                return BadRequest(new { Success = false, Message = "El título no puede estar vacío." });
            }

            if (!string.IsNullOrEmpty(updateDto.Descripcion) && updateDto.Descripcion.Trim().Length == 0)
            {
                return BadRequest(new { Success = false, Message = "La descripción no puede estar vacía." });
            }

            if (!string.IsNullOrEmpty(updateDto.Direccion) && updateDto.Direccion.Trim().Length == 0)
            {
                return BadRequest(new { Success = false, Message = "La dirección no puede estar vacía." });
            }

            // Validación de precio: siempre debe ser mayor a 0 si se proporciona
            if (updateDto.Precio.HasValue && updateDto.Precio.Value <= 0)
            {
                return BadRequest(new { Success = false, Message = "El precio debe ser mayor a 0." });
            }

            if (updateDto.IdEstadoAdmin.HasValue && updateDto.IdEstadoAdmin.Value <= 0)
            {
                return BadRequest(new { Success = false, Message = "El estado administrativo debe ser mayor que 0." });
            }

            if (updateDto.IdEstadoOperativo.HasValue && updateDto.IdEstadoOperativo.Value <= 0)
            {
                return BadRequest(new { Success = false, Message = "El estado operativo debe ser mayor que 0." });
            }

            // Forzar que mantenga el mismo tenant
            updateDto.IdInmobiliaria = tenantId;

            var response = await _propiedadService.UpdatePropiedadAsync(updateDto, tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
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
    }
}
