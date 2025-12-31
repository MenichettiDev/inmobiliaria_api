using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using inmobiliariaApi.Data;
using inmobiliariaApi.Models;

namespace inmobiliariaApi.Repositories
{
    public class InmobiliariaRepository : GenericRepository<Inmobiliaria>
    {
        private readonly ILogger<InmobiliariaRepository> _logger;

        public InmobiliariaRepository(ApplicationDbContext context, ILogger<InmobiliariaRepository> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task<Inmobiliaria?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(i => i.Plan)
                .Include(i => i.Estado)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<Inmobiliaria>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(i => i.Plan)
                .Include(i => i.Estado)
                .OrderByDescending(i => i.CreadoEn)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Inmobiliaria> Data, int TotalRecords)> GetPagedWithDetailsAsync(
            int page, int pageSize, string? nombre = null, int? planId = null, int? estadoId = null)
        {
            var query = _dbSet
                .Include(i => i.Plan)
                .Include(i => i.Estado)
                .AsQueryable();

            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(i => i.Nombre.Contains(nombre) || i.Subdominio.Contains(nombre));
            }

            if (planId.HasValue)
            {
                query = query.Where(i => i.IdPlan == planId.Value);
            }

            if (estadoId.HasValue)
            {
                query = query.Where(i => i.IdEstado == estadoId.Value);
            }

            var totalRecords = await query.CountAsync();
            var data = await query
                .OrderByDescending(i => i.CreadoEn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalRecords);
        }

        public async Task<Inmobiliaria?> GetBySubdominioAsync(string subdominio)
        {
            return await _dbSet
                .Include(i => i.Plan)
                .Include(i => i.Estado)
                .FirstOrDefaultAsync(i => i.Subdominio.ToLower() == subdominio.ToLower());
        }

        public async Task<Inmobiliaria?> GetByDominioPersonalizadoAsync(string dominio)
        {
            return await _dbSet
                .Include(i => i.Plan)
                .Include(i => i.Estado)
                .FirstOrDefaultAsync(i => i.DominioPersonalizado != null &&
                                          i.DominioPersonalizado.ToLower() == dominio.ToLower());
        }

        public async Task<bool> ExisteSubdominioAsync(string subdominio, int? excludeId = null)
        {
            var query = _dbSet.Where(i => i.Subdominio.ToLower() == subdominio.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(i => i.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> ExisteDominioPersonalizadoAsync(string dominio, int? excludeId = null)
        {
            var query = _dbSet.Where(i => i.DominioPersonalizado != null &&
                                          i.DominioPersonalizado.ToLower() == dominio.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(i => i.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Inmobiliaria>> GetActivasAsync()
        {
            return await _dbSet
                .Include(i => i.Plan)
                .Include(i => i.Estado)
                .Where(i => i.IdEstado == 1) // Solo activas
                .OrderBy(i => i.Nombre)
                .ToListAsync();
        }

        public async Task<InmobiliariaStatsDto> GetStatsAsync(int inmobiliariaId)
        {
            var totalUsuarios = await _context.Usuario
                .CountAsync(u => u.IdInmobiliaria == inmobiliariaId);

            var totalPropiedades = await _context.Propiedad
                .CountAsync(p => p.IdInmobiliaria == inmobiliariaId);

            var totalLeads = await _context.Lead
                .CountAsync(l => l.IdInmobiliaria == inmobiliariaId);

            return new InmobiliariaStatsDto
            {
                TotalUsuarios = totalUsuarios,
                TotalPropiedades = totalPropiedades,
                TotalLeads = totalLeads
            };
        }

        public async Task<bool> ValidatePlanAsync(int planId)
        {
            return await _context.Plan.AnyAsync(p => p.Id == planId && p.Activo == true);
        }
    }

    public class InmobiliariaStatsDto
    {
        public int TotalUsuarios { get; set; }
        public int TotalPropiedades { get; set; }
        public int TotalLeads { get; set; }
    }
}
