using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using inmobiliariaApi.Data;
using inmobiliariaApi.Models;
using System.Linq;

namespace inmobiliariaApi.Repositories
{
    public class TransaccionHistorialRepository : GenericRepository<TransaccionHistorial>
    {
        private readonly ILogger<TransaccionHistorialRepository> _logger;

        public TransaccionHistorialRepository(ApplicationDbContext context, ILogger<TransaccionHistorialRepository> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task<TransaccionHistorial?> GetByIdWithDetailsAndTenantAsync(int id, int tenantId)
        {
            return await _dbSet
                .Include(t => t.TipoTransaccion)
                .Include(t => t.Cliente)
                .Include(t => t.Propiedad)
                .Include(t => t.Agente)
                .FirstOrDefaultAsync(t => t.Id == id && t.IdInmobiliaria == tenantId);
        }

        public async Task<(IEnumerable<TransaccionHistorial> Data, int TotalRecords)> GetAllWithDetailsPagedByTenantAsync(
            int page, int pageSize, int tenantId, int? clienteId = null, int? agenteId = null, byte? tipoTransaccion = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            try
            {
                IQueryable<TransaccionHistorial> query = _dbSet.AsQueryable();

                query = query
                    .Include(t => t.TipoTransaccion)
                    .Include(t => t.Cliente)
                    .Include(t => t.Propiedad)
                    .Include(t => t.Agente)
                    // filtrar siempre por tenant
                    .Where(t => t.IdInmobiliaria == tenantId);

                if (clienteId.HasValue)
                    query = query.Where(t => t.IdCliente == clienteId.Value);

                if (agenteId.HasValue)
                    query = query.Where(t => t.IdAgente == agenteId.Value);

                if (tipoTransaccion.HasValue)
                    query = query.Where(t => t.IdTipoTransaccion == tipoTransaccion.Value);

                if (fechaDesde.HasValue)
                    query = query.Where(t => t.FechaOperacion >= fechaDesde.Value);

                if (fechaHasta.HasValue)
                    query = query.Where(t => t.FechaOperacion <= fechaHasta.Value);

                var total = await query.CountAsync();
                var data = await query
                    .OrderByDescending(t => t.FechaOperacion)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (data, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetAllWithDetailsPagedByTenantAsync");
                return (Enumerable.Empty<TransaccionHistorial>(), 0);
            }
        }

        public async Task<List<TransaccionHistorial>> GetTransaccionesComboAsync(int tenantId)
        {
            return await _dbSet
                .Where(t => t.IdInmobiliaria == tenantId)
                .OrderByDescending(t => t.FechaOperacion)
                .Select(t => new TransaccionHistorial
                {
                    Id = t.Id,
                    FechaOperacion = t.FechaOperacion,
                    Precio = t.Precio,
                    Observaciones = t.Observaciones
                })
                .ToListAsync();
        }
    }
}
