using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using inmobiliariaApi.DTOs.Inmobiliaria;
using inmobiliariaApi.Services;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InmobiliariaController : ControllerBase
    {
        private readonly InmobiliariaService _inmobiliariaService;

        public InmobiliariaController(InmobiliariaService inmobiliariaService)
        {
            _inmobiliariaService = inmobiliariaService;
        }

        // GET: api/inmobiliaria (Solo para Programadores - listar todas)
        [HttpGet]
        [Authorize(Roles = "Programador")]
        public async Task<IActionResult> GetInmobiliarias(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? nombre = null,
            [FromQuery] int? planId = null,
            [FromQuery] int? estadoId = null)
        {
            var response = await _inmobiliariaService.GetInmobiliariasAsync(page, pageSize, nombre, planId, estadoId);

            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // GET: api/inmobiliaria/{id} (Solo para Programadores)
        [HttpGet("{id}")]
        [Authorize(Roles = "Programador, Administrador")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "El ID debe ser mayor a 0." });
            }

            var response = await _inmobiliariaService.GetByIdAsync(id);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        // POST: api/inmobiliaria (Solo para Programadores - crear nueva inmobiliaria)
        [HttpPost]
        [Authorize(Roles = "Programador")]
        public async Task<IActionResult> Create([FromBody] CreateInmobiliariaDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Success = false, Message = "Datos no válidos.", Errors = errors });
            }

            // Validaciones adicionales
            if (string.IsNullOrWhiteSpace(createDto.Nombre))
            {
                return BadRequest(new { Success = false, Message = "El nombre es obligatorio." });
            }

            if (string.IsNullOrWhiteSpace(createDto.Subdominio))
            {
                return BadRequest(new { Success = false, Message = "El subdominio es obligatorio." });
            }

            if (createDto.IdPlan <= 0)
            {
                return BadRequest(new { Success = false, Message = "Debe seleccionar un plan válido." });
            }

            var response = await _inmobiliariaService.CreateInmobiliariaAsync(createDto);
            if (response.Success)
                return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
            return BadRequest(response);
        }

        // PUT: api/inmobiliaria/{id} (Solo para Programadores)
        [HttpPut("{id}")]
        [Authorize(Roles = "Programador")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateInmobiliariaDto updateDto)
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

            var response = await _inmobiliariaService.UpdateInmobiliariaAsync(updateDto);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        // PATCH: api/inmobiliaria/{id}/estado (Cambiar estado - Solo Programadores)
        [HttpPatch("{id}/estado")]
        [Authorize(Roles = "Programador")]
        public async Task<IActionResult> ChangeState(int id, [FromBody] int newState)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            if (newState < 1 || newState > 3)
            {
                return BadRequest(new { Success = false, Message = "Estado no válido. Use 1 (activa), 2 (suspendida) o 3 (cancelada)." });
            }

            var response = await _inmobiliariaService.ChangeStateAsync(id, newState);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }
    }
}
