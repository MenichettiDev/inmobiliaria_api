using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;
using inmobiliariaApi.DTOs.EstadoLead;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EstadoLeadController : ControllerBase
    {
        private readonly GenericRepository<EstadoLead> _estadoRepository;

        public EstadoLeadController(GenericRepository<EstadoLead> estadoRepository)
        {
            _estadoRepository = estadoRepository;
        }

        // GET: api/estadolead (Lista de todos los estados)
        [HttpGet]
        [Authorize(Roles = "Programador,Administrador,Supervisor")] // Solo roles administrativos
        public async Task<IActionResult> GetEstados()
        {
            var estados = await _estadoRepository.GetAllAsync();

            // Mapear a DTO limpio
            var result = estados.Select(e => new EstadoLeadDto
            {
                Id = e.Id,
                Nombre = e.Nombre,
                Activo = e.Activo
            });

            return Ok(new { Success = true, Data = result, Message = "Estados de lead obtenidos correctamente" });
        }

        // GET: api/estadolead/activos (Solo estados activos para operaciones)
        [HttpGet("activos")]
        [Authorize(Roles = "Programador,Administrador,Supervisor,Agente")] // Todos pueden consultar estados activos
        public async Task<IActionResult> GetEstadosActivos()
        {
            var estados = await _estadoRepository.FindAsync(e => e.Activo == true);

            var result = estados.Select(e => new EstadoLeadDto
            {
                Id = e.Id,
                Nombre = e.Nombre,
                Activo = e.Activo
            });

            return Ok(new { Success = true, Data = result, Message = "Estados de lead activos obtenidos correctamente" });
        }

        // GET: api/estadolead/{id} (Un estado específico)
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
                return NotFound(new { Success = false, Message = "Estado de lead no encontrado" });

            var result = new EstadoLeadDto
            {
                Id = estado.Id,
                Nombre = estado.Nombre,
                Activo = estado.Activo
            };

            return Ok(new { Success = true, Data = result, Message = "Estado de lead encontrado" });
        }

        // POST: api/estadolead (Crear un nuevo estado)
        [HttpPost]
        [Authorize(Roles = "Programador")] // Solo Programador puede crear estados
        public async Task<IActionResult> PostEstado([FromBody] CreateEstadoLeadDto estadoDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Success = false, Message = "Datos no válidos", Errors = errors });
            }

            // Validar que el nombre no exista
            var existingNombre = await _estadoRepository.FindAsync(e => e.Nombre.ToLower() == estadoDto.Nombre.ToLower());
            if (existingNombre.Any())
            {
                return BadRequest(new { Success = false, Message = "Ya existe un estado de lead con ese nombre." });
            }

            // Crear un objeto EstadoLead a partir del DTO
            var estado = new EstadoLead
            {
                Nombre = estadoDto.Nombre.Trim(),
                Activo = estadoDto.Activo
            };

            var result = await _estadoRepository.AddAsync(estado);

            var responseDto = new EstadoLeadDto
            {
                Id = result.Id,
                Nombre = result.Nombre,
                Activo = result.Activo
            };

            return CreatedAtAction(nameof(GetEstado), new { id = result.Id },
                new { Success = true, Data = responseDto, Message = "Estado de lead creado correctamente" });
        }

        // PUT: api/estadolead/{id} (Actualizar estado)
        [HttpPut("{id}")]
        [Authorize(Roles = "Programador")] // Solo Programador puede actualizar estados
        public async Task<IActionResult> PutEstado(int id, [FromBody] UpdateEstadoLeadDto estadoDto)
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
                return NotFound(new { Success = false, Message = "Estado de lead no encontrado" });

            // Proteger estados básicos del sistema (1-5) de cambios críticos
            if (id <= 5)
            {
                // Solo permitir cambiar el estado activo/inactivo, no el nombre
                if (estadoDto.Nombre.ToLower() != existingEstado.Nombre.ToLower())
                {
                    return BadRequest(new { Success = false, Message = "No se puede cambiar el nombre de los estados básicos del sistema." });
                }

                // No permitir desactivar el estado "nuevo" (1) que es fundamental
                if (id == 1 && !estadoDto.Activo)
                {
                    return BadRequest(new { Success = false, Message = "No se puede desactivar el estado 'nuevo'." });
                }
            }

            // Validar que el nombre no exista en otro registro
            var existingNombre = await _estadoRepository.FindAsync(e =>
                e.Nombre.ToLower() == estadoDto.Nombre.ToLower() && e.Id != id);
            if (existingNombre.Any())
            {
                return BadRequest(new { Success = false, Message = "Ya existe otro estado con ese nombre." });
            }

            // Actualizar campos
            existingEstado.Nombre = estadoDto.Nombre.Trim();
            existingEstado.Activo = estadoDto.Activo;

            await _estadoRepository.UpdateAsync(existingEstado);

            var responseDto = new EstadoLeadDto
            {
                Id = existingEstado.Id,
                Nombre = existingEstado.Nombre,
                Activo = existingEstado.Activo
            };

            return Ok(new { Success = true, Data = responseDto, Message = "Estado de lead actualizado correctamente" });
        }

        // DELETE: api/estadolead/{id} (Eliminar estado)
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
                return NotFound(new { Success = false, Message = "Estado de lead no encontrado" });

            // Verificar que no sea uno de los estados fundamentales (1-5)
            if (id <= 5)
            {
                return BadRequest(new { Success = false, Message = "No se pueden eliminar los estados básicos del sistema (nuevo, contactado, visitó, cerrado, perdido)." });
            }

            // Verificar que no haya leads usando este estado
            var estadoConLeads = await _estadoRepository.FindAsync(e => e.Id == id);
            var estadoCompleto = estadoConLeads.FirstOrDefault();
            if (estadoCompleto != null && estadoCompleto.Leads.Any())
            {
                // En lugar de eliminar físicamente, desactivar
                estadoCompleto.Activo = false;
                await _estadoRepository.UpdateAsync(estadoCompleto);
                return Ok(new { Success = true, Message = "Estado desactivado correctamente (hay leads que lo usan)." });
            }

            // Si no hay leads usando este estado, eliminar físicamente
            await _estadoRepository.DeleteAsync(id);
            return Ok(new { Success = true, Message = "Estado de lead eliminado correctamente" });
        }

        // PATCH: api/estadolead/{id}/toggle (Activar/Desactivar estado)
        [HttpPatch("{id}/toggle")]
        [Authorize(Roles = "Programador")]
        public async Task<IActionResult> ToggleEstado(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            // No permitir desactivar el estado "nuevo" (1)
            if (id == 1)
            {
                return BadRequest(new { Success = false, Message = "No se puede desactivar el estado 'nuevo'." });
            }

            var estado = await _estadoRepository.GetByIdAsync(id);
            if (estado == null)
                return NotFound(new { Success = false, Message = "Estado de lead no encontrado" });

            estado.Activo = !estado.Activo;
            await _estadoRepository.UpdateAsync(estado);

            string accion = estado.Activo ? "activado" : "desactivado";
            return Ok(new { Success = true, Message = $"Estado de lead {accion} correctamente." });
        }
    }
}
