using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;
using inmobiliariaApi.Services;
using inmobiliariaApi.DTOs.TransaccionHistorial;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransaccionHistorialController : ControllerBase
    {
        private readonly TransaccionHistorialService _service;

        public TransaccionHistorialController(TransaccionHistorialService service)
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
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
            [FromQuery] int? clienteId = null, [FromQuery] int? agenteId = null, [FromQuery] byte? tipoTransaccion = null,
            [FromQuery] DateTime? fechaDesde = null, [FromQuery] DateTime? fechaHasta = null)
        {
            var tenantId = GetTenantId();
            if (tenantId <= 0) return BadRequest(new { Success = false, Message = "Tenant no válido." });

            var resp = await _service.GetAllPaginatedAsync(page, pageSize, tenantId, clienteId, agenteId, tipoTransaccion, fechaDesde, fechaHasta);
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

            var resp = await _service.GetByIdAndTenantAsync(id, tenantId);
            if (resp.Success) return Ok(resp);
            return NotFound(resp);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> Create([FromBody] CreateTransaccionHistorialDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { Success = false, Message = "Datos no válidos.", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            var tenantId = GetTenantId();
            if (tenantId <= 0) return BadRequest(new { Success = false, Message = "Tenant no válido." });

            dto.IdInmobiliaria = tenantId;
            var resp = await _service.CreateAsync(dto);
            if (resp.Success) return CreatedAtAction(nameof(GetById), new { id = resp.Data?.Id }, resp);
            return BadRequest(resp);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTransaccionHistorialDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { Success = false, Message = "Datos no válidos.", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            if (id != dto.Id) return BadRequest(new { Success = false, Message = "ID no coincide." });

            var tenantId = GetTenantId();
            if (tenantId <= 0) return BadRequest(new { Success = false, Message = "Tenant no válido." });

            dto.IdInmobiliaria = tenantId;
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