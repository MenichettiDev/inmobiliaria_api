using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using inmobiliariaApi.Data;
using inmobiliariaApi.Models;

namespace inmobiliariaApi.Repositories
{
    public class UsoMensualRepository : GenericRepository<UsoMensual>
    {
        private readonly ILogger<UsoMensualRepository> _logger;

        public UsoMensualRepository(ApplicationDbContext context, ILogger<UsoMensualRepository> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task<UsoMensual?> GetByMesAndTenantAsync(string mes, int tenantId)
        {
            return await _dbSet
                .Include(u => u.Inmobiliaria)
                .FirstOrDefaultAsync(u => u.Mes == mes && u.IdInmobiliaria == tenantId);
        }

        public async Task<IEnumerable<UsoMensual>> GetByTenantAsync(int tenantId)
        {
            return await _dbSet
                .Include(u => u.Inmobiliaria)
                .Where(u => u.IdInmobiliaria == tenantId)
                .OrderByDescending(u => u.Mes)
                .ToListAsync();
        }

        public async Task<(IEnumerable<UsoMensual> Data, int TotalRecords)> GetPagedByTenantAsync(
            int page, int pageSize, int tenantId, string? mesDesde = null, string? mesHasta = null)
        {
            var query = _dbSet
                .Include(u => u.Inmobiliaria)
                .Where(u => u.IdInmobiliaria == tenantId);

            if (!string.IsNullOrEmpty(mesDesde))
            {
                query = query.Where(u => u.Mes.CompareTo(mesDesde) >= 0);
            }

            if (!string.IsNullOrEmpty(mesHasta))
            {
                query = query.Where(u => u.Mes.CompareTo(mesHasta) <= 0);
            }

            var totalRecords = await query.CountAsync();
            var data = await query
                .OrderByDescending(u => u.Mes)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalRecords);
        }

        public async Task<UsoMensual?> GetByIdAndTenantAsync(int id, int tenantId)
        {
            return await _dbSet
                .Include(u => u.Inmobiliaria)
                .FirstOrDefaultAsync(u => u.Id == id && u.IdInmobiliaria == tenantId);
        }

        public async Task<IEnumerable<UsoMensual>> GetUltimosSeisMesesAsync(int tenantId)
        {
            var fechaLimite = DateTime.Now.AddMonths(-6).ToString("yyyy-MM");

            return await _dbSet
                .Include(u => u.Inmobiliaria)
                .Where(u => u.IdInmobiliaria == tenantId && u.Mes.CompareTo(fechaLimite) >= 0)
                .OrderByDescending(u => u.Mes)
                .Take(6)
                .ToListAsync();
        }

        public async Task<int> GetTotalLeadsEnPeriodoAsync(int tenantId, string mesDesde, string mesHasta)
        {
            var query = _dbSet.Where(u => u.IdInmobiliaria == tenantId);

            if (!string.IsNullOrEmpty(mesDesde))
            {
                query = query.Where(u => u.Mes.CompareTo(mesDesde) >= 0);
            }

            if (!string.IsNullOrEmpty(mesHasta))
            {
                query = query.Where(u => u.Mes.CompareTo(mesHasta) <= 0);
            }

            return await query.SumAsync(u => u.LeadsGenerados);
        }
    }
}
