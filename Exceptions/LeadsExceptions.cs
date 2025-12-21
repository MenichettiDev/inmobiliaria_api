namespace pyreApi.Exceptions
{
    public class LimiteDeLeadsExcedidoException : Exception
    {
        public int LimiteActual { get; }
        public int LeadsUsados { get; }
        public string PlanActual { get; }

        public LimiteDeLeadsExcedidoException(int limiteActual, int leadsUsados, string planActual)
            : base($"Se ha alcanzado el límite mensual de leads ({leadsUsados}/{limiteActual}) para el plan {planActual}. Actualice su plan para crear más leads.")
        {
            LimiteActual = limiteActual;
            LeadsUsados = leadsUsados;
            PlanActual = planActual;
        }

        public LimiteDeLeadsExcedidoException(int limiteActual, int leadsUsados, string planActual, Exception innerException)
            : base($"Se ha alcanzado el límite mensual de leads ({leadsUsados}/{limiteActual}) para el plan {planActual}. Actualice su plan para crear más leads.", innerException)
        {
            LimiteActual = limiteActual;
            LeadsUsados = leadsUsados;
            PlanActual = planActual;
        }
    }

    public class InmobiliariaInactivaException : Exception
    {
        public int InmobiliariaId { get; }
        public string EstadoActual { get; }

        public InmobiliariaInactivaException(int inmobiliariaId, string estadoActual)
            : base($"La inmobiliaria (ID: {inmobiliariaId}) está inactiva (Estado: {estadoActual}). No se pueden realizar operaciones.")
        {
            InmobiliariaId = inmobiliariaId;
            EstadoActual = estadoActual;
        }

        public InmobiliariaInactivaException(int inmobiliariaId, string estadoActual, Exception innerException)
            : base($"La inmobiliaria (ID: {inmobiliariaId}) está inactiva (Estado: {estadoActual}). No se pueden realizar operaciones.", innerException)
        {
            InmobiliariaId = inmobiliariaId;
            EstadoActual = estadoActual;
        }
    }

    public class TenantAccessViolationException : Exception
    {
        public int TenantActual { get; }
        public int TenantSolicitado { get; }

        public TenantAccessViolationException(int tenantActual, int tenantSolicitado)
            : base($"Acceso denegado: intento de acceder a datos de la inmobiliaria {tenantSolicitado} desde la inmobiliaria {tenantActual}")
        {
            TenantActual = tenantActual;
            TenantSolicitado = tenantSolicitado;
        }
    }

    public class LeadNotFoundException : Exception
    {
        public int LeadId { get; }

        public LeadNotFoundException(int leadId)
            : base($"Lead con ID {leadId} no encontrado o no pertenece a la inmobiliaria actual")
        {
            LeadId = leadId;
        }
    }
}