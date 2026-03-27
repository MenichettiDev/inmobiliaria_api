using Microsoft.EntityFrameworkCore;
using inmobiliariaApi.Data;
using inmobiliariaApi.Models;
using System.Linq.Expressions;

namespace inmobiliariaApi.Repositories
{
    public class UsuarioWebRepository : GenericRepository<UsuarioWeb>
    {
        public UsuarioWebRepository(ApplicationDbContext context) : base(context) { }

        /// <summary>
        /// Obtiene un usuario web por email
        /// </summary>
        public async Task<UsuarioWeb?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email && u.Activo);
        }

        /// <summary>
        /// Obtiene un usuario web por Google ID
        /// </summary>
        public async Task<UsuarioWeb?> GetByGoogleIdAsync(string googleId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.GoogleId == googleId && u.Activo);
        }

        /// <summary>
        /// Obtiene los favoritos de un usuario web
        /// </summary>
        public async Task<IEnumerable<PropiedadFavorita>> GetFavoritosAsync(int idUsuarioWeb)
        {
            return await _context.PropiedadesFavoritas
                .Where(pf => pf.IdUsuarioWeb == idUsuarioWeb)
                .Include(pf => pf.Propiedad)
                .OrderByDescending(pf => pf.CreadoEn)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene un favorito específico
        /// </summary>
        public async Task<PropiedadFavorita?> GetFavoritoAsync(int idUsuarioWeb, int idPropiedad)
        {
            return await _context.PropiedadesFavoritas
                .FirstOrDefaultAsync(pf => pf.IdUsuarioWeb == idUsuarioWeb && pf.IdPropiedad == idPropiedad);
        }

        /// <summary>
        /// Verifica si una propiedad es favorita de un usuario
        /// </summary>
        public async Task<bool> IsFavoritoAsync(int idUsuarioWeb, int idPropiedad)
        {
            return await _context.PropiedadesFavoritas
                .AnyAsync(pf => pf.IdUsuarioWeb == idUsuarioWeb && pf.IdPropiedad == idPropiedad);
        }

        /// <summary>
        /// Agrega un favorito
        /// </summary>
        public async Task<PropiedadFavorita> AddFavoritoAsync(PropiedadFavorita favorito)
        {
            await _context.PropiedadesFavoritas.AddAsync(favorito);
            await _context.SaveChangesAsync();
            return favorito;
        }

        /// <summary>
        /// Remueve un favorito
        /// </summary>
        public async Task RemoveFavoritoAsync(int idUsuarioWeb, int idPropiedad)
        {
            var favorito = await GetFavoritoAsync(idUsuarioWeb, idPropiedad);
            if (favorito != null)
            {
                _context.PropiedadesFavoritas.Remove(favorito);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Obtiene todas las consultas (leads) enviadas por un usuario web
        /// </summary>
        public async Task<IEnumerable<Lead>> GetConsultasAsync(int idUsuarioWeb)
        {
            return await _context.Lead
                .Where(l => l.IdUsuarioWeb == idUsuarioWeb && l.Activo)
                .Include(l => l.Propiedad)
                .Include(l => l.Estado)
                .OrderByDescending(l => l.CreadoEn)
                .ToListAsync();
        }

        /// <summary>
        /// Verifica si un email ya existe
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email && u.Activo);
        }

        /// <summary>
        /// Verifica si un Google ID ya existe
        /// </summary>
        public async Task<bool> GoogleIdExistsAsync(string googleId)
        {
            return await _dbSet.AnyAsync(u => u.GoogleId == googleId && u.Activo);
        }
    }
}
