namespace inmobiliariaApi.DTOs.Plan
{
    public class PlanDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal PrecioUsd { get; set; }
        public int? MaxPropiedades { get; set; }
        public int? MaxUsuarios { get; set; }
        public int? MaxLeadsMes { get; set; }
        public bool WhiteLabel { get; set; }
        public bool DominioPersonalizado { get; set; }
        public bool Automatizaciones { get; set; }
        public bool ApiAcceso { get; set; }
        public string Soporte { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }
        public bool HistorialEstados { get; set; }
        public bool ActividadesLead { get; set; }
        public string? TiposActividad { get; set; }
        public bool AutomatizacionLeads { get; set; }
        public bool LeadScoring { get; set; }
    }
}
