using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using inmobiliariaApi.Data;
using inmobiliariaApi.Models;
using inmobiliariaApi.Services;
using inmobiliariaApi.Exceptions;
using inmobiliariaApi.DTOs.Lead;
using System.Linq.Expressions;

namespace inmobiliariaApi.Repositories
{
    public class LeadRepository : GenericRepository<Lead>
    {
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<LeadRepository> _logger;

        public LeadRepository(ApplicationDbContext context, ITenantContext tenantContext, ILogger<LeadRepository> logger)
            : base(context)
        {
            _tenantContext = tenantContext;
            _logger = logger;
        }

        // NEW: helper seguro para obtener tenant y evitar excepciones en local
        private int TryGetTenantIdOrDefault(int fallback = 0)
        {
            try
            {
                return _tenantContext.GetCurrentTenantId();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "No se pudo determinar el tenant desde TenantContext; usando fallback {Fallback}", fallback);
                return fallback;
            }
        }

        // Sobrescribir métodos base para agregar filtro de tenant
        public override async Task<IEnumerable<Lead>> GetAllAsync()
        {
            var tenantId = TryGetTenantIdOrDefault();
            return await _dbSet
                .Where(l => l.IdInmobiliaria == tenantId)
                .Include(l => l.Propiedad)
                .Include(l => l.Fuente)
                .Include(l => l.Estado)
                .Include(l => l.UsuarioAsignado)
                .Include(l => l.Cliente)
                .ToListAsync();
        }

