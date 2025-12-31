namespace inmobiliariaApi.DTOs.Suscripcion
{
    public class SuscripcionDto
    {
        public int Id { get; set; }
        public int IdInmobiliaria { get; set; }
        public int IdPlan { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
        public bool RenovacionAutomatica { get; set; }
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }
        public byte IdEstado { get; set; }
        
        // Información adicional
        public string? InmobiliariaNombre { get; set; }
        public string? PlanNombre { get; set; }
        public string? EstadoDescripcion { get; set; }
        public bool Vigente { get; set; } // Si está dentro del período de vigencia
        public int DiasRestantes { get; set; } // Días hasta el vencimiento
        public bool PorVencer { get; set; } // Si vence en los próximos 30 días
    }
}
