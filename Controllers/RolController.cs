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
        [Authorize(Roles = "Programador,Administrador,Supervisor,Agente")] // Todos pueden consultar roles
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _rolRepository.GetAllAsync();

            // Mapear a DTO limpio
            var result = roles.Select(r => new RolDto
            {
                Id = r.Id,
                Nombre = r.Nombre
            });

            return Ok(new { Success = true, Data = result, Message = "Roles obtenidos correctamente" });
        }

        // GET: api/rol/{id} (Un rol específico)
        [HttpGet("{id}")]
        [Authorize(Roles = "Programador,Administrador,Supervisor,Agente")]
        public async Task<IActionResult> GetRol(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "El ID debe ser mayor a 0." });
            }

            var rol = await _rolRepository.GetByIdAsync(id);
            if (rol == null)
                return NotFound(new { Success = false, Message = "Rol no encontrado" });

            var result = new RolDto { Id = rol.Id, Nombre = rol.Nombre };
            return Ok(new { Success = true, Data = result, Message = "Rol encontrado" });
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
        [Authorize(Roles = "Programador")]
        public async Task<IActionResult> PutRol(int id, [FromBody] UpdateRolDto rolDto)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Success = false, Message = "Datos no válidos", Errors = errors });
            }

            if (id != rolDto.Id)
                return BadRequest(new { Success = false, Message = "El ID de la URL no coincide con el ID del objeto" });

            var existingRol = await _rolRepository.GetByIdAsync(id);
            if (existingRol == null)
                return NotFound(new { Success = false, Message = "Rol no encontrado" });

            // Proteger roles básicos del sistema (1-5)
            if (id <= 5)
            {
                return BadRequest(new { Success = false, Message = "No se pueden modificar los roles básicos del sistema." });
            }

            // Validar que el nombre no exista en otro registro
            var existingNombre = await _rolRepository.FindAsync(r =>
                r.Nombre.ToLower() == rolDto.Nombre.ToLower() && r.Id != id);
            if (existingNombre.Any())
            {
                return BadRequest(new { Success = false, Message = "Ya existe otro rol con ese nombre." });
            }

            existingRol.Nombre = rolDto.Nombre.Trim();
            await _rolRepository.UpdateAsync(existingRol);

            var responseDto = new RolDto { Id = existingRol.Id, Nombre = existingRol.Nombre };
            return Ok(new { Success = true, Data = responseDto, Message = "Rol actualizado correctamente" });
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