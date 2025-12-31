using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;
using inmobiliariaApi.DTOs.EstadoPropiedadOperativo;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EstadoPropiedadOperativoController : ControllerBase
    {
        private readonly GenericRepository<EstadoPropiedadOperativo> _estadoRepository;

        public EstadoPropiedadOperativoController(GenericRepository<EstadoPropiedadOperativo> estadoRepository)
        {
            _estadoRepository = estadoRepository;
        }

        // GET: api/estadopropiedadoperativo (Lista de todos los estados)
        [HttpGet]
        [Authorize(Roles = "Programador,Administrador,Supervisor")] // Solo roles administrativos
        public async Task<IActionResult> GetEstados()
        {
            var estados = await _estadoRepository.GetAllAsync();

            // Mapear a DTO limpio
            var result = estados.Select(e => new EstadoPropiedadOperativoDto
            {
                Id = e.Id,
                Codigo = e.Codigo,
                Nombre = e.Nombre,
                Descripcion = e.Descripcion,
                Activo = e.Activo,
                ColorHex = e.ColorHex
            });

            return Ok(new { Success = true, Data = result, Message = "Estados operativos obtenidos correctamente" });
        }

        // GET: api/estadopropiedadoperativo/activos (Solo estados activos para seleccionar en formularios)
        [HttpGet("activos")]
        [Authorize(Roles = "Programador,Administrador,Supervisor,Agente")] // Todos pueden consultar estados activos
        public async Task<IActionResult> GetEstadosActivos()
        {
            var estados = await _estadoRepository.FindAsync(e => e.Activo == true);

            var result = estados.Select(e => new EstadoPropiedadOperativoDto
            {
                Id = e.Id,
                Codigo = e.Codigo,
                Nombre = e.Nombre,
                Descripcion = e.Descripcion,
                Activo = e.Activo,
                ColorHex = e.ColorHex
            });

            return Ok(new { Success = true, Data = result, Message = "Estados operativos activos obtenidos correctamente" });
        }

        // GET: api/estadopropiedadoperativo/{id} (Un estado específico)
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
                return NotFound(new { Success = false, Message = "Estado operativo no encontrado" });

            var result = new EstadoPropiedadOperativoDto
            {
                Id = estado.Id,
                Codigo = estado.Codigo,
                Nombre = estado.Nombre,
                Descripcion = estado.Descripcion,
                Activo = estado.Activo,
                ColorHex = estado.ColorHex
            };

            return Ok(new { Success = true, Data = result, Message = "Estado operativo encontrado" });
        }

        // POST: api/estadopropiedadoperativo (Crear un nuevo estado)
        [HttpPost]
        [Authorize(Roles = "Programador")] // Solo Programador puede crear estados
        public async Task<IActionResult> PostEstado([FromBody] CreateEstadoPropiedadOperativoDto estadoDto)
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
                return BadRequest(new { Success = false, Message = "Ya existe un estado operativo con ese código." });
            }

            // Validar que el nombre no exista
            var existingNombre = await _estadoRepository.FindAsync(e => e.Nombre.ToLower() == estadoDto.Nombre.ToLower());
            if (existingNombre.Any())
            {
                return BadRequest(new { Success = false, Message = "Ya existe un estado operativo con ese nombre." });
            }

            // Crear un objeto EstadoPropiedadOperativo a partir del DTO
            var estado = new EstadoPropiedadOperativo
            {
                Codigo = estadoDto.Codigo.Trim(),
                Nombre = estadoDto.Nombre.Trim(),
                Descripcion = estadoDto.Descripcion?.Trim(),
                Activo = estadoDto.Activo,
                ColorHex = estadoDto.ColorHex?.Trim() ?? "#CCCCCC"
            };

            var result = await _estadoRepository.AddAsync(estado);

            var responseDto = new EstadoPropiedadOperativoDto
            {
                Id = result.Id,
                Codigo = result.Codigo,
                Nombre = result.Nombre,
                Descripcion = result.Descripcion,
                Activo = result.Activo,
                ColorHex = result.ColorHex
            };

            return CreatedAtAction(nameof(GetEstado), new { id = result.Id },
                new { Success = true, Data = responseDto, Message = "Estado operativo creado correctamente" });
        }

        // PUT: api/estadopropiedadoperativo/{id} (Actualizar estado)
        [HttpPut("{id}")]
        [Authorize(Roles = "Programador")] // Solo Programador puede actualizar estados
        public async Task<IActionResult> PutEstado(int id, [FromBody] UpdateEstadoPropiedadOperativoDto estadoDto)
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
                return NotFound(new { Success = false, Message = "Estado operativo no encontrado" });

            // Validar que el código no exista en otro registro
            var existingCodigo = await _estadoRepository.FindAsync(e =>
                e.Codigo.ToLower() == estadoDto.Codigo.ToLower() && e.Id != id);
            if (existingCodigo.Any())
            {
                return BadRequest(new { Success = false, Message = "Ya existe otro estado operativo con ese código." });
            }

            // Validar que el nombre no exista en otro registro
            var existingNombre = await _estadoRepository.FindAsync(e =>
                e.Nombre.ToLower() == estadoDto.Nombre.ToLower() && e.Id != id);
            if (existingNombre.Any())
            {
                return BadRequest(new { Success = false, Message = "Ya existe otro estado operativo con ese nombre." });
            }

            // Actualizar campos
            existingEstado.Codigo = estadoDto.Codigo.Trim();
            existingEstado.Nombre = estadoDto.Nombre.Trim();
            existingEstado.Descripcion = estadoDto.Descripcion?.Trim();
            existingEstado.Activo = estadoDto.Activo;
            existingEstado.ColorHex = estadoDto.ColorHex?.Trim() ?? "#CCCCCC";

            await _estadoRepository.UpdateAsync(existingEstado);

            var responseDto = new EstadoPropiedadOperativoDto
            {
                Id = existingEstado.Id,
                Codigo = existingEstado.Codigo,
                Nombre = existingEstado.Nombre,
                Descripcion = existingEstado.Descripcion,
                Activo = existingEstado.Activo,
                ColorHex = existingEstado.ColorHex
            };

            return Ok(new { Success = true, Data = responseDto, Message = "Estado operativo actualizado correctamente" });
        }

        // DELETE: api/estadopropiedadoperativo/{id} (Eliminar estado - solo desactivar)
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
                return NotFound(new { Success = false, Message = "Estado operativo no encontrado" });

            // Verificar que no sea uno de los estados fundamentales (1, 2, 3, 4)
            if (id <= 4)
            {
                return BadRequest(new { Success = false, Message = "No se pueden eliminar los estados básicos del sistema." });
            }

            // Verificar que no haya propiedades usando este estado
            var estadoConPropiedades = await _estadoRepository.FindAsync(e => e.Id == id);
            var estadoCompleto = estadoConPropiedades.FirstOrDefault();
            if (estadoCompleto != null && estadoCompleto.Propiedades.Any())
            {
                // En lugar de eliminar físicamente, desactivar
                estadoCompleto.Activo = false;
                await _estadoRepository.UpdateAsync(estadoCompleto);
                return Ok(new { Success = true, Message = "Estado desactivado correctamente (hay propiedades que lo usan)." });
            }

            // Si no hay propiedades usando este estado, eliminar físicamente
            await _estadoRepository.DeleteAsync(id);
            return Ok(new { Success = true, Message = "Estado operativo eliminado correctamente" });
        }

        // PATCH: api/estadopropiedadoperativo/{id}/toggle (Activar/Desactivar estado)
        [HttpPatch("{id}/toggle")]
        [Authorize(Roles = "Programador")]
        public async Task<IActionResult> ToggleEstado(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            // No permitir desactivar estados fundamentales
            if (id <= 4)
            {
                return BadRequest(new { Success = false, Message = "No se pueden desactivar los estados básicos del sistema." });
            }

            var estado = await _estadoRepository.GetByIdAsync(id);
            if (estado == null)
                return NotFound(new { Success = false, Message = "Estado operativo no encontrado" });

            estado.Activo = !estado.Activo;
            await _estadoRepository.UpdateAsync(estado);

            string accion = estado.Activo ? "activado" : "desactivado";
            return Ok(new { Success = true, Message = $"Estado operativo {accion} correctamente." });
        }
    }
}
