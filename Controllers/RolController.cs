using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;
using inmobiliariaApi.DTOs.Rol;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RolController : ControllerBase
    {
        private readonly GenericRepository<Rol> _rolRepository;

        public RolController(GenericRepository<Rol> rolRepository)
        {
            _rolRepository = rolRepository;
        }

        // GET: api/rol (Lista de todos los roles)
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos pueden consultar roles
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _rolRepository.GetAllAsync();

            // Mapear a DTO
            var result = roles.Select(r => new
            {
                Id = r.Id,
                Nombre = r.Nombre
            });

            return Ok(new { Success = true, Data = result, Message = "Roles obtenidos correctamente" });
        }

        // GET: api/rol/{id} (Un rol específico)
        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos pueden consultar roles específicos
        public async Task<IActionResult> GetRol(int id)
        {
            var rol = await _rolRepository.GetByIdAsync(id);
            if (rol == null)
                return NotFound(new { Success = false, Message = "Rol no encontrado" });

            return Ok(new { Success = true, Data = new { rol.Id, rol.Nombre }, Message = "Rol encontrado" });
        }

        // POST: api/rol (Crear un nuevo rol)
        [HttpPost]
        [Authorize(Roles = "Programador")]
        public async Task<IActionResult> PostRol([FromBody] CreateRolDto rolDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Success = false, Message = "Datos no válidos", Errors = ModelState.Values });

            // Crear un objeto Rol a partir del DTO
            var rol = new Rol
            {
                Nombre = rolDto.Nombre
            };

            var result = await _rolRepository.AddAsync(rol);
            return CreatedAtAction(nameof(GetRol), new { id = result.Id }, new { Success = true, Data = new { result.Id, result.Nombre }, Message = "Rol creado correctamente" });
        }

        // PUT: api/rol/{id} (Actualizar rol)
        [HttpPut("{id}")]
        [Authorize(Roles = "Programador")] // Solo Programador puede actualizar roles
        public async Task<IActionResult> PutRol(int id, [FromBody] UpdateRolDto rolDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Success = false, Message = "Datos no válidos", Errors = ModelState.Values });

            var existingRol = await _rolRepository.GetByIdAsync(id);
            if (existingRol == null)
                return NotFound(new { Success = false, Message = "Rol no encontrado" });

            // Solo permitir modificar el nombre
            existingRol.Nombre = rolDto.Nombre;

            await _rolRepository.UpdateAsync(existingRol);
            return Ok(new { Success = true, Data = new { existingRol.Id, existingRol.Nombre }, Message = "Rol actualizado correctamente" });
        }

        // DELETE: api/rol/{id} (Eliminar rol)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Programador")] // Solo SuperAdmin puede eliminar roles
        public async Task<IActionResult> DeleteRol(int id)
        {
            var rol = await _rolRepository.GetByIdAsync(id);
            if (rol == null)
                return NotFound(new { Success = false, Message = "Rol no encontrado" });

            await _rolRepository.DeleteAsync(rol.Id);
            return Ok(new { Success = true, Message = "Rol eliminado correctamente" });
        }
    }
}