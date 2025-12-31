using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using inmobiliariaApi.DTOs.BusquedasGuardadas;
using inmobiliariaApi.Services;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BusquedasGuardadasController : ControllerBase
    {
        private readonly BusquedasGuardadasService _busquedasService;

        public BusquedasGuardadasController(BusquedasGuardadasService busquedasService)
        {
            _busquedasService = busquedasService;
        }

        private int GetTenantId()
        {
            var tenantClaim = User.FindFirst("IdInmobiliaria");
            return tenantClaim != null ? int.Parse(tenantClaim.Value) : 0;
        }

        // GET: api/busquedasguardadas (Búsquedas paginadas)
        [HttpGet]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> GetBusquedas(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? email = null)
        {
            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            var response = await _busquedasService.GetBusquedasPaginatedAsync(page, pageSize, tenantId, email);

            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // GET: api/busquedasguardadas/{id} (Búsqueda específica)
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

            var response = await _busquedasService.GetByIdAndTenantAsync(id, tenantId);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        // GET: api/busquedasguardadas/email/{email} (Búsquedas por email)
        [HttpGet("email/{email}")]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { Success = false, Message = "El email es obligatorio." });
            }

            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            var response = await _busquedasService.GetByEmailAndTenantAsync(email, tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // POST: api/busquedasguardadas (Crear búsqueda guardada)
        [HttpPost]
        [Authorize(Roles = "Administrador,Supervisor,Agente")]
        public async Task<IActionResult> Create([FromBody] CreateBusquedasGuardadasDto createDto)
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
            if (string.IsNullOrWhiteSpace(createDto.Email))
            {
                return BadRequest(new { Success = false, Message = "El email es obligatorio." });
            }

            if (string.IsNullOrWhiteSpace(createDto.FiltrosJson))
            {
                return BadRequest(new { Success = false, Message = "Los filtros son obligatorios." });
            }

            var response = await _busquedasService.CreateBusquedaAsync(createDto, tenantId);
            if (response.Success)
                return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
            return BadRequest(response);
        }

        // PUT: api/busquedasguardadas/{id} (Actualizar búsqueda guardada)
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBusquedasGuardadasDto updateDto)
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
            if (string.IsNullOrWhiteSpace(updateDto.Email))
            {
                return BadRequest(new { Success = false, Message = "El email es obligatorio." });
            }

            if (string.IsNullOrWhiteSpace(updateDto.FiltrosJson))
            {
                return BadRequest(new { Success = false, Message = "Los filtros son obligatorios." });
            }

            var response = await _busquedasService.UpdateBusquedaAsync(updateDto, tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // DELETE: api/busquedasguardadas/{id} (Eliminar búsqueda guardada)
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

            var response = await _busquedasService.DeleteAsync(id, tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // POST: api/busquedasguardadas/{id}/marcar-enviada (Marcar como enviada)
        [HttpPost("{id}/marcar-enviada")]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> MarcarComoEnviada(int id)
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

            var response = await _busquedasService.MarcarComoEnviadaAsync(id, tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }
    }
}
