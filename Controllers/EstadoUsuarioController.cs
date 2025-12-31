using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;
using inmobiliariaApi.DTOs.EstadoUsuario;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EstadoUsuarioController : ControllerBase
    {
        private readonly GenericRepository<EstadoUsuario> _estadoUsuarioRepository;

        public EstadoUsuarioController(GenericRepository<EstadoUsuario> estadoUsuarioRepository)
        {
            _estadoUsuarioRepository = estadoUsuarioRepository;
        }

        // GET: api/estadousuario (Lista de todos los estados)
        [HttpGet]
        [Authorize(Roles = "Programador,Administrador,Supervisor")] // Solo roles administrativos
        public async Task<IActionResult> GetEstados()
        {
            var estados = await _estadoUsuarioRepository.GetAllAsync();

            // Mapear a DTO limpio
            var result = estados.Select(e => new EstadoUsuarioDto
            {
                Id = e.Id,
                Codigo = e.Codigo,
                Descripcion = e.Descripcion,
                Activo = e.Activo
            });

            return Ok(new { Success = true, Data = result, Message = "Estados de usuario obtenidos correctamente" });
        }

        // GET: api/estadousuario/activos (Solo estados activos para seleccionar en formularios)
        [HttpGet("activos")]
        [Authorize(Roles = "Programador,Administrador,Supervisor,Agente")] // Todos pueden consultar estados activos
        public async Task<IActionResult> GetEstadosActivos()
        {
            var estados = await _estadoUsuarioRepository.FindAsync(e => e.Activo == true);

            var result = estados.Select(e => new EstadoUsuarioDto
            {
                Id = e.Id,
                Codigo = e.Codigo,
                Descripcion = e.Descripcion,
                Activo = e.Activo
            });

            return Ok(new { Success = true, Data = result, Message = "Estados activos obtenidos correctamente" });
        }

        // GET: api/estadousuario/{id} (Un estado específico)
        [HttpGet("{id}")]
        [Authorize(Roles = "Programador,Administrador,Supervisor")]
        public async Task<IActionResult> GetEstado(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "El ID debe ser mayor a 0." });
            }

            var estado = await _estadoUsuarioRepository.GetByIdAsync(id);
            if (estado == null)
                return NotFound(new { Success = false, Message = "Estado de usuario no encontrado" });

            var result = new EstadoUsuarioDto
            {
                Id = estado.Id,
                Codigo = estado.Codigo,
                Descripcion = estado.Descripcion,
                Activo = estado.Activo
            };

            return Ok(new { Success = true, Data = result, Message = "Estado de usuario encontrado" });
        }

        // POST: api/estadousuario (Crear un nuevo estado)
        [HttpPost]
        [Authorize(Roles = "Programador")] // Solo Programador puede crear estados
        public async Task<IActionResult> PostEstado([FromBody] CreateEstadoUsuarioDto estadoDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Success = false, Message = "Datos no válidos", Errors = errors });
            }

            // Validar que el código no exista
            var existingCodigo = await _estadoUsuarioRepository.FindAsync(e => e.Codigo.ToLower() == estadoDto.Codigo.ToLower());
            if (existingCodigo.Any())
            {
                return BadRequest(new { Success = false, Message = "Ya existe un estado con ese código." });
            }

            // Crear un objeto EstadoUsuario a partir del DTO
            var estado = new EstadoUsuario
            {
                Codigo = estadoDto.Codigo.Trim(),
                Descripcion = estadoDto.Descripcion.Trim(),
                Activo = estadoDto.Activo
            };

            var result = await _estadoUsuarioRepository.AddAsync(estado);
            
            var responseDto = new EstadoUsuarioDto
            {
                Id = result.Id,
                Codigo = result.Codigo,
                Descripcion = result.Descripcion,
                Activo = result.Activo
            };

            return CreatedAtAction(nameof(GetEstado), new { id = result.Id }, 
                new { Success = true, Data = responseDto, Message = "Estado de usuario creado correctamente" });
        }

        // PUT: api/estadousuario/{id} (Actualizar estado)
        [HttpPut("{id}")]
        [Authorize(Roles = "Programador")] // Solo Programador puede actualizar estados
        public async Task<IActionResult> PutEstado(int id, [FromBody] UpdateEstadoUsuarioDto estadoDto)
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

            if (id != estadoDto.Id)
                return BadRequest(new { Success = false, Message = "El ID de la URL no coincide con el ID del objeto" });

            var existingEstado = await _estadoUsuarioRepository.GetByIdAsync(id);
            if (existingEstado == null)
                return NotFound(new { Success = false, Message = "Estado de usuario no encontrado" });

            // Validar que el código no exista en otro registro
            var existingCodigo = await _estadoUsuarioRepository.FindAsync(e => 
                e.Codigo.ToLower() == estadoDto.Codigo.ToLower() && e.Id != id);
            if (existingCodigo.Any())
            {
                return BadRequest(new { Success = false, Message = "Ya existe otro estado con ese código." });
            }

            // Actualizar campos
            existingEstado.Codigo = estadoDto.Codigo.Trim();
            existingEstado.Descripcion = estadoDto.Descripcion.Trim();
            existingEstado.Activo = estadoDto.Activo;

            await _estadoUsuarioRepository.UpdateAsync(existingEstado);

            var responseDto = new EstadoUsuarioDto
            {
                Id = existingEstado.Id,
                Codigo = existingEstado.Codigo,
                Descripcion = existingEstado.Descripcion,
                Activo = existingEstado.Activo
            };

            return Ok(new { Success = true, Data = responseDto, Message = "Estado de usuario actualizado correctamente" });
        }

        // DELETE: api/estadousuario/{id} (Eliminar estado - solo desactivar)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Programador")] // Solo Programador puede eliminar estados
        public async Task<IActionResult> DeleteEstado(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            var estado = await _estadoUsuarioRepository.GetByIdAsync(id);
            if (estado == null)
                return NotFound(new { Success = false, Message = "Estado de usuario no encontrado" });

            // Verificar que no sea uno de los estados fundamentales (1, 2, 3)
            if (id <= 3)
            {
                return BadRequest(new { Success = false, Message = "No se pueden eliminar los estados básicos del sistema." });
            }

            // Verificar que no haya usuarios usando este estado
            var usuariosConEstado = await _estadoUsuarioRepository.FindAsync(e => e.Id == id);
            var estadoConUsuarios = usuariosConEstado.FirstOrDefault();
            if (estadoConUsuarios != null && estadoConUsuarios.Usuarios.Any())
            {
                // En lugar de eliminar físicamente, desactivar
                estadoConUsuarios.Activo = false;
                await _estadoUsuarioRepository.UpdateAsync(estadoConUsuarios);
                return Ok(new { Success = true, Message = "Estado desactivado correctamente (hay usuarios que lo usan)." });
            }

            // Si no hay usuarios usando este estado, eliminar físicamente
            await _estadoUsuarioRepository.DeleteAsync(id);
            return Ok(new { Success = true, Message = "Estado de usuario eliminado correctamente" });
        }

        // PATCH: api/estadousuario/{id}/toggle (Activar/Desactivar estado)
        [HttpPatch("{id}/toggle")]
        [Authorize(Roles = "Programador")]
        public async Task<IActionResult> ToggleEstado(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            // No permitir desactivar estados fundamentales
            if (id <= 3)
            {
                return BadRequest(new { Success = false, Message = "No se pueden desactivar los estados básicos del sistema." });
            }

            var estado = await _estadoUsuarioRepository.GetByIdAsync(id);
            if (estado == null)
                return NotFound(new { Success = false, Message = "Estado de usuario no encontrado" });

            estado.Activo = !estado.Activo;
            await _estadoUsuarioRepository.UpdateAsync(estado);

            string accion = estado.Activo ? "activado" : "desactivado";
            return Ok(new { Success = true, Message = $"Estado {accion} correctamente." });
        }
    }
}
