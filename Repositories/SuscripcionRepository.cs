using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using inmobiliariaApi.Data;
using inmobiliariaApi.Models;

namespace inmobiliariaApi.Repositories
{
    public class SuscripcionRepository : GenericRepository<Suscripcion>
    {
        private readonly ILogger<SuscripcionRepository> _logger;

        public SuscripcionRepository(ApplicationDbContext context, ILogger<SuscripcionRepository> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task<Suscripcion?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(s => s.Inmobiliaria)
                .Include(s => s.Estado)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Suscripcion>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(s => s.Inmobiliaria)
                .Include(s => s.Estado)
                .OrderByDescending(s => s.CreadoEn)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Suscripcion> Data, int TotalRecords)> GetPagedWithDetailsAsync(
            int page, int pageSize, int? inmobiliariaId = null, int? planId = null, byte? estadoId = null, bool? vigente = null)
        {
            var query = _dbSet
                .Include(s => s.Inmobiliaria)
                .Include(s => s.Estado)
                .AsQueryable();

            if (inmobiliariaId.HasValue)
            {
                query = query.Where(s => s.IdInmobiliaria == inmobiliariaId.Value);
            }

            if (planId.HasValue)
            {
                query = query.Where(s => s.IdPlan == planId.Value);
            }

            if (estadoId.HasValue)
            {
                query = query.Where(s => s.IdEstado == estadoId.Value);
            }

            if (vigente.HasValue)
            {
                var ahora = DateTime.UtcNow;
                if (vigente.Value)
                {
                    query = query.Where(s => s.Inicio <= ahora && s.Fin >= ahora && s.IdEstado == 1);
                }
                else
                {
                    query = query.Where(s => s.Fin < ahora || s.IdEstado != 1);
                }
            }

            var totalRecords = await query.CountAsync();
            var data = await query
                .OrderByDescending(s => s.CreadoEn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalRecords);
        }

        public async Task<Suscripcion?> GetActiveByInmobiliariaAsync(int inmobiliariaId)
        {
            var ahora = DateTime.UtcNow;
            return await _dbSet
                .Include(s => s.Inmobiliaria)
                .Include(s => s.Estado)
                .Where(s => s.IdInmobiliaria == inmobiliariaId && 
                           s.IdEstado == 1 && 
                           s.Inicio <= ahora && 
                           s.Fin >= ahora)
                .OrderByDescending(s => s.Fin)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Suscripcion>> GetByInmobiliariaAsync(int inmobiliariaId)
        {
            return await _dbSet
                .Include(s => s.Inmobiliaria)
                .Include(s => s.Estado)
                .Where(s => s.IdInmobiliaria == inmobiliariaId)
                .OrderByDescending(s => s.CreadoEn)
                .ToListAsync();
        }

        public async Task<IEnumerable<Suscripcion>> GetVencenSoonAsync(int dias = 30)
        {
            var fechaLimite = DateTime.UtcNow.AddDays(dias);
            return await _dbSet
                .Include(s => s.Inmobiliaria)
                .Include(s => s.Estado)
                .Where(s => s.IdEstado == 1 && 
                           s.Fin <= fechaLimite && 
                           s.Fin >= DateTime.UtcNow)
                .OrderBy(s => s.Fin)
                .ToListAsync();
        }

        public async Task<IEnumerable<Suscripcion>> GetVencidasAsync()
        {
            var ahora = DateTime.UtcNow;
            return await _dbSet
                .Include(s => s.Inmobiliaria)
                .Include(s => s.Estado)
                .Where(s => s.IdEstado == 1 && s.Fin < ahora)
                .OrderByDescending(s => s.Fin)
                .ToListAsync();
        }

        public async Task<IEnumerable<Suscripcion>> GetPendientesRenovacionAsync()
        {
            var ahora = DateTime.UtcNow;
            var en30Dias = ahora.AddDays(30);

            return await _dbSet
                .Include(s => s.Inmobiliaria)
                .Include(s => s.Estado)
                .Where(s => s.IdEstado == 1 && 
                           s.RenovacionAutomatica && 
                           s.Fin >= ahora && 
                           s.Fin <= en30Dias)
                .OrderBy(s => s.Fin)
                .ToListAsync();
        }

        public async Task<bool> ValidateInmobiliariaAsync(int inmobiliariaId)
        {
            return await _context.Inmobiliaria
                .AnyAsync(i => i.Id == inmobiliariaId && i.IdEstado == 1);
        }

        public async Task<bool> ValidatePlanAsync(int planId)
        {
            return await _context.Plan
                .AnyAsync(p => p.Id == planId && p.Activo == true);
        }

        public async Task<bool> HasActiveSuscripcionAsync(int inmobiliariaId, int? excludeId = null)
        {
            var ahora = DateTime.UtcNow;
            var query = _dbSet.Where(s => s.IdInmobiliaria == inmobiliariaId && 
                                         s.IdEstado == 1 && 
                                         s.Inicio <= ahora && 
                                         s.Fin >= ahora);

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<int> GetCountActivasByPlanAsync(int planId)
        {
            var ahora = DateTime.UtcNow;
            return await _dbSet
                .CountAsync(s => s.IdPlan == planId && 
                                s.IdEstado == 1 && 
                                s.Inicio <= ahora && 
                                s.Fin >= ahora);
        }
    }
}
