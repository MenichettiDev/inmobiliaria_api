using inmobiliariaApi.Repositories;
using inmobiliariaApi.Services;
using inmobiliariaApi.Models;
using inmobiliariaApi.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace inmobiliariaApi.Services
{
    public interface IUsoMensualService
    {
        Task ValidarLimiteLeadsAsync();
        Task<(int usados, int limite)> GetUsoLeadsActualAsync();
    }

    public class UsoMensualService : IUsoMensualService
    {
        private readonly ITenantContext _tenantContext;
        private readonly LeadRepository _leadRepository;

        public UsoMensualService(ITenantContext tenantContext, LeadRepository leadRepository)
        {
            _tenantContext = tenantContext;
            _leadRepository = leadRepository;
        }

        public async Task ValidarLimiteLeadsAsync()
        {
            var inmobiliaria = await _tenantContext.GetCurrentInmobiliariaAsync();

            if (inmobiliaria == null)
                throw new InvalidOperationException("No se puede determinar la inmobiliaria actual");

            // Validar que la inmobiliaria está activa
            if (inmobiliaria.IdEstado != 1)
            {
                throw new InmobiliariaInactivaException(
                    inmobiliaria.Id,
                    inmobiliaria.Estado?.Descripcion ?? "Desconocido"
                );
            }

            var now = DateTime.UtcNow;
            var leadsUsados = await _leadRepository.GetLeadsCountByMonthAsync(now.Year, now.Month);
            var limite = inmobiliaria.Plan?.MaxLeadsMes ?? 0;

            if (limite > 0 && leadsUsados >= limite)
            {
                throw new LimiteDeLeadsExcedidoException(
                    limite,
                    leadsUsados,
                    inmobiliaria.Plan?.Nombre ?? "Desconocido"
                );
            }
        }

        public async Task<(int usados, int limite)> GetUsoLeadsActualAsync()
        {
            var inmobiliaria = await _tenantContext.GetCurrentInmobiliariaAsync();
            if (inmobiliaria == null)
                throw new InvalidOperationException("No se puede determinar la inmobiliaria actual");

            var now = DateTime.UtcNow;
            var usados = await _leadRepository.GetLeadsCountByMonthAsync(now.Year, now.Month);
            var limite = inmobiliaria.Plan?.MaxLeadsMes ?? 0;

            return (usados, limite);
        }
    }
}