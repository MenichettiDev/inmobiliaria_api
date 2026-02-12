using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using inmobiliariaApi.Data;
using inmobiliariaApi.Models;

namespace inmobiliariaApi.Repositories
{
    // ...new file...
    public class ClienteRepository : GenericRepository<Cliente>
    {
        private readonly ILogger<ClienteRepository> _logger;

        public ClienteRepository(ApplicationDbContext context, ILogger<ClienteRepository> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task<Cliente?> GetByIdAndTenantAsync(int id, int tenantId)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Id == id && c.IdInmobiliaria == tenantId);
        }

        public async Task<(IEnumerable<Cliente> Data, int TotalRecords)> GetAllPagedByTenantAsync(int page, int pageSize, int tenantId, string? nombre = null, bool? activo = null)
        {
            var query = _dbSet.Where(c => c.IdInmobiliaria == tenantId);

            if (!string.IsNullOrWhiteSpace(nombre))
                query = query.Where(c => EF.Functions.Like(c.NombreCompleto, $"%{nombre}%"));

            if (activo.HasValue)
                query = query.Where(c => c.Activo == activo.Value);

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(c => c.CreadoEn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }
    }
}
