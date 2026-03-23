using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using inmobiliariaApi.DTOs.Localidad;
using inmobiliariaApi.Services;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LocalidadController : ControllerBase
    {
        private readonly LocalidadService _service;

        public LocalidadController(LocalidadService service)
        {
            _service = service;
        }

        // GET: api/Localidad
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var response = await _service.GetAllAsync();
            return response.Success ? Ok(response) : BadRequest(response);
        }

        // GET: api/Localidad/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(long id)
        {
            if (id <= 0)
                return BadRequest(new { Success = false, Message = "ID no válido." });

            var response = await _service.GetByIdAsync(id);
            return response.Success ? Ok(response) : NotFound(response);
        }

        // GET: api/Localidad/por-provincia/{idProvincia}
        [HttpGet("por-provincia/{idProvincia}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByProvincia(long idProvincia)
        {
            if (idProvincia <= 0)
                return BadRequest(new { Success = false, Message = "ID de provincia no válido." });

            var response = await _service.GetByProvinciaAsync(idProvincia);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        // POST: api/Localidad
        [HttpPost]
        [Authorize(Roles = "Programador,Administrador")]
        public async Task<IActionResult> Create([FromBody] CreateLocalidadDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { Success = false, Message = "Datos no válidos.", Errors = errors });
            }

            var response = await _service.CreateAsync(dto);
            if (response.Success)
                return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
            return BadRequest(response);
        }

        // PUT: api/Localidad/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Programador,Administrador")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateLocalidadDto dto)
        {
            if (id <= 0 || id != dto.Id)
                return BadRequest(new { Success = false, Message = "ID no válido o no coincide." });

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { Success = false, Message = "Datos no válidos.", Errors = errors });
            }

            var response = await _service.UpdateAsync(dto);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        // DELETE: api/Localidad/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Programador,Administrador")]
        public async Task<IActionResult> Delete(long id)
        {
            if (id <= 0)
                return BadRequest(new { Success = false, Message = "ID no válido." });

            var response = await _service.DeleteAsync(id);
            return response.Success ? Ok(response) : NotFound(response);
        }
    }
}
