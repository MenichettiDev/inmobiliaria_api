using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using inmobiliariaApi.Data;
using inmobiliariaApi.Models;

namespace inmobiliariaApi.Repositories
{
    public class ImagenPropiedadRepository : GenericRepository<ImagenPropiedad>
    {
        private readonly ILogger<ImagenPropiedadRepository> _logger;

        public ImagenPropiedadRepository(ApplicationDbContext context, ILogger<ImagenPropiedadRepository> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<ImagenPropiedad>> GetByPropiedadIdAsync(int propiedadId)
        {
            return await _dbSet
                .Include(i => i.Propiedad)
                .Where(i => i.IdPropiedad == propiedadId)
                .OrderBy(i => i.Orden)
                .ThenBy(i => i.CreadoEn)
                .ToListAsync();
        }

        public async Task<IEnumerable<ImagenPropiedad>> GetByPropiedadAndTenantAsync(int propiedadId, int tenantId)
        {
            return await _dbSet
                .Include(i => i.Propiedad)
                .Where(i => i.IdPropiedad == propiedadId && i.Propiedad!.IdInmobiliaria == tenantId)
                .OrderBy(i => i.Orden)
                .ThenBy(i => i.CreadoEn)
                .ToListAsync();
        }

        public async Task<ImagenPropiedad?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(i => i.Propiedad)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<ImagenPropiedad?> GetByIdAndTenantAsync(int id, int tenantId)
        {
            return await _dbSet
                .Include(i => i.Propiedad)
                .FirstOrDefaultAsync(i => i.Id == id && i.Propiedad!.IdInmobiliaria == tenantId);
        }

        public async Task<bool> ValidatePropiedadInTenantAsync(int propiedadId, int tenantId)
        {
            return await _context.Propiedad
                .AnyAsync(p => p.Id == propiedadId && p.IdInmobiliaria == tenantId);
        }

        public async Task<Propiedad?> GetPropiedadByIdAsync(int propiedadId)
        {
            return await _context.Propiedad
                .FirstOrDefaultAsync(p => p.Id == propiedadId);
        }

        public async Task<Propiedad?> GetPropiedadByIdAndTenantAsync(int propiedadId, int tenantId)
        {
            return await _context.Propiedad
                .FirstOrDefaultAsync(p => p.Id == propiedadId && p.IdInmobiliaria == tenantId);
        }

        public async Task<int> GetCountByPropiedadAsync(int propiedadId)
        {
            return await _dbSet.CountAsync(i => i.IdPropiedad == propiedadId);
        }

        public async Task<int> GetMaxOrdenByPropiedadAsync(int propiedadId)
        {
            var maxOrden = await _dbSet
                .Where(i => i.IdPropiedad == propiedadId)
                .MaxAsync(i => (int?)i.Orden);

            return maxOrden ?? 0;
        }

        public async Task ReordenarImagenesAsync(int propiedadId, List<int> nuevosOrdenes)
        {
            var imagenes = await _dbSet
                .Where(i => i.IdPropiedad == propiedadId)
                .ToListAsync();

            for (int i = 0; i < imagenes.Count && i < nuevosOrdenes.Count; i++)
            {
                imagenes[i].Orden = nuevosOrdenes[i];
            }

            await _context.SaveChangesAsync();
        }

        public async Task<(IEnumerable<ImagenPropiedad> Data, int TotalRecords)> GetPagedByTenantAsync(
            int page, int pageSize, int tenantId, int? propiedadId = null)
        {
            var query = _dbSet
                .Include(i => i.Propiedad)
                .Where(i => i.Propiedad!.IdInmobiliaria == tenantId);

            if (propiedadId.HasValue)
            {
                query = query.Where(i => i.IdPropiedad == propiedadId.Value);
            }

            var totalRecords = await query.CountAsync();
            var data = await query
                .OrderByDescending(i => i.CreadoEn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalRecords);
        }
    }
}
