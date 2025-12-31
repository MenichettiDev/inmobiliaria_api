using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using inmobiliariaApi.Data;
using inmobiliariaApi.Models;

namespace inmobiliariaApi.Repositories
{
    public class LeadEstadoHistorialRepository : GenericRepository<LeadEstadoHistorial>
    {
        private readonly ILogger<LeadEstadoHistorialRepository> _logger;

        public LeadEstadoHistorialRepository(ApplicationDbContext context, ILogger<LeadEstadoHistorialRepository> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<LeadEstadoHistorial>> GetHistorialByLeadIdAsync(int leadId)
        {
            return await _dbSet
                .Include(h => h.EstadoAnterior)
                .Include(h => h.EstadoNuevo)
                .Include(h => h.Usuario)
                .Include(h => h.Lead)
                .Where(h => h.IdLead == leadId)
                .OrderByDescending(h => h.CreadoEn)
                .ToListAsync();
        }

        public async Task<IEnumerable<LeadEstadoHistorial>> GetHistorialByLeadAndTenantAsync(int leadId, int tenantId)
        {
            return await _dbSet
                .Include(h => h.EstadoAnterior)
                .Include(h => h.EstadoNuevo)
                .Include(h => h.Usuario)
                .Include(h => h.Lead)
                .Where(h => h.IdLead == leadId && h.Lead!.IdInmobiliaria == tenantId)
                .OrderByDescending(h => h.CreadoEn)
                .ToListAsync();
        }

        public async Task<(IEnumerable<LeadEstadoHistorial> Data, int TotalRecords)> GetHistorialPagedByTenantAsync(
            int page, int pageSize, int tenantId, int? leadId = null, int? usuarioId = null, int? estadoId = null)
        {
            var query = _dbSet
                .Include(h => h.EstadoAnterior)
                .Include(h => h.EstadoNuevo)
                .Include(h => h.Usuario)
                .Include(h => h.Lead)
                .Where(h => h.Lead!.IdInmobiliaria == tenantId);

            if (leadId.HasValue)
            {
                query = query.Where(h => h.IdLead == leadId.Value);
            }

            if (usuarioId.HasValue)
            {
                query = query.Where(h => h.IdUsuario == usuarioId.Value);
            }

            if (estadoId.HasValue)
            {
                query = query.Where(h => h.IdEstadoNuevo == estadoId.Value || h.IdEstadoAnterior == estadoId.Value);
            }

            var totalRecords = await query.CountAsync();
            var data = await query
                .OrderByDescending(h => h.CreadoEn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalRecords);
        }

        public async Task<LeadEstadoHistorial?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(h => h.EstadoAnterior)
                .Include(h => h.EstadoNuevo)
                .Include(h => h.Usuario)
                .Include(h => h.Lead)
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task<LeadEstadoHistorial?> GetByIdWithDetailsAndTenantAsync(int id, int tenantId)
        {
            return await _dbSet
                .Include(h => h.EstadoAnterior)
                .Include(h => h.EstadoNuevo)
                .Include(h => h.Usuario)
                .Include(h => h.Lead)
                .FirstOrDefaultAsync(h => h.Id == id && h.Lead!.IdInmobiliaria == tenantId);
        }

        public async Task<bool> ValidateLeadInTenantAsync(int leadId, int tenantId)
        {
            return await _context.Lead
                .AnyAsync(l => l.Id == leadId && l.IdInmobiliaria == tenantId);
        }
    }
}
