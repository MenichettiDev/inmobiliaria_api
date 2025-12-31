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

        // Sobrescribir métodos base para agregar filtro de tenant
        public override async Task<IEnumerable<Lead>> GetAllAsync()
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            return await _dbSet
                .Where(l => l.IdInmobiliaria == tenantId && l.IdEstadoAdmin != 3) // No eliminados
                .Include(l => l.Propiedad)
                .Include(l => l.Fuente)
                .Include(l => l.Estado)
                .Include(l => l.UsuarioAsignado)
                .ToListAsync();
        }

        public override async Task<Lead?> GetByIdAsync(int id)
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            return await _dbSet
                .Where(l => l.IdInmobiliaria == tenantId && l.Id == id && l.IdEstadoAdmin != 3)
                .Include(l => l.Propiedad)
                .Include(l => l.Fuente)
                .Include(l => l.Estado)
                .Include(l => l.UsuarioAsignado)
                .Include(l => l.EstadoAdmin)
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
            var tenantId = _tenantContext.GetCurrentTenantId();

            var query = _dbSet
                .Where(l => l.IdInmobiliaria == tenantId && l.IdEstadoAdmin != 3)
                .Include(l => l.Propiedad)
                .Include(l => l.Fuente)
                .Include(l => l.Estado)
                .Include(l => l.UsuarioAsignado)
                .Include(l => l.EstadoAdmin)
                .AsQueryable();

            // Aplicar filtros
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

            if (filtros.FechaDesde.HasValue)
            {
                query = query.Where(l => l.CreadoEn >= filtros.FechaDesde.Value);
            }

            if (filtros.FechaHasta.HasValue)
            {
                query = query.Where(l => l.CreadoEn <= filtros.FechaHasta.Value.AddDays(1));
            }

            // Contar total antes de paginación
            var totalCount = await query.CountAsync();

            // Aplicar ordenamiento
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
                        query = filtros.OrderDescending
                            ? query.OrderByDescending(l => l.Estado!.Nombre)
                            : query.OrderBy(l => l.Estado!.Nombre);
                        break;
                    default: // creado_en por defecto
                        query = filtros.OrderDescending
                            ? query.OrderByDescending(l => l.CreadoEn)
                            : query.OrderBy(l => l.CreadoEn);
                        break;
                }
            }

            // Aplicar paginación
            var items = await query
                .Skip((filtros.Page - 1) * filtros.PageSize)
                .Take(filtros.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> GetLeadsCountByMonthAsync(int year, int month)
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            return await _dbSet
                .Where(l => l.IdInmobiliaria == tenantId &&
                           l.CreadoEn >= startDate &&
                           l.CreadoEn < endDate &&
                           l.IdEstadoAdmin != 3)
                .CountAsync();
        }

        public async Task<bool> ValidatePropertiesBelongsToTenantAsync(int? propiedadId)
        {
            if (!propiedadId.HasValue)
                return true; // Válido si no se especifica propiedad

            var tenantId = _tenantContext.GetCurrentTenantId();
            return await _context.Propiedad
                .AnyAsync(p => p.Id == propiedadId.Value && p.IdInmobiliaria == tenantId);
        }

        // Método para validar que el usuario asignado pertenece al tenant
        public async Task<bool> ValidateUsuarioAsignadoBelongsToTenantAsync(int? usuarioId)
        {
            if (!usuarioId.HasValue)
                return true; // Válido si no se asigna usuario

            var tenantId = _tenantContext.GetCurrentTenantId();
            return await _context.Usuario
                .AnyAsync(u => u.Id == usuarioId.Value && u.IdInmobiliaria == tenantId && u.IdEstado == 1);
        }

        public override async Task<Lead> AddAsync(Lead entity)
        {
            // Asegurar que el lead pertenece al tenant actual
            entity.IdInmobiliaria = _tenantContext.GetCurrentTenantId();
            entity.IdEstadoAdmin = 1; // Activo por defecto
            entity.IdEstado = entity.IdEstado == 0 ? 1 : entity.IdEstado; // Nuevo por defecto

            return await base.AddAsync(entity);
        }

        // Agregar método CreateAsync para compatibilidad
        public async Task<Lead> CreateAsync(Lead entity)
        {
            return await AddAsync(entity);
        }

        // Agregar método UpdateAsync para compatibilidad con el servicio
        public override async Task<Lead> UpdateAsync(Lead entity)
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            if (entity.IdInmobiliaria != tenantId)
            {
                throw new UnauthorizedAccessException("No se puede modificar un lead que no pertenece al tenant actual");
            }

            entity.ActualizadoEn = DateTime.UtcNow;
            await base.UpdateAsync(entity);
            return entity;
        }

        public async Task<IEnumerable<Lead>> GetByTenantAsync(int tenantId)
        {
            return await _dbSet
                .Include(l => l.Propiedad)
                .Include(l => l.Inmobiliaria)
                .Include(l => l.UsuarioAsignado)
                .Include(l => l.Fuente)
                .Include(l => l.Estado)
                .Include(l => l.EstadoAdmin)
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
                .Include(l => l.Fuente)
                .Include(l => l.Estado)
                .Include(l => l.EstadoAdmin)
                .FirstOrDefaultAsync(l => l.Id == id && l.IdInmobiliaria == tenantId);
        }

        public async Task<(IEnumerable<Lead> Data, int TotalRecords)> GetPagedByTenantAsync(
            int page, int pageSize, int tenantId, string? nombre = null, int? estadoId = null,
            int? fuenteId = null, int? usuarioAsignadoId = null, int? propiedadId = null, int? estadoAdminId = null)
        {
            var query = _dbSet
                .Include(l => l.Propiedad)
                .Include(l => l.Inmobiliaria)
                .Include(l => l.UsuarioAsignado)
                .Include(l => l.Fuente)
                .Include(l => l.Estado)
                .Include(l => l.EstadoAdmin)
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

            if (estadoAdminId.HasValue)
            {
                query = query.Where(l => l.IdEstadoAdmin == estadoAdminId.Value);
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
            return await _context.Propiedad
                .AnyAsync(p => p.Id == propiedadId && p.IdInmobiliaria == tenantId);
        }

        public async Task<bool> ValidateUsuarioInTenantAsync(int usuarioId, int tenantId)
        {
            return await _context.Usuario
                .AnyAsync(u => u.Id == usuarioId && u.IdInmobiliaria == tenantId && u.IdEstado == 1);
        }

        public async Task<int> GetCountByFuenteAndTenantAsync(int fuenteId, int tenantId, DateTime? desde = null, DateTime? hasta = null)
        {
            var query = _dbSet.Where(l => l.IdFuente == fuenteId && l.IdInmobiliaria == tenantId);

            if (desde.HasValue)
                query = query.Where(l => l.CreadoEn >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(l => l.CreadoEn <= hasta.Value);

            return await query.CountAsync();
        }

        public async Task<IEnumerable<Lead>> GetLeadsActivosByTenantAsync(int tenantId)
        {
            return await _dbSet
                .Include(l => l.Propiedad)
                .Include(l => l.UsuarioAsignado)
                .Include(l => l.Fuente)
                .Include(l => l.Estado)
                .Where(l => l.IdInmobiliaria == tenantId && l.IdEstadoAdmin == 1)
                .OrderByDescending(l => l.CreadoEn)
                .ToListAsync();
        }
    }
}