using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using inmobiliariaApi.Data;
using inmobiliariaApi.Models;

namespace inmobiliariaApi.Repositories
{
    public class PropiedadRepository : GenericRepository<Propiedad>
    {
        private readonly ILogger<PropiedadRepository> _logger;

        public PropiedadRepository(ApplicationDbContext context, ILogger<PropiedadRepository> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task<Propiedad?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(p => p.AgenteResponsable)
                .Include(p => p.EstadoAdmin)
                .Include(p => p.EstadoOperativo)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Propiedad?> GetByIdWithDetailsAndTenantAsync(int id, int tenantId)
        {
            return await _dbSet
                .Include(p => p.AgenteResponsable)
                .Include(p => p.EstadoAdmin)
                .Include(p => p.EstadoOperativo)
                .FirstOrDefaultAsync(p => p.Id == id && p.IdInmobiliaria == tenantId);
        }

        public async Task<(IEnumerable<Propiedad> Data, int TotalRecords)> GetAllWithDetailsPagedByTenantAsync(
            int page, int pageSize, int tenantId, string? titulo = null, int? agenteId = null,
            int? estadoAdmin = null, int? estadoOperativo = null, decimal? precioMin = null, decimal? precioMax = null)
        {
            var query = _dbSet
                .Include(p => p.AgenteResponsable)
                .Include(p => p.EstadoAdmin)
                .Include(p => p.EstadoOperativo)
                .Where(p => p.IdInmobiliaria == tenantId);

            if (!string.IsNullOrEmpty(titulo))
            {
                query = query.Where(p => p.Titulo.Contains(titulo));
            }

            if (agenteId.HasValue)
            {
                query = query.Where(p => p.IdAgenteResponsable == agenteId.Value);
            }

            if (estadoAdmin.HasValue)
            {
                query = query.Where(p => p.IdEstadoAdmin == estadoAdmin.Value);
            }

            if (estadoOperativo.HasValue)
            {
                query = query.Where(p => p.IdEstadoOperativo == estadoOperativo.Value);
            }

            if (precioMin.HasValue)
            {
                query = query.Where(p => p.Precio >= precioMin.Value);
            }

            if (precioMax.HasValue)
            {
                query = query.Where(p => p.Precio <= precioMax.Value);
            }

            var totalRecords = await query.CountAsync();
            var data = await query
                .OrderByDescending(p => p.CreadoEn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalRecords);
        }

        public async Task<bool> ValidateAgenteInTenantAsync(int agenteId, int tenantId)
        {
            return await _context.Usuario
                .AnyAsync(u => u.Id == agenteId && u.IdInmobiliaria == tenantId && u.IdEstado == 1);
        }

        public async Task<List<Propiedad>> GetPropiedadesComboAsync(int tenantId)
        {
            return await _dbSet
                .Where(p => p.IdInmobiliaria == tenantId && p.IdEstadoAdmin == 1)
                .OrderBy(p => p.Titulo)
                .Select(p => new Propiedad
                {
                    Id = p.Id,
                    Titulo = p.Titulo,
                    Direccion = p.Direccion,
                    Precio = p.Precio
                })
                .ToListAsync();
        }
    }
}
