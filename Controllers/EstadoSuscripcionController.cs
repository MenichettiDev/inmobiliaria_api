using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;
using inmobiliariaApi.DTOs.EstadoSuscripcion;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EstadoSuscripcionController : ControllerBase
    {
        private readonly GenericRepository<EstadoSuscripcion> _estadoRepository;

        public EstadoSuscripcionController(GenericRepository<EstadoSuscripcion> estadoRepository)
        {
            _estadoRepository = estadoRepository;
        }

        // GET: api/estadosuscripcion (Lista de todos los estados)
        [HttpGet]
        [Authorize(Roles = "Programador,Administrador")] // Solo roles de alta administración
        public async Task<IActionResult> GetEstados()
        {
            var estados = await _estadoRepository.GetAllAsync();

            // Mapear a DTO limpio
            var result = estados.Select(e => new EstadoSuscripcionDto
            {
                Id = e.Id,
                Codigo = e.Codigo,
                Descripcion = e.Descripcion,
                PermiteOperar = e.PermiteOperar
            });

            return Ok(new { Success = true, Data = result, Message = "Estados de suscripción obtenidos correctamente" });
        }

        // GET: api/estadosuscripcion/operativos (Solo estados que permiten operar)
        [HttpGet("operativos")]
        [Authorize(Roles = "Programador,Administrador,Supervisor")] // Para consultas de negocio
        public async Task<IActionResult> GetEstadosOperativos()
        {
            var estados = await _estadoRepository.FindAsync(e => e.PermiteOperar == true);

            var result = estados.Select(e => new EstadoSuscripcionDto
            {
                Id = e.Id,
                Codigo = e.Codigo,
                Descripcion = e.Descripcion,
                PermiteOperar = e.PermiteOperar
            });

            return Ok(new { Success = true, Data = result, Message = "Estados operativos obtenidos correctamente" });
        }

        // GET: api/estadosuscripcion/{id} (Un estado específico)
        [HttpGet("{id}")]
        [Authorize(Roles = "Programador,Administrador")]
        public async Task<IActionResult> GetEstado(byte id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "El ID debe ser mayor a 0." });
            }

            var estado = await _estadoRepository.GetByIdAsync(id);
            if (estado == null)
                return NotFound(new { Success = false, Message = "Estado de suscripción no encontrado" });

            var result = new EstadoSuscripcionDto
            {
                Id = estado.Id,
                Codigo = estado.Codigo,
                Descripcion = estado.Descripcion,
                PermiteOperar = estado.PermiteOperar
            };

            return Ok(new { Success = true, Data = result, Message = "Estado de suscripción encontrado" });
        }

        // POST: api/estadosuscripcion (Crear un nuevo estado)
        [HttpPost]
        [Authorize(Roles = "Programador")] // Solo Programador puede crear estados de suscripción
        public async Task<IActionResult> PostEstado([FromBody] CreateEstadoSuscripcionDto estadoDto)
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
                return BadRequest(new { Success = false, Message = "Ya existe un estado de suscripción con ese código." });
            }

            // Validar que la descripción no exista
            var existingDescripcion = await _estadoRepository.FindAsync(e => e.Descripcion.ToLower() == estadoDto.Descripcion.ToLower());
            if (existingDescripcion.Any())
            {
                return BadRequest(new { Success = false, Message = "Ya existe un estado de suscripción con esa descripción." });
            }

            // Crear un objeto EstadoSuscripcion a partir del DTO
            var estado = new EstadoSuscripcion
            {
                Codigo = estadoDto.Codigo.Trim(),
                Descripcion = estadoDto.Descripcion.Trim(),
                PermiteOperar = estadoDto.PermiteOperar
            };

            var result = await _estadoRepository.AddAsync(estado);

            var responseDto = new EstadoSuscripcionDto
            {
                Id = result.Id,
                Codigo = result.Codigo,
                Descripcion = result.Descripcion,
                PermiteOperar = result.PermiteOperar
            };

            return CreatedAtAction(nameof(GetEstado), new { id = result.Id },
                new { Success = true, Data = responseDto, Message = "Estado de suscripción creado correctamente" });
        }

        // PUT: api/estadosuscripcion/{id} (Actualizar estado)
        [HttpPut("{id}")]
        [Authorize(Roles = "Programador")] // Solo Programador puede actualizar estados
        public async Task<IActionResult> PutEstado(byte id, [FromBody] UpdateEstadoSuscripcionDto estadoDto)
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
                return NotFound(new { Success = false, Message = "Estado de suscripción no encontrado" });

            // Proteger estados básicos del sistema (1-4) de cambios críticos
            if (id <= 4)
            {
                // Solo permitir cambiar la descripción, no el código ni la funcionalidad operativa
                if (estadoDto.Codigo.ToLower() != existingEstado.Codigo.ToLower())
                {
                    return BadRequest(new { Success = false, Message = "No se puede cambiar el código de los estados básicos del sistema." });
                }

                // Solo permitir cambiar PermiteOperar en casos muy específicos
                if (id == 1 && !estadoDto.PermiteOperar) // Estado "activa" siempre debe permitir operar
                {
                    return BadRequest(new { Success = false, Message = "El estado 'activa' siempre debe permitir operar." });
                }
            }

            // Validar que el código no exista en otro registro
            var existingCodigo = await _estadoRepository.FindAsync(e =>
                e.Codigo.ToLower() == estadoDto.Codigo.ToLower() && e.Id != id);
            if (existingCodigo.Any())
            {
                return BadRequest(new { Success = false, Message = "Ya existe otro estado con ese código." });
            }

            // Validar que la descripción no exista en otro registro
            var existingDescripcion = await _estadoRepository.FindAsync(e =>
                e.Descripcion.ToLower() == estadoDto.Descripcion.ToLower() && e.Id != id);
            if (existingDescripcion.Any())
            {
                return BadRequest(new { Success = false, Message = "Ya existe otro estado con esa descripción." });
            }

            // Actualizar campos
            existingEstado.Codigo = estadoDto.Codigo.Trim();
            existingEstado.Descripcion = estadoDto.Descripcion.Trim();
            existingEstado.PermiteOperar = estadoDto.PermiteOperar;

            await _estadoRepository.UpdateAsync(existingEstado);

            var responseDto = new EstadoSuscripcionDto
            {
                Id = existingEstado.Id,
                Codigo = existingEstado.Codigo,
                Descripcion = existingEstado.Descripcion,
                PermiteOperar = existingEstado.PermiteOperar
            };

            return Ok(new { Success = true, Data = responseDto, Message = "Estado de suscripción actualizado correctamente" });
        }

        // DELETE: api/estadosuscripcion/{id} (Eliminar estado)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Programador")] // Solo Programador puede eliminar estados
        public async Task<IActionResult> DeleteEstado(byte id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            var estado = await _estadoRepository.GetByIdAsync(id);
            if (estado == null)
                return NotFound(new { Success = false, Message = "Estado de suscripción no encontrado" });

            // Verificar que no sea uno de los estados fundamentales (1-4)
            if (id <= 4)
            {
                return BadRequest(new { Success = false, Message = "No se pueden eliminar los estados básicos del sistema (activa, pausada, vencida, cancelada)." });
            }

            // Verificar que no haya suscripciones usando este estado
            var estadoConSuscripciones = await _estadoRepository.FindAsync(e => e.Id == id);
            var estadoCompleto = estadoConSuscripciones.FirstOrDefault();
            if (estadoCompleto != null && estadoCompleto.Suscripciones.Any())
            {
                return BadRequest(new { Success = false, Message = "No se puede eliminar el estado porque hay suscripciones que lo están usando." });
            }

            // Si no hay suscripciones usando este estado, eliminar físicamente
            await _estadoRepository.DeleteAsync(id);
            return Ok(new { Success = true, Message = "Estado de suscripción eliminado correctamente" });
        }

        // PATCH: api/estadosuscripcion/{id}/toggle-operacion (Activar/Desactivar operación)
        [HttpPatch("{id}/toggle-operacion")]
        [Authorize(Roles = "Programador")]
        public async Task<IActionResult> ToggleOperacion(byte id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            // No permitir desactivar operación del estado "activa"
            if (id == 1)
            {
                return BadRequest(new { Success = false, Message = "No se puede desactivar la operación del estado 'activa'." });
            }

            var estado = await _estadoRepository.GetByIdAsync(id);
            if (estado == null)
                return NotFound(new { Success = false, Message = "Estado de suscripción no encontrado" });

            estado.PermiteOperar = !estado.PermiteOperar;
            await _estadoRepository.UpdateAsync(estado);

            string accion = estado.PermiteOperar ? "permite" : "no permite";
            return Ok(new { Success = true, Message = $"Estado actualizado: ahora {accion} operar." });
        }
    }
}
