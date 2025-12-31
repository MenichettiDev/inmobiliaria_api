using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;
using inmobiliariaApi.DTOs.EstadoLeadAdmin;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EstadoLeadAdminController : ControllerBase
    {
        private readonly GenericRepository<EstadoLeadAdmin> _estadoRepository;

        public EstadoLeadAdminController(GenericRepository<EstadoLeadAdmin> estadoRepository)
        {
            _estadoRepository = estadoRepository;
        }

        // GET: api/estadoleadadmin (Lista de todos los estados)
        [HttpGet]
        [Authorize(Roles = "Programador,Administrador,Supervisor")] // Solo roles administrativos
        public async Task<IActionResult> GetEstados()
        {
            var estados = await _estadoRepository.GetAllAsync();

            // Mapear a DTO limpio
            var result = estados.Select(e => new EstadoLeadAdminDto
            {
                Id = e.Id,
                Codigo = e.Codigo,
                Descripcion = e.Descripcion,
                Activo = e.Activo
            });

            return Ok(new { Success = true, Data = result, Message = "Estados administrativos de lead obtenidos correctamente" });
        }

        // GET: api/estadoleadadmin/activos (Solo estados activos para operaciones)
        [HttpGet("activos")]
        [Authorize(Roles = "Programador,Administrador,Supervisor,Agente")] // Todos pueden consultar estados activos
        public async Task<IActionResult> GetEstadosActivos()
        {
            var estados = await _estadoRepository.FindAsync(e => e.Activo == true);

            var result = estados.Select(e => new EstadoLeadAdminDto
            {
                Id = e.Id,
                Codigo = e.Codigo,
                Descripcion = e.Descripcion,
                Activo = e.Activo
            });

            return Ok(new { Success = true, Data = result, Message = "Estados administrativos activos obtenidos correctamente" });
        }

        // GET: api/estadoleadadmin/{id} (Un estado específico)
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
                return NotFound(new { Success = false, Message = "Estado administrativo de lead no encontrado" });

            var result = new EstadoLeadAdminDto
            {
                Id = estado.Id,
                Codigo = estado.Codigo,
                Descripcion = estado.Descripcion,
                Activo = estado.Activo
            };

            return Ok(new { Success = true, Data = result, Message = "Estado administrativo de lead encontrado" });
        }

        // POST: api/estadoleadadmin (Crear un nuevo estado)
        [HttpPost]
        [Authorize(Roles = "Programador")] // Solo Programador puede crear estados
        public async Task<IActionResult> PostEstado([FromBody] CreateEstadoLeadAdminDto estadoDto)
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
                return BadRequest(new { Success = false, Message = "Ya existe un estado administrativo con ese código." });
            }

            // Validar que la descripción no exista
            var existingDescripcion = await _estadoRepository.FindAsync(e => e.Descripcion.ToLower() == estadoDto.Descripcion.ToLower());
            if (existingDescripcion.Any())
            {
                return BadRequest(new { Success = false, Message = "Ya existe un estado administrativo con esa descripción." });
            }

            // Crear un objeto EstadoLeadAdmin a partir del DTO
            var estado = new EstadoLeadAdmin
            {
                Codigo = estadoDto.Codigo.Trim(),
                Descripcion = estadoDto.Descripcion.Trim(),
                Activo = estadoDto.Activo
            };

            var result = await _estadoRepository.AddAsync(estado);

            var responseDto = new EstadoLeadAdminDto
            {
                Id = result.Id,
                Codigo = result.Codigo,
                Descripcion = result.Descripcion,
                Activo = result.Activo
            };

            return CreatedAtAction(nameof(GetEstado), new { id = result.Id },
                new { Success = true, Data = responseDto, Message = "Estado administrativo de lead creado correctamente" });
        }

        // PUT: api/estadoleadadmin/{id} (Actualizar estado)
        [HttpPut("{id}")]
        [Authorize(Roles = "Programador")] // Solo Programador puede actualizar estados
        public async Task<IActionResult> PutEstado(int id, [FromBody] UpdateEstadoLeadAdminDto estadoDto)
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
                return NotFound(new { Success = false, Message = "Estado administrativo de lead no encontrado" });

            // Proteger estados básicos del sistema (1-3) de cambios críticos
            if (id <= 3)
            {
                // Solo permitir cambiar el estado activo/inactivo, no el código ni la descripción
                if (estadoDto.Codigo.ToLower() != existingEstado.Codigo.ToLower())
                {
                    return BadRequest(new { Success = false, Message = "No se puede cambiar el código de los estados básicos del sistema." });
                }

                // No permitir desactivar el estado "activo" (1)
                if (id == 1 && !estadoDto.Activo)
                {
                    return BadRequest(new { Success = false, Message = "No se puede desactivar el estado 'activo'." });
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
            existingEstado.Activo = estadoDto.Activo;

            await _estadoRepository.UpdateAsync(existingEstado);

            var responseDto = new EstadoLeadAdminDto
            {
                Id = existingEstado.Id,
                Codigo = existingEstado.Codigo,
                Descripcion = existingEstado.Descripcion,
                Activo = existingEstado.Activo
            };

            return Ok(new { Success = true, Data = responseDto, Message = "Estado administrativo actualizado correctamente" });
        }

        // DELETE: api/estadoleadadmin/{id} (Eliminar estado)
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
                return NotFound(new { Success = false, Message = "Estado administrativo de lead no encontrado" });

            // Verificar que no sea uno de los estados fundamentales (1-3)
            if (id <= 3)
            {
                return BadRequest(new { Success = false, Message = "No se pueden eliminar los estados básicos del sistema (activo, archivado, eliminado)." });
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
            return Ok(new { Success = true, Message = "Estado administrativo de lead eliminado correctamente" });
        }

        // PATCH: api/estadoleadadmin/{id}/toggle (Activar/Desactivar estado)
        [HttpPatch("{id}/toggle")]
        [Authorize(Roles = "Programador")]
        public async Task<IActionResult> ToggleEstado(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            // No permitir desactivar el estado "activo" (1)
            if (id == 1)
            {
                return BadRequest(new { Success = false, Message = "No se puede desactivar el estado 'activo'." });
            }

            var estado = await _estadoRepository.GetByIdAsync(id);
            if (estado == null)
                return NotFound(new { Success = false, Message = "Estado administrativo de lead no encontrado" });

            estado.Activo = !estado.Activo;
            await _estadoRepository.UpdateAsync(estado);

            string accion = estado.Activo ? "activado" : "desactivado";
            return Ok(new { Success = true, Message = $"Estado administrativo {accion} correctamente." });
        }
    }
}
