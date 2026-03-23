using Microsoft.EntityFrameworkCore;
using inmobiliariaApi.Data;
using inmobiliariaApi.Models;

namespace inmobiliariaApi.Repositories
{
    public class ProvinciaRepository : GenericRepository<Provincia>
    {
        public ProvinciaRepository(ApplicationDbContext context) : base(context) { }

        public override async Task<Provincia?> GetByIdAsync(int id) => await GetByIdLongAsync(id);

        public async Task<Provincia?> GetByIdLongAsync(long id)
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

        public async Task<IEnumerable<Provincia>> GetAllActivasAsync()
        {
            return await _dbSet
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<bool> ExisteNombreAsync(string nombre, long? excludeId = null)
        {
            var q = _dbSet.Where(p => p.Nombre.ToLower() == nombre.ToLower());
            if (excludeId.HasValue) q = q.Where(p => p.Id != excludeId.Value);
            return await q.AnyAsync();
        }
    }
}
