using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using inmobiliariaApi.DTOs.LeadEstadoHistorial;
using inmobiliariaApi.Services;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeadEstadoHistorialController : ControllerBase
    {
        private readonly LeadEstadoHistorialService _historialService;

        public LeadEstadoHistorialController(LeadEstadoHistorialService historialService)
        {
            _historialService = historialService;
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

        // GET: api/leadestadohistorial (Historial paginado)
        [HttpGet]
        [Authorize(Roles = "Administrador,Supervisor,Agente")]
        public async Task<IActionResult> GetHistorial(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? leadId = null,
            [FromQuery] int? usuarioId = null,
            [FromQuery] int? estadoId = null)
        {
            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            var response = await _historialService.GetHistorialPaginatedAsync(
                page, pageSize, tenantId, leadId, usuarioId, estadoId);

            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // GET: api/leadestadohistorial/{id} (Registro específico)
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

            var response = await _historialService.GetByIdAndTenantAsync(id, tenantId);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        // GET: api/leadestadohistorial/lead/{leadId} (Historial de un lead específico)
        [HttpGet("lead/{leadId}")]
        [Authorize(Roles = "Administrador,Supervisor,Agente")]
        public async Task<IActionResult> GetHistorialByLead(int leadId)
        {
            if (leadId <= 0)
            {
                return BadRequest(new { Success = false, Message = "El ID del lead debe ser mayor a 0." });
            }

            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            var response = await _historialService.GetHistorialByLeadAsync(leadId, tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // POST: api/leadestadohistorial (Crear registro de historial)
        [HttpPost]
        [Authorize(Roles = "Administrador,Supervisor,Agente")]
        public async Task<IActionResult> Create([FromBody] CreateLeadEstadoHistorialDto createDto)
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
            if (createDto.IdLead <= 0)
            {
                return BadRequest(new { Success = false, Message = "El ID del lead debe ser mayor a 0." });
            }

            if (createDto.IdEstadoAnterior <= 0)
            {
                return BadRequest(new { Success = false, Message = "El estado anterior debe ser mayor a 0." });
            }

            if (createDto.IdEstadoNuevo <= 0)
            {
                return BadRequest(new { Success = false, Message = "El estado nuevo debe ser mayor a 0." });
            }

            if (createDto.IdEstadoAnterior == createDto.IdEstadoNuevo)
            {
                return BadRequest(new { Success = false, Message = "El estado anterior y el nuevo no pueden ser iguales." });
            }

            // Asignar el usuario del token JWT
            createDto.IdUsuario = userId;

            var response = await _historialService.CreateHistorialAsync(createDto, tenantId);
            if (response.Success)
                return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
            return BadRequest(response);
        }
    }
}
