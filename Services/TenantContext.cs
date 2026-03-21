using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
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
        void InvalidateTenantCache(string subdomain);
        void InvalidateTenantCacheById(int tenantId);
    }

    public class TenantContext : ITenantContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TenantContext> _logger;

        // TTL: 10 min sliding, 30 min absolute — datos de tenant cambian raramente
        private static readonly MemoryCacheEntryOptions TenantIdCacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

        private static readonly MemoryCacheEntryOptions TenantFullCacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

        // Cache corto para subdominos inválidos (evita hammering a DB con subdominios inexistentes)
        private static readonly MemoryCacheEntryOptions NegativeCacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(30));

        // Cache a nivel de request (evita múltiples lookups por request)
        private int? _requestTenantId;
        private string? _requestSubdomain;

        public TenantContext(
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            IMemoryCache cache,
            ILogger<TenantContext> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public string GetCurrentSubdomain()
        {
            if (_requestSubdomain != null)
                return _requestSubdomain;

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
                throw new InvalidOperationException("No se puede obtener el contexto HTTP");

            var host = request.Host.Host;
            var parts = host.Split('.');

            if (parts.Length >= 3)
            {
                _requestSubdomain = parts[0];
                return _requestSubdomain;
            }

            // Header personalizado para desarrollo local
            var subdomainHeader = request.Headers["X-Subdomain"].FirstOrDefault();
            if (!string.IsNullOrEmpty(subdomainHeader))
            {
                _requestSubdomain = subdomainHeader;
                return _requestSubdomain;
            }

            throw new InvalidOperationException("No se puede determinar el subdominio de la inmobiliaria");
        }

        public int GetCurrentTenantId()
        {
            // 1. Cache de request (más rápido)
            if (_requestTenantId.HasValue)
                return _requestTenantId.Value;

            var subdomain = GetCurrentSubdomain();
            var cacheKey = $"tenant:id:{subdomain}";

            // 2. Verificar cache negativo (subdominio inválido conocido)
            if (_cache.TryGetValue($"tenant:invalid:{subdomain}", out _))
                throw new InvalidOperationException($"Inmobiliaria no encontrada para el subdominio: {subdomain}");

            // 3. IMemoryCache
            if (_cache.TryGetValue(cacheKey, out int cachedId))
            {
                _requestTenantId = cachedId;
                return cachedId;
            }

            // 4. DB query (solo si no está en cache)
            _logger.LogDebug("Cache miss — consultando DB para subdominio: {Subdomain}", subdomain);

            var inmobiliaria = _context.Inmobiliaria
                .AsNoTracking()
                .FirstOrDefault(i => i.Subdominio == subdomain && i.IdEstado == 1);

            if (inmobiliaria == null)
            {
                // Cachear resultado negativo
                _cache.Set($"tenant:invalid:{subdomain}", true, NegativeCacheOptions);
                throw new InvalidOperationException($"Inmobiliaria no encontrada para el subdominio: {subdomain}");
            }

            _cache.Set(cacheKey, inmobiliaria.Id, TenantIdCacheOptions);
            _requestTenantId = inmobiliaria.Id;

            return inmobiliaria.Id;
        }

        public async Task<Inmobiliaria?> GetCurrentInmobiliariaAsync()
        {
            var tenantId = GetCurrentTenantId();
            var cacheKey = $"tenant:full:{tenantId}";

            if (_cache.TryGetValue(cacheKey, out Inmobiliaria? cached))
                return cached;

            _logger.LogDebug("Cache miss — consultando Inmobiliaria completa para tenant: {TenantId}", tenantId);

            var inmobiliaria = await _context.Inmobiliaria
                .AsNoTracking()
                .Include(i => i.Plan)
                .Include(i => i.Estado)
                .FirstOrDefaultAsync(i => i.Id == tenantId);

            if (inmobiliaria != null)
                _cache.Set(cacheKey, inmobiliaria, TenantFullCacheOptions);

            return inmobiliaria;
        }

        // Invalidar cache por subdominio (usar cuando se actualiza una Inmobiliaria)
        public void InvalidateTenantCache(string subdomain)
        {
            _cache.Remove($"tenant:id:{subdomain}");
            _cache.Remove($"tenant:invalid:{subdomain}");
            _logger.LogInformation("Cache de tenant invalidado para subdominio: {Subdomain}", subdomain);
        }

        // Invalidar cache por ID (requiere conocer el subdominio; útil desde controladores)
        public void InvalidateTenantCacheById(int tenantId)
        {
            _cache.Remove($"tenant:full:{tenantId}");
            _logger.LogInformation("Cache de Inmobiliaria completa invalidado para tenant: {TenantId}", tenantId);
        }
    }
}
