namespace inmobiliariaApi.DTOs.Inmobiliaria
{
    public class InmobiliariaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Subdominio { get; set; } = string.Empty;
        public string? DominioPersonalizado { get; set; }
        public int IdPlan { get; set; }
        public int IdEstado { get; set; }
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }

        // Información adicional
        public string? PlanNombre { get; set; }
        public string? EstadoDescripcion { get; set; }
        public int TotalUsuarios { get; set; }
        public int TotalPropiedades { get; set; }
        public int TotalLeads { get; set; }
    }
}
