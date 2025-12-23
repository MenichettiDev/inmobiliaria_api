using inmobiliariaApi.Data;
using inmobiliariaApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace inmobiliariaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requiere autenticación para todo el controller
    public class ImagenController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ImagenController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/imagen (Lista de imágenes)
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden ver imágenes
        public async Task<IActionResult> GetImagenes()
        {
            var imagenes = await _context.ImagenPropiedad.ToListAsync();
            var totalImagenes = imagenes.Count;

            return Ok(
                new
                {
                    status = 200,
                    message = totalImagenes > 0
                        ? "Lista de imágenes obtenida correctamente."
                        : "No hay imágenes disponibles.",
                    totalImagenes,
                    imagenes = imagenes ?? new List<ImagenPropiedad>(), // esto es para evitar nulls
                }
            );
        }

        // GET: api/imagen/{id} (Una imagen específica)
        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden ver imágenes específicas
        public async Task<IActionResult> GetImagen(int id)
        {
            var imagen = await _context.ImagenPropiedad.FindAsync(id);
            if (imagen == null)
            {
                return NotFound(
                    new
                    {
                        status = 404,
                        error = "Not Found",
                        message = "La imagen no existe.",
                    }
                );
            }

            return Ok(
                new
                {
                    status = 200,
                    message = "Imagen encontrada.",
                    imagen,
                }
            );
        }

        // POST: api/imagen (Subir nueva imagen)
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden subir imágenes
        public async Task<IActionResult> PostImagen(ImagenPropiedad imagen)
        {
            if (string.IsNullOrEmpty(imagen.Url))
            {
                return BadRequest(
                    new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "La ruta de la imagen es obligatoria.",
                    }
                );
            }

            imagen.Id = 0; // Aseguramos que la DB genere el ID automáticamente
            _context.ImagenPropiedad.Add(imagen);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetImagen),
                new { id = imagen.Id },
                new
                {
                    status = 201,
                    message = "Imagen subida correctamente.",
                    imagen,
                }
            );
        }

        // PUT: api/imagen/{id} (Actualizar imagen)
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden actualizar imágenes
        public async Task<IActionResult> PutImagen(int id, ImagenPropiedad imagen)
        {
            if (id != imagen.Id)
            {
                return BadRequest(
                    new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "El ID en la URL no coincide con el ID del cuerpo de la solicitud.",
                    }
                );
            }

            _context.Entry(imagen).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ImagenPropiedad.Any(e => e.Id == id))
                {
                    return NotFound(
                        new
                        {
                            status = 404,
                            error = "Not Found",
                            message = "La imagen no existe.",
                        }
                    );
                }
                throw;
            }

            return Ok(
                new
                {
                    status = 200,
                    message = "Imagen actualizada correctamente.",
                    imagen,
                }
            );
        }

        // DELETE: api/imagen/{id} (Eliminar imagen)
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede eliminar imágenes
        public async Task<IActionResult> DeleteImagen(int id)
        {
            var existeImagen = await _context.ImagenPropiedad.AnyAsync(i => i.Id == id);
            if (!existeImagen)
            {
                return NotFound(
                    new
                    {
                        status = 404,
                        error = "Not Found",
                        message = "La imagen no existe.",
                    }
                );
            }

            var imagen = await _context.ImagenPropiedad.FindAsync(id);
            if (imagen == null)
            {
                return NotFound(new { status = 404, message = "Imagen no encontrada." });
            }
            _context.ImagenPropiedad.Remove(imagen);
            await _context.SaveChangesAsync();

            return Ok(
                new
                {
                    status = 200,
                    message = "Imagen eliminada correctamente.",
                    imagenEliminadaId = id,
                    detalles = "No hay referencias activas, eliminación exitosa.",
                }
            );
        }
    }
}
