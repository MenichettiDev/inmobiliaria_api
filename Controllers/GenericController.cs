using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [Authorize] // Requiere autenticación para todo el controller
    public abstract class GenericController<T> : ControllerBase where T : class
    {
        protected readonly GenericService<T> _service;

        protected GenericController(GenericService<T> service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden consultar
        public async Task<IActionResult> GetAll()
        {
            var response = await _service.GetAllAsync();
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden consultar específicos
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetByIdAsync(id);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        [HttpGet("paged")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden consultar paginado
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var response = await _service.GetPagedAsync(page, pageSize);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Administrador")] // Solo SuperAdmin y Administrador pueden crear
        public async Task<IActionResult> Create([FromBody] T entity)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _service.AddAsync(entity);
            if (response.Success)
                return CreatedAtAction(nameof(GetById), new { id = GetEntityId(response.Data) }, response);
            return BadRequest(response);
        }

        [HttpPut]
        [Authorize(Roles = "SuperAdmin,Administrador")] // Solo SuperAdmin y Administrador pueden actualizar
        public async Task<IActionResult> Update([FromBody] T entity)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _service.UpdateAsync(entity);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede eliminar
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteAsync(id);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        protected abstract object GetEntityId(T? entity);
    }
}
