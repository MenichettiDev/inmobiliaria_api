using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using inmobiliariaApi.DTOs.Suscripcion;
using inmobiliariaApi.Services;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SuscripcionController : ControllerBase
    {
        private readonly SuscripcionService _suscripcionService;

        public SuscripcionController(SuscripcionService suscripcionService)
        {
            _suscripcionService = suscripcionService;
        }

        private int GetTenantId()
        {
            var tenantClaim = User.FindFirst("IdInmobiliaria");
            return tenantClaim != null ? int.Parse(tenantClaim.Value) : 0;
        }

        // GET: api/suscripcion (Solo para Programadores - listar todas)
        [HttpGet]
        [Authorize(Roles = "Programador")]
        public async Task<IActionResult> GetSuscripciones(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? inmobiliariaId = null,
            [FromQuery] int? planId = null,
            [FromQuery] byte? estadoId = null,
            [FromQuery] bool? vigente = null)
        {
            var response = await _suscripcionService.GetSuscripcionesAsync(page, pageSize, inmobiliariaId, planId, estadoId, vigente);

            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // GET: api/suscripcion/{id} (Solo para Programadores)
        [HttpGet("{id}")]
        [Authorize(Roles = "Programador")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "El ID debe ser mayor a 0." });
            }

            var response = await _suscripcionService.GetByIdAsync(id);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        // GET: api/suscripcion/mi-suscripcion (Para usuarios ver su suscripción activa)
        [HttpGet("mi-suscripcion")]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> GetMySuscripcion()
        {
            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            var response = await _suscripcionService.GetActiveByInmobiliariaAsync(tenantId);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        // POST: api/suscripcion (Solo para Programadores - crear suscripción)
        [HttpPost]
        [Authorize(Roles = "Programador")]
        public async Task<IActionResult> Create([FromBody] CreateSuscripcionDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Success = false, Message = "Datos no válidos.", Errors = errors });
            }

            // Validaciones adicionales
            if (createDto.IdInmobiliaria <= 0)
            {
                return BadRequest(new { Success = false, Message = "Debe seleccionar una inmobiliaria válida." });
            }

            if (createDto.IdPlan <= 0)
            {
                return BadRequest(new { Success = false, Message = "Debe seleccionar un plan válido." });
            }

            if (createDto.Fin <= createDto.Inicio)
            {
                return BadRequest(new { Success = false, Message = "La fecha de fin debe ser posterior a la fecha de inicio." });
            }

            if (createDto.Inicio < DateTime.UtcNow.Date)
            {
                return BadRequest(new { Success = false, Message = "La fecha de inicio no puede ser anterior a hoy." });
            }

            var response = await _suscripcionService.CreateSuscripcionAsync(createDto);
            if (response.Success)
                return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
            return BadRequest(response);
        }

        // PUT: api/suscripcion/{id} (Solo para Programadores)
        [HttpPut("{id}")]
        [Authorize(Roles = "Programador")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSuscripcionDto updateDto)
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

            var response = await _suscripcionService.UpdateSuscripcionAsync(updateDto);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // POST: api/suscripcion/{id}/renovar (Solo para Programadores)
        [HttpPost("{id}/renovar")]
        [Authorize(Roles = "Programador")]
        public async Task<IActionResult> Renovar(int id, [FromBody] RenovarSuscripcionDto renovarDto)
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

            if (id != renovarDto.IdSuscripcion)
                return BadRequest(new { Success = false, Message = "ID no coincide." });

            if (renovarDto.NuevaFechaFin <= DateTime.UtcNow)
            {
                return BadRequest(new { Success = false, Message = "La nueva fecha de fin debe ser posterior a hoy." });
            }

            var response = await _suscripcionService.RenovarSuscripcionAsync(renovarDto);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // PATCH: api/suscripcion/{id}/estado (Cambiar estado - Solo Programadores)
        [HttpPatch("{id}/estado")]
        [Authorize(Roles = "Programador")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] byte nuevoEstado)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            if (nuevoEstado < 1 || nuevoEstado > 4)
            {
                return BadRequest(new { Success = false, Message = "Estado no válido. Use 1 (activa), 2 (pausada), 3 (vencida) o 4 (cancelada)." });
            }

            var response = await _suscripcionService.CambiarEstadoAsync(id, nuevoEstado);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // GET: api/suscripcion/vencen-pronto (Suscripciones próximas a vencer - Solo Programadores)
        [HttpGet("vencen-pronto")]
        [Authorize(Roles = "Programador")]
        public async Task<IActionResult> GetVencenPronto([FromQuery] int dias = 30)
        {
            if (dias < 1 || dias > 365)
            {
                return BadRequest(new { Success = false, Message = "Los días deben estar entre 1 y 365." });
            }

            var response = await _suscripcionService.GetVencenSoonAsync(dias);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }
    }
}
