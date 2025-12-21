using Microsoft.EntityFrameworkCore;
using pyreApi.Data;
using pyreApi.Models;
using pyreApi.Services;
using pyreApi.Exceptions;
using pyreApi.DTOs.Lead;
using System.Linq.Expressions;

namespace pyreApi.Repositories
{
    public class LeadRepository : GenericRepository<Lead>
    {
        private readonly ITenantContext _tenantContext;

        public LeadRepository(ApplicationDbContext context, ITenantContext tenantContext)
            : base(context)
        {
            _tenantContext = tenantContext;
        }

        // Sobrescribir métodos base para agregar filtro de tenant
        public override async Task<IEnumerable<Lead>> GetAllAsync()
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            return await _dbSet
                .Where(l => l.IdInmobiliaria == tenantId && l.IdEstadoAdmin != 3) // No eliminados
                .Include(l => l.Propiedad)
                .Include(l => l.FuenteContacto)
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
                .Include(l => l.FuenteContacto)
                .Include(l => l.Estado)
                .Include(l => l.UsuarioAsignado)
                .Include(l => l.UsuarioCrea)
                .Include(l => l.UsuarioModifica)
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
            lead.FechaUltimaInteraccion = DateTime.UtcNow;
            lead.FechaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return lead;
        }

        public async Task<(IEnumerable<Lead> Items, int TotalCount)> GetLeadsConFiltrosAsync(LeadFiltrosDto filtros)
        {
            var tenantId = _tenantContext.GetCurrentTenantId();

            var query = _dbSet
                .Where(l => l.IdInmobiliaria == tenantId && l.IdEstadoAdmin != 3)
                .Include(l => l.Propiedad)
                .Include(l => l.FuenteContacto)
                .Include(l => l.Estado)
                .Include(l => l.UsuarioAsignado)
                .Include(l => l.UsuarioCrea)
                .Include(l => l.UsuarioModifica)
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

            if (filtros.IdFuenteContacto.HasValue)
            {
                query = query.Where(l => l.IdFuenteContacto == filtros.IdFuenteContacto.Value);
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
                query = query.Where(l => l.FechaContacto >= filtros.FechaDesde.Value);
            }

            if (filtros.FechaHasta.HasValue)
            {
                query = query.Where(l => l.FechaContacto <= filtros.FechaHasta.Value.AddDays(1));
            }

            if (filtros.PuntuacionMinima.HasValue)
            {
                query = query.Where(l => l.Puntuacion >= filtros.PuntuacionMinima.Value);
            }

            if (filtros.PuntuacionMaxima.HasValue)
            {
                query = query.Where(l => l.Puntuacion <= filtros.PuntuacionMaxima.Value);
            }

            if (!string.IsNullOrWhiteSpace(filtros.TipoOperacion))
            {
                query = query.Where(l => l.TipoOperacionInteres == filtros.TipoOperacion);
            }

            if (filtros.PresupuestoMinimo.HasValue)
            {
                query = query.Where(l => l.PresupuestoMaximo == null || l.PresupuestoMaximo >= filtros.PresupuestoMinimo.Value);
            }

            if (filtros.PresupuestoMaximo.HasValue)
            {
                query = query.Where(l => l.PresupuestoMinimo == null || l.PresupuestoMinimo <= filtros.PresupuestoMaximo.Value);
            }

            // Contar total antes de paginación
            var totalCount = await query.CountAsync();

            // Aplicar ordenamiento
            if (!string.IsNullOrWhiteSpace(filtros.OrderBy))
            {
                switch (filtros.OrderBy.ToLower())
                {
                    case "fechacontacto":
                        query = filtros.OrderDescending
                            ? query.OrderByDescending(l => l.FechaContacto)
                            : query.OrderBy(l => l.FechaContacto);
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
                    case "puntuacion":
                        query = filtros.OrderDescending
                            ? query.OrderByDescending(l => l.Puntuacion)
                            : query.OrderBy(l => l.Puntuacion);
                        break;
                    default: // fechacreacion
                        query = filtros.OrderDescending
                            ? query.OrderByDescending(l => l.FechaCreacion)
                            : query.OrderBy(l => l.FechaCreacion);
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
                           l.FechaCreacion >= startDate &&
                           l.FechaCreacion < endDate &&
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
                .AnyAsync(u => u.Id == usuarioId.Value && u.IdInmobiliaria == tenantId && u.Activo);
        }

        public override async Task<Lead> CreateAsync(Lead entity)
        {
            // Asegurar que el lead pertenece al tenant actual
            entity.IdInmobiliaria = _tenantContext.GetCurrentTenantId();
            entity.IdEstadoAdmin = 1; // Activo por defecto
            entity.IdEstado = entity.IdEstado == 0 ? 1 : entity.IdEstado; // Nuevo por defecto

            return await base.CreateAsync(entity);
        }
    }
}