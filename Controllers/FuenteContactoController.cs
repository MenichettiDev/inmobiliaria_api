using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;
using inmobiliariaApi.DTOs.FuenteContacto;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FuenteContactoController : ControllerBase
    {
        private readonly GenericRepository<FuenteContacto> _fuenteRepository;

        public FuenteContactoController(GenericRepository<FuenteContacto> fuenteRepository)
        {
            _fuenteRepository = fuenteRepository;
        }

        // GET: api/fuentecontacto (Lista de todas las fuentes)
        [HttpGet]
        [Authorize(Roles = "Programador,Administrador,Supervisor,Agente")] // Todos pueden consultar fuentes
        public async Task<IActionResult> GetFuentes()
        {
            var fuentes = await _fuenteRepository.GetAllAsync();

            // Mapear a DTO limpio
            var result = fuentes.Select(f => new FuenteContactoDto
            {
                Id = f.Id,
                Nombre = f.Nombre
            }).OrderBy(f => f.Nombre);

            return Ok(new { Success = true, Data = result, Message = "Fuentes de contacto obtenidas correctamente" });
        }

        // GET: api/fuentecontacto/{id} (Una fuente específica)
        [HttpGet("{id}")]
        [Authorize(Roles = "Programador,Administrador,Supervisor")]
        public async Task<IActionResult> GetFuente(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "El ID debe ser mayor a 0." });
            }

            var fuente = await _fuenteRepository.GetByIdAsync(id);
            if (fuente == null)
                return NotFound(new { Success = false, Message = "Fuente de contacto no encontrada" });

            var result = new FuenteContactoDto
            {
                Id = fuente.Id,
                Nombre = fuente.Nombre
            };

            return Ok(new { Success = true, Data = result, Message = "Fuente de contacto encontrada" });
        }

        // POST: api/fuentecontacto (Crear una nueva fuente)
        [HttpPost]
        [Authorize(Roles = "Programador,Administrador")] // Solo Programador y Administrador pueden crear fuentes
        public async Task<IActionResult> PostFuente([FromBody] CreateFuenteContactoDto fuenteDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Success = false, Message = "Datos no válidos", Errors = errors });
            }

            // Validar que el nombre no exista
            var existingNombre = await _fuenteRepository.FindAsync(f => f.Nombre.ToLower() == fuenteDto.Nombre.ToLower());
            if (existingNombre.Any())
            {
                return BadRequest(new { Success = false, Message = "Ya existe una fuente de contacto con ese nombre." });
            }

            // Crear un objeto FuenteContacto a partir del DTO
            var fuente = new FuenteContacto
            {
                Nombre = fuenteDto.Nombre.Trim()
            };

            var result = await _fuenteRepository.AddAsync(fuente);

            var responseDto = new FuenteContactoDto
            {
                Id = result.Id,
                Nombre = result.Nombre
            };

            return CreatedAtAction(nameof(GetFuente), new { id = result.Id },
                new { Success = true, Data = responseDto, Message = "Fuente de contacto creada correctamente" });
        }

        // PUT: api/fuentecontacto/{id} (Actualizar fuente)
        [HttpPut("{id}")]
        [Authorize(Roles = "Programador,Administrador")] // Solo Programador y Administrador pueden actualizar
        public async Task<IActionResult> PutFuente(int id, [FromBody] UpdateFuenteContactoDto fuenteDto)
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

            if (id != fuenteDto.Id)
                return BadRequest(new { Success = false, Message = "El ID de la URL no coincide con el ID del objeto" });

            var existingFuente = await _fuenteRepository.GetByIdAsync(id);
            if (existingFuente == null)
                return NotFound(new { Success = false, Message = "Fuente de contacto no encontrada" });

            // Proteger fuentes básicas del sistema (1-8) de cambios de nombre
            if (id <= 8)
            {
                // Solo permitir cambios menores, no cambios de nombre para fuentes básicas conocidas
                var fuentesBasicas = new[] { "directo", "google", "facebook", "instagram", "idealista", "zillow", "whatsapp", "referido" };
                if (fuentesBasicas.Contains(existingFuente.Nombre.ToLower()) &&
                    fuenteDto.Nombre.ToLower() != existingFuente.Nombre.ToLower())
                {
                    return BadRequest(new { Success = false, Message = "No se puede cambiar el nombre de las fuentes básicas del sistema." });
                }
            }

            // Validar que el nombre no exista en otro registro
            var existingNombre = await _fuenteRepository.FindAsync(f =>
                f.Nombre.ToLower() == fuenteDto.Nombre.ToLower() && f.Id != id);
            if (existingNombre.Any())
            {
                return BadRequest(new { Success = false, Message = "Ya existe otra fuente con ese nombre." });
            }

            // Actualizar campos
            existingFuente.Nombre = fuenteDto.Nombre.Trim();

            await _fuenteRepository.UpdateAsync(existingFuente);

            var responseDto = new FuenteContactoDto
            {
                Id = existingFuente.Id,
                Nombre = existingFuente.Nombre
            };

            return Ok(new { Success = true, Data = responseDto, Message = "Fuente de contacto actualizada correctamente" });
        }

        // DELETE: api/fuentecontacto/{id} (Eliminar fuente)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Programador")] // Solo Programador puede eliminar fuentes
        public async Task<IActionResult> DeleteFuente(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            var fuente = await _fuenteRepository.GetByIdAsync(id);
            if (fuente == null)
                return NotFound(new { Success = false, Message = "Fuente de contacto no encontrada" });

            // Verificar que no sea una de las fuentes fundamentales (1-8)
            if (id <= 8)
            {
                return BadRequest(new { Success = false, Message = "No se pueden eliminar las fuentes básicas del sistema (directo, google, facebook, instagram, idealista, zillow, whatsapp, referido)." });
            }

            // Verificar que no haya leads usando esta fuente
            var fuenteConLeads = await _fuenteRepository.FindAsync(f => f.Id == id);
            var fuenteCompleta = fuenteConLeads.FirstOrDefault();
            if (fuenteCompleta != null && fuenteCompleta.Leads.Any())
            {
                return BadRequest(new { Success = false, Message = "No se puede eliminar la fuente porque hay leads que la están usando." });
            }

            // Si no hay leads usando esta fuente, eliminar físicamente
            await _fuenteRepository.DeleteAsync(id);
            return Ok(new { Success = true, Message = "Fuente de contacto eliminada correctamente" });
        }

        // GET: api/fuentecontacto/populares (Fuentes más utilizadas)
        [HttpGet("populares")]
        [Authorize(Roles = "Programador,Administrador,Supervisor")]
        public async Task<IActionResult> GetFuentesPopulares()
        {
            var fuentes = await _fuenteRepository.GetAllAsync();

            // Obtener las fuentes más comunes ordenadas por uso
            var fuentesPopulares = fuentes
                .Select(f => new
                {
                    Id = f.Id,
                    Nombre = f.Nombre,
                    TotalLeads = f.Leads?.Count ?? 0
                })
                .OrderByDescending(f => f.TotalLeads)
                .Take(10)
                .ToList();

            return Ok(new { Success = true, Data = fuentesPopulares, Message = "Fuentes populares obtenidas correctamente" });
        }
    }
}
