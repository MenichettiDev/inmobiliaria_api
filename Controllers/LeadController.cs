using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using inmobiliariaApi.DTOs.Lead;
using inmobiliariaApi.Services;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeadController : ControllerBase
    {
        private readonly LeadService _leadService;

        public LeadController(LeadService leadService)
        {
            _leadService = leadService;
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

        // GET: api/lead (Leads paginados)
        [HttpGet]
        [Authorize(Roles = "Administrador,Supervisor,Agente")]
        public async Task<IActionResult> GetLeads(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? nombre = null,
            [FromQuery] int? estadoId = null,
            [FromQuery] int? fuenteId = null,
            [FromQuery] int? usuarioAsignadoId = null,
            [FromQuery] int? propiedadId = null,
            [FromQuery] bool? activo = null)
        {
            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            var response = await _leadService.GetLeadsPaginatedAsync(
                page, pageSize, tenantId, nombre, estadoId, fuenteId, usuarioAsignadoId, propiedadId, activo);

            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // GET: api/lead/{id} (Lead específico)
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

            var response = await _leadService.GetByIdAndTenantAsync(id, tenantId);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        // POST: api/lead (Crear lead)
        [HttpPost]
        [Authorize(Roles = "Administrador,Supervisor,Agente")]
        public async Task<IActionResult> Create([FromBody] CreateLeadDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Success = false, Message = "Datos no válidos.", Errors = errors });
            }

            var tenantId = GetTenantId();
            var userId = GetUserId();

            if (tenantId <= 0 || userId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Usuario o tenant no válido." });
            }

            // Validaciones adicionales
            if (string.IsNullOrWhiteSpace(createDto.NombreCompleto))
            {
                return BadRequest(new { Success = false, Message = "El nombre completo es obligatorio." });
            }

            if (createDto.IdFuente <= 0)
            {
                return BadRequest(new { Success = false, Message = "La fuente de contacto es obligatoria." });
            }

            if (string.IsNullOrWhiteSpace(createDto.Email) && string.IsNullOrWhiteSpace(createDto.Telefono))
            {
                return BadRequest(new { Success = false, Message = "Debe proporcionar al menos un email o teléfono." });
            }

            var response = await _leadService.CreateLeadAsync(createDto, tenantId, userId);
            if (response.Success)
                return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
            return BadRequest(response);
        }

        // PUT: api/lead/{id} (Actualizar lead)
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Supervisor,Agente")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLeadDto updateDto)
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

            var response = await _leadService.UpdateLeadAsync(updateDto, tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // DELETE: api/lead/{id} (Eliminar lead)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador,Supervisor")]
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

            var response = await _leadService.DeleteAsync(id, tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // POST: api/lead/{id}/cambiar-estado (Cambiar estado del lead)
        [HttpPost("{id}/cambiar-estado")]
        [Authorize(Roles = "Administrador,Supervisor,Agente")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoLeadDto cambioDto)
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

            if (id != cambioDto.IdLead)
                return BadRequest(new { Success = false, Message = "ID no coincide." });

            var tenantId = GetTenantId();
            var userId = GetUserId();

            if (tenantId <= 0 || userId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Usuario o tenant no válido." });
            }

            if (cambioDto.IdEstadoNuevo <= 0)
            {
                return BadRequest(new { Success = false, Message = "El nuevo estado es obligatorio." });
            }

            var response = await _leadService.CambiarEstadoAsync(cambioDto, tenantId, userId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // POST: api/lead/{id}/asignar (Asignar usuario al lead)
        [HttpPost("{id}/asignar")]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> AsignarUsuario(int id, [FromBody] AsignarLeadDto asignacionDto)
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

            if (id != asignacionDto.IdLead)
                return BadRequest(new { Success = false, Message = "ID no coincide." });

            var tenantId = GetTenantId();
            var userId = GetUserId();

            if (tenantId <= 0 || userId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Usuario o tenant no válido." });
            }

            if (asignacionDto.IdUsuarioAsignado <= 0)
            {
                return BadRequest(new { Success = false, Message = "El usuario asignado es obligatorio." });
            }

            var response = await _leadService.AsignarUsuarioAsync(asignacionDto, tenantId, userId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }
    }
}