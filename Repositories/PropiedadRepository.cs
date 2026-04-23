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
                .Include(p => p.EstadoOperativo)
                .Include(p => p.Localidad)
                    .ThenInclude(l => l!.Provincia)
                .Include(p => p.Imagenes)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Propiedad?> GetByIdWithDetailsAndTenantAsync(int id, int tenantId)
        {
            return await _dbSet
                .Include(p => p.AgenteResponsable)
                .Include(p => p.EstadoOperativo)
                .Include(p => p.Localidad)
                    .ThenInclude(l => l!.Provincia)
                .Include(p => p.Imagenes)
                .FirstOrDefaultAsync(p => p.Id == id && p.IdInmobiliaria == tenantId);
        }

        public async Task<(IEnumerable<Propiedad> Data, int TotalRecords)> GetAllWithDetailsPagedByTenantAsync(
            int page, int pageSize, int tenantId, string? titulo = null, int? agenteId = null,
            int? estadoAdmin = null, int? estadoOperativo = null, decimal? precioMin = null, decimal? precioMax = null)
        {
            var query = _dbSet
                .Include(p => p.AgenteResponsable)
                .Include(p => p.EstadoOperativo)
                .Include(p => p.Localidad)
                    .ThenInclude(l => l!.Provincia)
                .Include(p => p.Imagenes)
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
                // estadoAdmin: 1 = activo, cualquier otro = inactivo
                query = query.Where(p => p.Activo == (estadoAdmin.Value == 1));
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
                .Where(p => p.IdInmobiliaria == tenantId && p.Activo)
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

        // ===== MÉTODOS PÚBLICOS (SIN AUTENTICACIÓN) =====

        public async Task<(IEnumerable<Propiedad> Data, int TotalRecords)> GetPublicadasCrossTenantPagedAsync(
            int page, int pageSize, string? titulo = null, decimal? precioMin = null, decimal? precioMax = null,
            int? idInmobiliaria = null, long? idProvincia = null)
        {
            var query = _dbSet
                .Include(p => p.Inmobiliaria)
                .Include(p => p.Localidad)
                    .ThenInclude(l => l!.Provincia)
                .Include(p => p.Imagenes)
                .Where(p => p.EsPublicada && p.Activo);

            if (!string.IsNullOrEmpty(titulo))
                query = query.Where(p => p.Titulo.Contains(titulo) || p.Direccion.Contains(titulo));

            if (precioMin.HasValue)
                query = query.Where(p => p.Precio >= precioMin.Value);

            if (precioMax.HasValue)
                query = query.Where(p => p.Precio <= precioMax.Value);

            if (idInmobiliaria.HasValue)
                query = query.Where(p => p.IdInmobiliaria == idInmobiliaria.Value);

            if (idProvincia.HasValue)
                query = query.Where(p => p.Localidad != null && p.Localidad.IdProvincia == idProvincia.Value);

            var totalRecords = await query.CountAsync();
            var data = await query
                .OrderByDescending(p => p.PublicadaEn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalRecords);
        }

        public async Task<(IEnumerable<Propiedad> Data, int TotalRecords)> GetPublicadasBySubdominioPagedAsync(
            string subdominio, int page, int pageSize, string? titulo = null, decimal? precioMin = null, decimal? precioMax = null,
            int? idInmobiliaria = null, long? idProvincia = null)
        {
            var query = _dbSet
                .Include(p => p.Inmobiliaria)
                .Include(p => p.Localidad)
                    .ThenInclude(l => l!.Provincia)
                .Include(p => p.Imagenes)
                .Where(p => p.EsPublicada && p.Activo && p.Inmobiliaria!.Subdominio == subdominio);

            if (!string.IsNullOrEmpty(titulo))
                query = query.Where(p => p.Titulo.Contains(titulo) || p.Direccion.Contains(titulo));

            if (precioMin.HasValue)
                query = query.Where(p => p.Precio >= precioMin.Value);

            if (precioMax.HasValue)
                query = query.Where(p => p.Precio <= precioMax.Value);

            if (idInmobiliaria.HasValue)
                query = query.Where(p => p.IdInmobiliaria == idInmobiliaria.Value);

            if (idProvincia.HasValue)
                query = query.Where(p => p.Localidad != null && p.Localidad.IdProvincia == idProvincia.Value);

            var totalRecords = await query.CountAsync();
            var data = await query
                .OrderByDescending(p => p.PublicadaEn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalRecords);
        }

        public async Task<(List<(int Id, string Nombre, int Count)> Inmobiliarias, List<(long Id, string Nombre, int Count)> Provincias)>
            GetPublicFiltrosOpcionesAsync()
        {
            var baseIds = await _dbSet
                .Where(p => p.EsPublicada && p.Activo)
                .Select(p => new { p.IdInmobiliaria, p.IdLocalidad })
                .ToListAsync();

            // Inmobiliarias
            var inmoCounts = baseIds
                .GroupBy(x => x.IdInmobiliaria)
                .ToDictionary(g => g.Key, g => g.Count());

            var inmoNombres = await _context.Inmobiliaria
                .Where(i => inmoCounts.Keys.Contains(i.Id))
                .Select(i => new { i.Id, i.Nombre })
                .ToListAsync();

            var inmobiliarias = inmoNombres
                .Select(i => (Id: i.Id, Nombre: i.Nombre, Count: inmoCounts.GetValueOrDefault(i.Id)))
                .OrderBy(x => x.Nombre)
                .ToList();

            // Provincias — via localidades
            var localidadIds = baseIds
                .Where(x => x.IdLocalidad.HasValue)
                .Select(x => x.IdLocalidad!.Value)
                .Distinct()
                .ToList();

            var localidadProvMap = await _context.Localidades
                .Where(l => localidadIds.Contains(l.Id))
                .Select(l => new { l.Id, l.IdProvincia })
                .ToListAsync();

            var provCounts = baseIds
                .Where(x => x.IdLocalidad.HasValue)
                .GroupJoin(
                    localidadProvMap,
                    p => p.IdLocalidad!.Value,
                    l => l.Id,
                    (p, ls) => ls.Select(l => l.IdProvincia))
                .SelectMany(x => x)
                .GroupBy(idProv => idProv)
                .ToDictionary(g => g.Key, g => g.Count());

            var provNombres = await _context.Provincias
                .Where(p => provCounts.Keys.Contains(p.Id))
                .Select(p => new { p.Id, p.Nombre })
                .ToListAsync();

            var provincias = provNombres
                .Select(p => (Id: p.Id, Nombre: p.Nombre, Count: provCounts.GetValueOrDefault(p.Id)))
                .OrderBy(x => x.Nombre)
                .ToList();

            return (inmobiliarias, provincias);
        }

        public async Task<Propiedad?> GetPublicadaByIdAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Inmobiliaria)
                .Include(p => p.Localidad)
                    .ThenInclude(l => l!.Provincia)
                .Include(p => p.Imagenes)
                .FirstOrDefaultAsync(p => p.Id == id && p.EsPublicada && p.Activo);
        }
    }
}
