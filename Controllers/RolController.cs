using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using pyreApi.DTOs.Rol;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación para todo el controller
    public class RolController : ControllerBase
    {
        private readonly RolService _rolService;

        public RolController(RolService rolService)
        {
            _rolService = rolService;
        }

        // GET: api/rol (Lista de todos los roles)
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos pueden consultar roles
        public async Task<IActionResult> GetRoles()
        {
            var result = await _rolService.GetAllRolesAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // GET: api/rol/{id} (Un rol específico)
        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos pueden consultar roles específicos
        public async Task<IActionResult> GetRol(int id)
        {
            var result = await _rolService.GetRolByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        // POST: api/rol (Crear un nuevo rol)
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede crear roles
        public async Task<IActionResult> PostRol([FromBody] CreateRolDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _rolService.CreateRolAsync(createDto);
            return result.Success ? CreatedAtAction(nameof(GetRol), new { id = result.Data?.IdRol }, result) : BadRequest(result);
        }

        // PUT: api/rol/{id} (Actualizar rol)
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede actualizar roles
        public async Task<IActionResult> PutRol(int id, [FromBody] UpdateRolDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != updateDto.IdRol)
                return BadRequest("El ID de la URL no coincide con el ID del objeto");

            var result = await _rolService.UpdateRolAsync(updateDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // DELETE: api/rol/{id} (Eliminar rol)
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede eliminar roles
        public async Task<IActionResult> DeleteRol(int id)
        {
            var result = await _rolService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}