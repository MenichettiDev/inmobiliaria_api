using Microsoft.EntityFrameworkCore;
using inmobiliariaApi.Data;
using inmobiliariaApi.Models;

namespace inmobiliariaApi.Repositories
{
    public class LocalidadRepository : GenericRepository<Localidad>
    {
        public LocalidadRepository(ApplicationDbContext context) : base(context) { }

        public override async Task<Localidad?> GetByIdAsync(int id) => await GetByIdLongAsync(id);

        public async Task<Localidad?> GetByIdLongAsync(long id)
        {
            return await _dbSet.FindAsync(id);
        }

        public override async Task DeleteAsync(int id) => await DeleteLongAsync(id);

        public async Task DeleteLongAsync(long id)
        {
            var entity = await GetByIdLongAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Localidad>> GetByProvinciaAsync(long idProvincia)
        {
            return await _dbSet
                .Include(l => l.Provincia)
                .Where(l => l.IdProvincia == idProvincia)
                .OrderBy(l => l.Nombre)
                .ToListAsync();
        }

        public async Task<Localidad?> GetByIdWithProvinciaAsync(long id)
        {
            return await _dbSet.Include(l => l.Provincia).FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<Localidad>> GetAllWithProvinciaAsync()
        {
            return await _dbSet
                .Include(l => l.Provincia)
                .OrderBy(l => l.Provincia!.Nombre)
                .ThenBy(l => l.Nombre)
                .ToListAsync();
        }

        public async Task<bool> ExisteEnProvinciaAsync(string nombre, long idProvincia, long? excludeId = null)
        {
            var q = _dbSet.Where(l => l.Nombre.ToLower() == nombre.ToLower() && l.IdProvincia == idProvincia);
            if (excludeId.HasValue) q = q.Where(l => l.Id != excludeId.Value);
            return await q.AnyAsync();
        }
    }
}
