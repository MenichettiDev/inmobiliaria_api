using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using inmobiliariaApi.Data;
using inmobiliariaApi.Models;

namespace inmobiliariaApi.Repositories
{
    public class BusquedasGuardadasRepository : GenericRepository<BusquedasGuardadas>
    {
        private readonly ILogger<BusquedasGuardadasRepository> _logger;

        public BusquedasGuardadasRepository(ApplicationDbContext context, ILogger<BusquedasGuardadasRepository> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<BusquedasGuardadas>> GetByTenantAsync(int tenantId)
        {
            return await _dbSet
                .Include(b => b.Inmobiliaria)
                .Where(b => b.IdInmobiliaria == tenantId)
                .OrderByDescending(b => b.CreadoEn)
                .ToListAsync();
        }

        public async Task<IEnumerable<BusquedasGuardadas>> GetByEmailAndTenantAsync(string email, int tenantId)
        {
            return await _dbSet
                .Include(b => b.Inmobiliaria)
                .Where(b => b.Email.ToLower() == email.ToLower() && b.IdInmobiliaria == tenantId)
                .OrderByDescending(b => b.CreadoEn)
                .ToListAsync();
        }

        public async Task<BusquedasGuardadas?> GetByIdAndTenantAsync(int id, int tenantId)
        {
            return await _dbSet
                .Include(b => b.Inmobiliaria)
                .FirstOrDefaultAsync(b => b.Id == id && b.IdInmobiliaria == tenantId);
        }

        public async Task<(IEnumerable<BusquedasGuardadas> Data, int TotalRecords)> GetPagedByTenantAsync(
            int page, int pageSize, int tenantId, string? email = null)
        {
            var query = _dbSet
                .Include(b => b.Inmobiliaria)
                .Where(b => b.IdInmobiliaria == tenantId);

            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(b => b.Email.ToLower().Contains(email.ToLower()));
            }

            var totalRecords = await query.CountAsync();
            var data = await query
                .OrderByDescending(b => b.CreadoEn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalRecords);
        }

        public async Task<IEnumerable<BusquedasGuardadas>> GetPendientesEnvioAsync(int tenantId, int horasSinEnvio = 24)
        {
            var fechaLimite = DateTime.UtcNow.AddHours(-horasSinEnvio);

            return await _dbSet
                .Include(b => b.Inmobiliaria)
                .Where(b => b.IdInmobiliaria == tenantId &&
                           (b.UltimoEnvio == null || b.UltimoEnvio <= fechaLimite))
                .OrderBy(b => b.UltimoEnvio ?? DateTime.MinValue)
                .ToListAsync();
        }

        public async Task<int> GetCountByEmailAndTenantAsync(string email, int tenantId)
        {
            return await _dbSet
                .CountAsync(b => b.Email.ToLower() == email.ToLower() && b.IdInmobiliaria == tenantId);
        }

        public async Task<bool> ExistsEmailAndFiltersAsync(string email, string filtrosJson, int tenantId)
        {
            return await _dbSet
                .AnyAsync(b => b.Email.ToLower() == email.ToLower() &&
                              b.FiltrosJson == filtrosJson &&
                              b.IdInmobiliaria == tenantId);
        }

        public async Task ActualizarUltimoEnvioAsync(int id)
        {
            var busqueda = await _dbSet.FindAsync(id);
            if (busqueda != null)
            {
                busqueda.UltimoEnvio = DateTime.UtcNow;
                busqueda.ActualizadoEn = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<BusquedasGuardadas>> GetByEmailGlobalAsync(string email)
        {
            return await _dbSet
                .Include(b => b.Inmobiliaria)
                .Where(b => b.Email.ToLower() == email.ToLower())
                .OrderByDescending(b => b.CreadoEn)
                .ToListAsync();
        }
    }
}
