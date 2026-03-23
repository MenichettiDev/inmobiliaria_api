using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using inmobiliariaApi.DTOs.Provincia;
using inmobiliariaApi.Services;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProvinciaController : ControllerBase
    {
        private readonly ProvinciaService _service;

        public ProvinciaController(ProvinciaService service)
        {
            _service = service;
        }

        // GET: api/Provincia
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] bool soloActivas = true)
        {
            var response = await _service.GetAllAsync(soloActivas);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        // GET: api/Provincia/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(long id)
        {
            if (id <= 0)
                return BadRequest(new { Success = false, Message = "ID no válido." });

            var response = await _service.GetByIdAsync(id);
            return response.Success ? Ok(response) : NotFound(response);
        }

        // POST: api/Provincia
        [HttpPost]
        [Authorize(Roles = "Programador,Administrador")]
        public async Task<IActionResult> Create([FromBody] CreateProvinciaDto dto)
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

        // PUT: api/Provincia/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Programador,Administrador")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateProvinciaDto dto)
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

        // DELETE: api/Provincia/{id}
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
