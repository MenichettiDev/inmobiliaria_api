using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;
using inmobiliariaApi.DTOs.EstadoPropiedadActividad;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EstadoPropiedadActividadController : ControllerBase
    {
        private readonly GenericRepository<EstadoPropiedadActividad> _estadoRepository;

        public EstadoPropiedadActividadController(GenericRepository<EstadoPropiedadActividad> estadoRepository)
        {
            _estadoRepository = estadoRepository;
        }

        // GET: api/estadopropiedadactividad (Lista de todos los estados)
        [HttpGet]
        [Authorize(Roles = "Programador,Administrador,Supervisor")] // Solo roles administrativos
        public async Task<IActionResult> GetEstados()
        {
            var estados = await _estadoRepository.GetAllAsync();

            // Mapear a DTO limpio
            var result = estados.Select(e => new EstadoPropiedadActividadDto
            {
                Id = e.Id,
                Codigo = e.Codigo,
                Descripcion = e.Descripcion,
                Visible = e.Visible
            });

            return Ok(new { Success = true, Data = result, Message = "Estados de actividad obtenidos correctamente" });
        }

        // GET: api/estadopropiedadactividad/visibles (Solo estados visibles para seleccionar en formularios)
        [HttpGet("visibles")]
        [Authorize(Roles = "Programador,Administrador,Supervisor,Agente")] // Todos pueden consultar estados visibles
        public async Task<IActionResult> GetEstadosVisibles()
        {
            var estados = await _estadoRepository.FindAsync(e => e.Visible == true);

            var result = estados.Select(e => new EstadoPropiedadActividadDto
            {
                Id = e.Id,
                Codigo = e.Codigo,
                Descripcion = e.Descripcion,
                Visible = e.Visible
            });

            return Ok(new { Success = true, Data = result, Message = "Estados de actividad visibles obtenidos correctamente" });
        }

        // GET: api/estadopropiedadactividad/{id} (Un estado específico)
        [HttpGet("{id}")]
        [Authorize(Roles = "Programador,Administrador,Supervisor")]
        public async Task<IActionResult> GetEstado(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "El ID debe ser mayor a 0." });
            }

            var estado = await _estadoRepository.GetByIdAsync(id);
            if (estado == null)
                return NotFound(new { Success = false, Message = "Estado de actividad no encontrado" });

            var result = new EstadoPropiedadActividadDto
            {
                Id = estado.Id,
                Codigo = estado.Codigo,
                Descripcion = estado.Descripcion,
                Visible = estado.Visible
            };

            return Ok(new { Success = true, Data = result, Message = "Estado de actividad encontrado" });
        }

        // POST: api/estadopropiedadactividad (Crear un nuevo estado)
        [HttpPost]
        [Authorize(Roles = "Programador")] // Solo Programador puede crear estados
        public async Task<IActionResult> PostEstado([FromBody] CreateEstadoPropiedadActividadDto estadoDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Success = false, Message = "Datos no válidos", Errors = errors });
            }

            // Validar que el código no exista
            var existingCodigo = await _estadoRepository.FindAsync(e => e.Codigo.ToLower() == estadoDto.Codigo.ToLower());
            if (existingCodigo.Any())
            {
                return BadRequest(new { Success = false, Message = "Ya existe un estado de actividad con ese código." });
            }

            // Validar que la descripción no exista
            var existingDescripcion = await _estadoRepository.FindAsync(e => e.Descripcion.ToLower() == estadoDto.Descripcion.ToLower());
            if (existingDescripcion.Any())
            {
                return BadRequest(new { Success = false, Message = "Ya existe un estado de actividad con esa descripción." });
            }

            // Crear un objeto EstadoPropiedadActividad a partir del DTO
            var estado = new EstadoPropiedadActividad
            {
                Codigo = estadoDto.Codigo.Trim(),
                Descripcion = estadoDto.Descripcion.Trim(),
                Visible = estadoDto.Visible
            };

            var result = await _estadoRepository.AddAsync(estado);

            var responseDto = new EstadoPropiedadActividadDto
            {
                Id = result.Id,
                Codigo = result.Codigo,
                Descripcion = result.Descripcion,
                Visible = result.Visible
            };

            return CreatedAtAction(nameof(GetEstado), new { id = result.Id },
                new { Success = true, Data = responseDto, Message = "Estado de actividad creado correctamente" });
        }

        // PUT: api/estadopropiedadactividad/{id} (Actualizar estado)
        [HttpPut("{id}")]
        [Authorize(Roles = "Programador")] // Solo Programador puede actualizar estados
        public async Task<IActionResult> PutEstado(int id, [FromBody] UpdateEstadoPropiedadActividadDto estadoDto)
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

            var existingEstado = await _estadoRepository.GetByIdAsync(id);
            if (existingEstado == null)
                return NotFound(new { Success = false, Message = "Estado de actividad no encontrado" });

            // Validar que el código no exista en otro registro
            var existingCodigo = await _estadoRepository.FindAsync(e =>
                e.Codigo.ToLower() == estadoDto.Codigo.ToLower() && e.Id != id);
            if (existingCodigo.Any())
            {
                return BadRequest(new { Success = false, Message = "Ya existe otro estado de actividad con ese código." });
            }

            // Validar que la descripción no exista en otro registro
            var existingDescripcion = await _estadoRepository.FindAsync(e =>
                e.Descripcion.ToLower() == estadoDto.Descripcion.ToLower() && e.Id != id);
            if (existingDescripcion.Any())
            {
                return BadRequest(new { Success = false, Message = "Ya existe otro estado de actividad con esa descripción." });
            }

            // Actualizar campos
            existingEstado.Codigo = estadoDto.Codigo.Trim();
            existingEstado.Descripcion = estadoDto.Descripcion.Trim();
            existingEstado.Visible = estadoDto.Visible;

            await _estadoRepository.UpdateAsync(existingEstado);

            var responseDto = new EstadoPropiedadActividadDto
            {
                Id = existingEstado.Id,
                Codigo = existingEstado.Codigo,
                Descripcion = existingEstado.Descripcion,
                Visible = existingEstado.Visible
            };

            return Ok(new { Success = true, Data = responseDto, Message = "Estado de actividad actualizado correctamente" });
        }

        // DELETE: api/estadopropiedadactividad/{id} (Eliminar estado - solo ocultar)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Programador")] // Solo Programador puede eliminar estados
        public async Task<IActionResult> DeleteEstado(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            var estado = await _estadoRepository.GetByIdAsync(id);
            if (estado == null)
                return NotFound(new { Success = false, Message = "Estado de actividad no encontrado" });

            // Verificar que no sea uno de los estados fundamentales (1, 2)
            if (id <= 2)
            {
                return BadRequest(new { Success = false, Message = "No se pueden eliminar los estados básicos del sistema (activa/eliminada)." });
            }

            // Verificar que no haya propiedades usando este estado
            var estadoConPropiedades = await _estadoRepository.FindAsync(e => e.Id == id);
            var estadoCompleto = estadoConPropiedades.FirstOrDefault();
            if (estadoCompleto != null && estadoCompleto.Propiedades.Any())
            {
                // En lugar de eliminar físicamente, ocultar
                estadoCompleto.Visible = false;
                await _estadoRepository.UpdateAsync(estadoCompleto);
                return Ok(new { Success = true, Message = "Estado ocultado correctamente (hay propiedades que lo usan)." });
            }

            // Si no hay propiedades usando este estado, eliminar físicamente
            await _estadoRepository.DeleteAsync(id);
            return Ok(new { Success = true, Message = "Estado de actividad eliminado correctamente" });
        }

        // PATCH: api/estadopropiedadactividad/{id}/toggle (Mostrar/Ocultar estado)
        [HttpPatch("{id}/toggle")]
        [Authorize(Roles = "Programador")]
        public async Task<IActionResult> ToggleEstado(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            // No permitir ocultar estados fundamentales
            if (id <= 2)
            {
                return BadRequest(new { Success = false, Message = "No se pueden ocultar los estados básicos del sistema (activa/eliminada)." });
            }

            var estado = await _estadoRepository.GetByIdAsync(id);
            if (estado == null)
                return NotFound(new { Success = false, Message = "Estado de actividad no encontrado" });

            estado.Visible = !estado.Visible;
            await _estadoRepository.UpdateAsync(estado);

            string accion = estado.Visible ? "visible" : "oculto";
            return Ok(new { Success = true, Message = $"Estado marcado como {accion} correctamente." });
        }
    }
}
