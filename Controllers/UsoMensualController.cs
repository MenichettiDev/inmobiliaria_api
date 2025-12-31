using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using inmobiliariaApi.DTOs.UsoMensual;
using inmobiliariaApi.Services;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsoMensualController : ControllerBase
    {
        private readonly UsoMensualService _usoMensualService;

        public UsoMensualController(UsoMensualService usoMensualService)
        {
            _usoMensualService = usoMensualService;
        }

        private int GetTenantId()
        {
            var tenantClaim = User.FindFirst("IdInmobiliaria");
            return tenantClaim != null ? int.Parse(tenantClaim.Value) : 0;
        }

        // GET: api/usomensual (Uso mensual paginado)
        [HttpGet]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> GetUsoMensual(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? mesDesde = null,
            [FromQuery] string? mesHasta = null)
        {
            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            var response = await _usoMensualService.GetUsoMensualPaginatedAsync(
                page, pageSize, tenantId, mesDesde, mesHasta);

            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // GET: api/usomensual/{id} (Registro específico)
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Supervisor")]
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

            var response = await _usoMensualService.GetByIdAndTenantAsync(id, tenantId);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        // GET: api/usomensual/ultimos-seis-meses (Dashboard/estadísticas)
        [HttpGet("ultimos-seis-meses")]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> GetUltimosSeisMeses()
        {
            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            var response = await _usoMensualService.GetUltimosSeisMesesAsync(tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // GET: api/usomensual/estadisticas (Estadísticas avanzadas)
        [HttpGet("estadisticas")]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> GetEstadisticas(
            [FromQuery] string? mesDesde = null,
            [FromQuery] string? mesHasta = null)
        {
            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            var response = await _usoMensualService.GetEstadisticasAsync(tenantId, mesDesde, mesHasta);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // POST: api/usomensual (Crear registro manual)
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create([FromBody] CreateUsoMensualDto createDto)
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

            var response = await _usoMensualService.CreateUsoMensualAsync(createDto, tenantId);
            if (response.Success)
                return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
            return BadRequest(response);
        }

        // PUT: api/usomensual/{id} (Actualizar registro)
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUsoMensualDto updateDto)
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

            var response = await _usoMensualService.UpdateUsoMensualAsync(updateDto, tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // POST: api/usomensual/incrementar (Incrementar automáticamente - usado por el sistema)
        [HttpPost("incrementar")]
        [Authorize] // Todos los roles pueden incrementar (cuando se crea un lead)
        public async Task<IActionResult> IncrementarLeads()
        {
            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            var response = await _usoMensualService.IncrementarLeadsMesActualAsync(tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }
    }
}
