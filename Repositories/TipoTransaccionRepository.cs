using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using inmobiliariaApi.Data;
using inmobiliariaApi.Models;

namespace inmobiliariaApi.Repositories
{
    public class TipoTransaccionRepository : GenericRepository<TipoTransaccion>
    {
        private readonly ILogger<TipoTransaccionRepository> _logger;

        public TipoTransaccionRepository(ApplicationDbContext context, ILogger<TipoTransaccionRepository> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task<TipoTransaccion?> GetByIdAndTenantAsync(int id, int tenantId)
        {
            return await _dbSet.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<TipoTransaccion>> GetAllByTenantAsync(int tenantId)
        {
            return await _dbSet
                .OrderBy(t => t.Descripcion)
                .ToListAsync();
        }

        public async Task<List<TipoTransaccion>> GetComboAsync(int tenantId)
        {
            return await _dbSet
                .OrderBy(t => t.Descripcion)
                .Select(t => new TipoTransaccion { Id = t.Id, Descripcion = t.Descripcion })
                .ToListAsync();
        }
    }
}