        public override async Task<Lead?> GetByIdAsync(int id)
        {
            var tenantId = TryGetTenantIdOrDefault();
            return await _dbSet
                .Where(l => l.IdInmobiliaria == tenantId && l.Id == id)
                .Include(l => l.Propiedad)
                .Include(l => l.Fuente)
                .Include(l => l.Estado)
                .Include(l => l.UsuarioAsignado)
                .Include(l => l.Cliente)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Lead>> GetByInmobiliariaAsync()
        {
            return await GetAllAsync(); // Ya incluye el filtro de tenant
        }

        public async Task<Lead?> UpdateEstadoAsync(int leadId, int nuevoEstadoId)
        {
            var lead = await GetByIdAsync(leadId);
            if (lead == null)
                return null;

            lead.IdEstado = nuevoEstadoId;
            lead.ActualizadoEn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return lead;
        }

        public async Task<(IEnumerable<Lead> Items, int TotalCount)> GetLeadsConFiltrosAsync(LeadFiltrosDto filtros)
        {
            var tenantId = TryGetTenantIdOrDefault();

            var query = _dbSet
                .Where(l => l.IdInmobiliaria == tenantId)
                .Include(l => l.Propiedad)
                .Include(l => l.Fuente)
                .Include(l => l.Estado)
                .Include(l => l.UsuarioAsignado)
                .Include(l => l.Cliente)
                .AsQueryable();

            // Apply Activo filter if specified
            if (filtros.Activo.HasValue)
            {
                query = query.Where(l => l.Activo == filtros.Activo.Value);
            }

            // Apply other filters
            if (!string.IsNullOrWhiteSpace(filtros.NombreCompleto))
            {
                query = query.Where(l => l.NombreCompleto.Contains(filtros.NombreCompleto));
            }

            if (!string.IsNullOrWhiteSpace(filtros.Email))
            {
                query = query.Where(l => l.Email != null && l.Email.Contains(filtros.Email));
            }

            if (!string.IsNullOrWhiteSpace(filtros.Telefono))
            {
                query = query.Where(l => l.Telefono != null && l.Telefono.Contains(filtros.Telefono));
            }

            if (filtros.IdEstado.HasValue)
            {
                query = query.Where(l => l.IdEstado == filtros.IdEstado.Value);
            }

            if (filtros.IdFuente.HasValue)
            {
                query = query.Where(l => l.IdFuente == filtros.IdFuente.Value);
            }

            if (filtros.IdUsuarioAsignado.HasValue)
            {
                query = query.Where(l => l.IdUsuarioAsignado == filtros.IdUsuarioAsignado.Value);
            }

            if (filtros.IdPropiedad.HasValue)
            {
                query = query.Where(l => l.IdPropiedad == filtros.IdPropiedad.Value);
            }

            if (filtros.IdCliente.HasValue)
            {
                query = query.Where(l => l.IdCliente == filtros.IdCliente.Value);
            }

            if (filtros.FechaDesde.HasValue)
            {
                query = query.Where(l => l.CreadoEn >= filtros.FechaDesde.Value);
            }

            if (filtros.FechaHasta.HasValue)
            {
                query = query.Where(l => l.CreadoEn <= filtros.FechaHasta.Value.AddDays(1));
            }

            var totalCount = await query.CountAsync();

            // Apply ordering - avoid referencing Estado.Nombre to prevent SQL errors
            if (!string.IsNullOrWhiteSpace(filtros.OrderBy))
            {
                switch (filtros.OrderBy.ToLower())
                {
                    case "creado_en":
                    case "fecha":
                        query = filtros.OrderDescending
                            ? query.OrderByDescending(l => l.CreadoEn)
                            : query.OrderBy(l => l.CreadoEn);
                        break;
                    case "nombre":
                    case "nombrecompleto":
                        query = filtros.OrderDescending
                            ? query.OrderByDescending(l => l.NombreCompleto)
                            : query.OrderBy(l => l.NombreCompleto);
                        break;
                    case "estado":
                        // Use IdEstado instead of Estado.Nombre to avoid SQL errors
                        query = filtros.OrderDescending
                            ? query.OrderByDescending(l => l.IdEstado)
                            : query.OrderBy(l => l.IdEstado);
                        break;
                    default:
                        query = filtros.OrderDescending
                            ? query.OrderByDescending(l => l.CreadoEn)
                            : query.OrderBy(l => l.CreadoEn);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(l => l.CreadoEn);
            }

            var items = await query
                .Skip((filtros.Page - 1) * filtros.PageSize)
                .Take(filtros.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> GetLeadsCountByMonthAsync(int year, int month)
        {
            var tenantId = TryGetTenantIdOrDefault();
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            return await _dbSet
                .Where(l => l.IdInmobiliaria == tenantId &&
                           l.CreadoEn >= startDate &&
                           l.CreadoEn < endDate)
                .CountAsync();
        }

        public async Task<bool> ValidatePropertiesBelongsToTenantAsync(int? propiedadId)
        {
            if (!propiedadId.HasValue)
                return true; // Válido si no se especifica propiedad

            var tenantId = TryGetTenantIdOrDefault();
            _logger.LogDebug("ValidatePropertiesBelongsToTenantAsync checking propiedad {PropiedadId} for tenant {TenantId}", propiedadId.Value, tenantId);
            var exists = await _context.Propiedad
                .AnyAsync(p => p.Id == propiedadId.Value && p.IdInmobiliaria == tenantId);
            _logger.LogDebug("ValidatePropertiesBelongsToTenantAsync result: {Exists}", exists);
            return exists;
        }

        // Método para validar que el usuario asignado pertenece al tenant
        public async Task<bool> ValidateUsuarioAsignadoBelongsToTenantAsync(int? usuarioId)
        {
            if (!usuarioId.HasValue)
                return true; // Válido si no se asigna usuario

            var tenantId = TryGetTenantIdOrDefault();
            _logger.LogDebug("ValidateUsuarioAsignadoBelongsToTenantAsync checking usuario {UsuarioId} for tenant {TenantId}", usuarioId.Value, tenantId);
            var exists = await _context.Usuario
                .AnyAsync(u => u.Id == usuarioId.Value && u.IdInmobiliaria == tenantId && u.IdEstado == 1);
            _logger.LogDebug("ValidateUsuarioAsignadoBelongsToTenantAsync result: {Exists}", exists);
            return exists;
        }

        public override async Task<Lead> AddAsync(Lead entity)
        {
            try
            {
                var tenantId = TryGetTenantIdOrDefault();
                _logger.LogDebug("AddAsync starting for tenant {TenantId} with entity {@Entity}", tenantId, entity);

                // Si obtuvimos un tenant válido, forzamos IdInmobiliaria; si no, respetamos lo que venga en entity
                if (tenantId > 0)
                    entity.IdInmobiliaria = tenantId;

                entity.Activo = true; // Activo por defecto
                entity.IdEstado = entity.IdEstado == 0 ? 1 : entity.IdEstado; // Nuevo por defecto

                var added = await base.AddAsync(entity);
                _logger.LogInformation("AddAsync finished. Lead Id: {LeadId}", added.Id);
                return added;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en AddAsync al intentar guardar lead {@Entity}", entity);
                throw;
            }
        }

        // Agregar método CreateAsync para compatibilidad
        public async Task<Lead> CreateAsync(Lead entity)
        {
            _logger.LogDebug("CreateAsync delegando a AddAsync para entidad {@Entity}", entity);
            return await AddAsync(entity);
        }

        // Agregar método UpdateAsync para compatibilidad con el servicio
        public override async Task<Lead> UpdateAsync(Lead entity)
        {
            try
            {
                var tenantFromContext = TryGetTenantIdOrDefault();
                var entityTenant = entity.IdInmobiliaria;

                _logger.LogDebug("UpdateAsync iniciando para lead {LeadId}. Tenant from context: {ContextTenant}, Entity tenant: {EntityTenant}",
                    entity.Id, tenantFromContext, entityTenant);
                _logger.LogDebug("Entity antes del update: {@Entity}", entity);

                // En lugar de usar solo el tenant del context, verificamos si la entidad ya tiene el tenant correcto
                // Esto permite que el update funcione cuando el servicio ya estableció el tenant correcto
                if (tenantFromContext > 0 && entityTenant != tenantFromContext)
                {
                    _logger.LogError("Intento de modificar lead {LeadId} que no pertenece al tenant del contexto {ContextTenant}. Lead pertenece a {EntityTenant}",
                        entity.Id, tenantFromContext, entityTenant);
                    throw new UnauthorizedAccessException("No se puede modificar un lead que no pertenece al tenant actual");
                }

                // Si el tenant del contexto es 0 (local), permitir el update si la entidad tiene un tenant válido
                if (tenantFromContext == 0 && entityTenant > 0)
                {
                    _logger.LogWarning("Tenant context devolvió 0 (probablemente local), pero entity tiene tenant {EntityTenant}. Permitiendo update.", entityTenant);
                }

                entity.ActualizadoEn = DateTime.UtcNow;
                _logger.LogDebug("Llamando a base.UpdateAsync para lead {LeadId}", entity.Id);

                // FIX: Usar await directamente sin asignar a var
                await base.UpdateAsync(entity);
                _logger.LogInformation("UpdateAsync completado exitosamente para lead {LeadId}", entity.Id);

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en UpdateAsync para lead {LeadId}: {Message}", entity?.Id, ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<Lead>> GetByTenantAsync(int tenantId)
        {
            return await _dbSet
                .Include(l => l.Propiedad)
                .Include(l => l.Inmobiliaria)
                .Include(l => l.UsuarioAsignado)
                .Include(l => l.Cliente)
                .Include(l => l.Fuente)
                .Include(l => l.Estado)
                .Where(l => l.IdInmobiliaria == tenantId)
                .OrderByDescending(l => l.CreadoEn)
                .ToListAsync();
        }

        public async Task<Lead?> GetByIdAndTenantAsync(int id, int tenantId)
        {
            return await _dbSet
                .Include(l => l.Propiedad)
                .Include(l => l.Inmobiliaria)
                .Include(l => l.UsuarioAsignado)
                .Include(l => l.Cliente)
                .Include(l => l.Fuente)
                .Include(l => l.Estado)
                .FirstOrDefaultAsync(l => l.Id == id && l.IdInmobiliaria == tenantId);
        }

        public async Task<(IEnumerable<Lead> Data, int TotalRecords)> GetPagedByTenantAsync(
            int page, int pageSize, int tenantId, string? nombre = null, int? estadoId = null,
            int? fuenteId = null, int? usuarioAsignadoId = null, int? propiedadId = null, int? clienteId = null, bool? activo = true)
        {
            var query = _dbSet
                .Include(l => l.Propiedad)
                .Include(l => l.Inmobiliaria)
                .Include(l => l.UsuarioAsignado)
                .Include(l => l.Cliente)
                .Include(l => l.Fuente)
                .Include(l => l.Estado)
                .Where(l => l.IdInmobiliaria == tenantId);

            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(l => l.NombreCompleto.Contains(nombre) ||
                                        (l.Email != null && l.Email.Contains(nombre)));
            }

            if (estadoId.HasValue)
            {
                query = query.Where(l => l.IdEstado == estadoId.Value);
            }

            if (fuenteId.HasValue)
            {
                query = query.Where(l => l.IdFuente == fuenteId.Value);
            }

            if (usuarioAsignadoId.HasValue)
            {
                if (usuarioAsignadoId.Value == 0)
                    query = query.Where(l => l.IdUsuarioAsignado == null);
                else
                    query = query.Where(l => l.IdUsuarioAsignado == usuarioAsignadoId.Value);
            }

            if (propiedadId.HasValue)
            {
                query = query.Where(l => l.IdPropiedad == propiedadId.Value);
            }

            if (clienteId.HasValue)
            {
                if (clienteId.Value == 0)
                    query = query.Where(l => l.IdCliente == null);
                else
                    query = query.Where(l => l.IdCliente == clienteId.Value);
            }

            if (activo.HasValue)
            {
                query = query.Where(l => l.Activo == activo);
            }

            var totalRecords = await query.CountAsync();
            var data = await query
                .OrderByDescending(l => l.CreadoEn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalRecords);
        }

        public async Task<bool> ValidatePropiedadInTenantAsync(int propiedadId, int tenantId)
        {
            _logger.LogDebug("ValidatePropiedadInTenantAsync check propiedad {PropiedadId} for tenant {TenantId}", propiedadId, tenantId);
            var result = await _context.Propiedad
                .AnyAsync(p => p.Id == propiedadId && p.IdInmobiliaria == tenantId);
            _logger.LogDebug("ValidatePropiedadInTenantAsync result: {Result}", result);
            return result;
        }

        public async Task<bool> ValidateUsuarioInTenantAsync(int usuarioId, int tenantId)
        {
            _logger.LogDebug("ValidateUsuarioInTenantAsync check usuario {UsuarioId} for tenant {TenantId}", usuarioId, tenantId);
            var result = await _context.Usuario
                .AnyAsync(u => u.Id == usuarioId && u.IdInmobiliaria == tenantId && u.IdEstado == 1);
            _logger.LogDebug("ValidateUsuarioInTenantAsync result: {Result}", result);
            return result;
        }

        public async Task<bool> ValidateClienteInTenantAsync(int clienteId, int tenantId)
        {
            _logger.LogDebug("ValidateClienteInTenantAsync check cliente {ClienteId} for tenant {TenantId}", clienteId, tenantId);
            var result = await _context.Clientes
                .AnyAsync(c => c.Id == clienteId && c.IdInmobiliaria == tenantId && c.Activo == true);
            _logger.LogDebug("ValidateClienteInTenantAsync result: {Result}", result);
            return result;
        }
    }
}