namespace inmobiliariaApi.DTOs.BusquedasGuardadas
{
    public class BusquedasGuardadasDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FiltrosJson { get; set; } = string.Empty;
        public DateTime? UltimoEnvio { get; set; }
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }
        public int IdInmobiliaria { get; set; }

        // Información adicional
        public string? InmobiliariaNombre { get; set; }
        public object? FiltrosParsed { get; set; } // Filtros parseados desde JSON
    }
}
