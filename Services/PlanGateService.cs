using Microsoft.EntityFrameworkCore;
using inmobiliariaApi.Data;
using inmobiliariaApi.Models;

namespace inmobiliariaApi.Services
{
    /// <summary>
    /// Verifica si un tenant puede ejecutar una acción según su plan activo.
    /// Usar en Services antes de operaciones que tienen límites por plan.
    /// </summary>
    public class PlanGateService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PlanGateService> _logger;

        public PlanGateService(ApplicationDbContext context, ILogger<PlanGateService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ── Obtener plan activo ───────────────────────────────────────────────

        private async Task<Plan?> GetPlanActivoAsync(int idInmobiliaria)
        {
            var ahora = DateTime.UtcNow;
            return await _context.Suscripciones
                .AsNoTracking()
                .Where(s => s.IdInmobiliaria == idInmobiliaria &&
                            s.IdEstado == 1 &&
                            s.Inicio <= ahora &&
                            s.Fin >= ahora)
                .OrderByDescending(s => s.Fin)
                .Select(s => s.Plan)
                .FirstOrDefaultAsync();
        }

        // ── Feature gates ─────────────────────────────────────────────────────

        public async Task<GateResult> PuedeCrearPropiedadAsync(int idInmobiliaria)
        {
            var plan = await GetPlanActivoAsync(idInmobiliaria);
            if (plan == null)
                return GateResult.Fail("Sin suscripción activa. Contrate un plan para continuar.");

            if (plan.MaxPropiedades == null) // null = ilimitado
                return GateResult.Pass();

            var actual = await _context.Propiedad
                .CountAsync(p => p.IdInmobiliaria == idInmobiliaria);

            if (actual >= plan.MaxPropiedades)
                return GateResult.Fail(
                    $"Límite de propiedades alcanzado ({plan.MaxPropiedades}). Actualice su plan.");

            return GateResult.Pass();
        }

        public async Task<GateResult> PuedeCrearUsuarioAsync(int idInmobiliaria)
        {
            var plan = await GetPlanActivoAsync(idInmobiliaria);
            if (plan == null)
                return GateResult.Fail("Sin suscripción activa.");

            if (plan.MaxUsuarios == null)
                return GateResult.Pass();

            var actual = await _context.Usuario
                .CountAsync(u => u.IdInmobiliaria == idInmobiliaria);

            if (actual >= plan.MaxUsuarios)
                return GateResult.Fail(
                    $"Límite de usuarios alcanzado ({plan.MaxUsuarios}). Actualice su plan.");

            return GateResult.Pass();
        }

        public async Task<GateResult> PuedeCrearLeadAsync(int idInmobiliaria)
        {
            var plan = await GetPlanActivoAsync(idInmobiliaria);
            if (plan == null)
                return GateResult.Fail("Sin suscripción activa.");

            if (plan.MaxLeadsMes == null)
                return GateResult.Pass();

            var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var leadsEsteMes = await _context.Lead
                .CountAsync(l => l.IdInmobiliaria == idInmobiliaria &&
                                 l.CreadoEn >= inicioMes);

            if (leadsEsteMes >= plan.MaxLeadsMes)
                return GateResult.Fail(
                    $"Límite mensual de leads alcanzado ({plan.MaxLeadsMes}). Actualice su plan.");

            return GateResult.Pass();
        }

        public async Task<GateResult> PuedeUsarApiAsync(int idInmobiliaria)
        {
            var plan = await GetPlanActivoAsync(idInmobiliaria);
            if (plan == null)
                return GateResult.Fail("Sin suscripción activa.");

            return plan.ApiAcceso
                ? GateResult.Pass()
                : GateResult.Fail("El acceso a la API no está incluido en su plan actual.");
        }

        public async Task<GateResult> PuedeUsarAutomatizacionesAsync(int idInmobiliaria)
        {
            var plan = await GetPlanActivoAsync(idInmobiliaria);
            if (plan == null)
                return GateResult.Fail("Sin suscripción activa.");

            return plan.AutomatizacionLeads
                ? GateResult.Pass()
                : GateResult.Fail("Las automatizaciones no están incluidas en su plan actual.");
        }

        // Devuelve un resumen del plan y uso actual para mostrar al tenant
        public async Task<object?> GetResumenPlanAsync(int idInmobiliaria)
        {
            var plan = await GetPlanActivoAsync(idInmobiliaria);
            if (plan == null) return null;

            var ahora = DateTime.UtcNow;
            var inicioMes = new DateTime(ahora.Year, ahora.Month, 1);

            var propiedades = await _context.Propiedad.CountAsync(p => p.IdInmobiliaria == idInmobiliaria);
            var usuarios = await _context.Usuario.CountAsync(u => u.IdInmobiliaria == idInmobiliaria);
            var leadsEsteMes = await _context.Lead
                .CountAsync(l => l.IdInmobiliaria == idInmobiliaria && l.CreadoEn >= inicioMes);

            return new
            {
                Plan = new { plan.Id, plan.Nombre, plan.PrecioUsd },
                Uso = new
                {
                    Propiedades = new { Actual = propiedades, Maximo = plan.MaxPropiedades },
                    Usuarios = new { Actual = usuarios, Maximo = plan.MaxUsuarios },
                    LeadsMes = new { Actual = leadsEsteMes, Maximo = plan.MaxLeadsMes }
                },
                Features = new
                {
                    plan.ApiAcceso,
                    plan.Automatizaciones,
                    plan.AutomatizacionLeads,
                    plan.LeadScoring,
                    plan.WhiteLabel,
                    plan.DominioPersonalizado,
                    plan.HistorialEstados,
                    plan.ActividadesLead
                }
            };
        }
    }

    public class GateResult
    {
        public bool Allowed { get; private set; }
        public string? Reason { get; private set; }

        private GateResult(bool allowed, string? reason = null)
        {
            Allowed = allowed;
            Reason = reason;
        }

        public static GateResult Pass() => new(true);
        public static GateResult Fail(string reason) => new(false, reason);
    }
}
