using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using inmobiliariaApi.Services;
using inmobiliariaApi.DTOs.TipoTransaccion;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TipoTransaccionController : ControllerBase
    {
        private readonly TipoTransaccionService _service;

        public TipoTransaccionController(TipoTransaccionService service)
        {
            _service = service;
        }

        private int GetTenantId()
        {
            var tenantClaim = User.FindFirst("IdInmobiliaria");
            return tenantClaim != null ? int.Parse(tenantClaim.Value) : 0;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Supervisor,Agente")]
        public async Task<IActionResult> GetAll()
        {
            var tenantId = GetTenantId();
            if (tenantId <= 0) return BadRequest(new { Success = false, Message = "Tenant no válido." });

            var resp = await _service.GetAllAsync(tenantId);
            if (resp.Success) return Ok(resp);
            return BadRequest(resp);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Supervisor,Agente")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest(new { Success = false, Message = "ID no válido." });
            var tenantId = GetTenantId();
            if (tenantId <= 0) return BadRequest(new { Success = false, Message = "Tenant no válido." });

            var resp = await _service.GetByIdAsync(id, tenantId);
            if (resp.Success) return Ok(resp);
            return NotFound(resp);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> Create([FromBody] CreateTipoTransaccionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { Success = false, Message = "Datos no válidos.", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            var tenantId = GetTenantId();
            if (tenantId <= 0) return BadRequest(new { Success = false, Message = "Tenant no válido." });

            var resp = await _service.CreateAsync(dto);
            if (resp.Success) return CreatedAtAction(nameof(GetById), new { id = resp.Data?.Id }, resp);
            return BadRequest(resp);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTipoTransaccionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { Success = false, Message = "Datos no válidos.", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            if (id != dto.Id) return BadRequest(new { Success = false, Message = "ID no coincide." });

            var tenantId = GetTenantId();
            if (tenantId <= 0) return BadRequest(new { Success = false, Message = "Tenant no válido." });

            var resp = await _service.UpdateAsync(dto, tenantId);
            if (resp.Success) return Ok(resp);
            return BadRequest(resp);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest(new { Success = false, Message = "ID no válido." });
            var tenantId = GetTenantId();
            if (tenantId <= 0) return BadRequest(new { Success = false, Message = "Tenant no válido." });

            var resp = await _service.DeleteAsync(id, tenantId);
            if (resp.Success) return Ok(resp);
            return BadRequest(resp);
        }

        [HttpGet("combo")]
        [Authorize(Roles = "Administrador,Supervisor,Agente")]
        public async Task<IActionResult> GetCombo()
        {
            var tenantId = GetTenantId();
            if (tenantId <= 0) return BadRequest(new { Success = false, Message = "Tenant no válido." });

            var resp = await _service.GetComboAsync(tenantId);
            if (resp.Success) return Ok(resp);
            return BadRequest(resp);
        }
    }
}
