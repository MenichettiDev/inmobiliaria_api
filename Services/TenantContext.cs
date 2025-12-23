using Microsoft.AspNetCore.Http;
using inmobiliariaApi.Models;
using inmobiliariaApi.Data;
using Microsoft.EntityFrameworkCore;

namespace inmobiliariaApi.Services
{
    public interface ITenantContext
    {
        int GetCurrentTenantId();
        Task<Inmobiliaria?> GetCurrentInmobiliariaAsync();
        string GetCurrentSubdomain();
    }

    public class TenantContext : ITenantContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private int? _cachedTenantId;
        private string? _cachedSubdomain;

        public TenantContext(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public string GetCurrentSubdomain()
        {
            if (_cachedSubdomain != null)
                return _cachedSubdomain;

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
                throw new InvalidOperationException("No se puede obtener el contexto HTTP");

            var host = request.Host.Host;

            // Extraer subdominio (ej: demo.inmobiliaria.com -> demo)
            var parts = host.Split('.');
            if (parts.Length >= 3)
            {
                _cachedSubdomain = parts[0];
                return _cachedSubdomain;
            }

            // Para desarrollo local, usar header personalizado
            var subdomainHeader = request.Headers["X-Subdomain"].FirstOrDefault();
            if (!string.IsNullOrEmpty(subdomainHeader))
            {
                _cachedSubdomain = subdomainHeader;
                return _cachedSubdomain;
            }

            throw new InvalidOperationException("No se puede determinar el subdominio de la inmobiliaria");
        }

        public int GetCurrentTenantId()
        {
            if (_cachedTenantId.HasValue)
                return _cachedTenantId.Value;

            var subdomain = GetCurrentSubdomain();

            // En un escenario real, esto debería estar en caché (Redis, Memory Cache, etc.)
            var inmobiliaria = _context.Inmobiliaria
                .FirstOrDefault(i => i.Subdominio == subdomain && i.IdEstado == 1); // solo activas

            if (inmobiliaria == null)
                throw new InvalidOperationException($"Inmobiliaria no encontrada para el subdominio: {subdomain}");

            _cachedTenantId = inmobiliaria.Id;
            return _cachedTenantId.Value;
        }

        public async Task<Inmobiliaria?> GetCurrentInmobiliariaAsync()
        {
            var tenantId = GetCurrentTenantId();
            return await _context.Inmobiliaria
                .Include(i => i.Plan)
                .Include(i => i.Estado)
                .FirstOrDefaultAsync(i => i.Id == tenantId);
        }
    }
}