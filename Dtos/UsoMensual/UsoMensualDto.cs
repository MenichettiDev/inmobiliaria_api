namespace inmobiliariaApi.DTOs.UsoMensual
{
    public class UsoMensualDto
    {
        public int Id { get; set; }
        public int IdInmobiliaria { get; set; }
        public string Mes { get; set; } = string.Empty;
        public int LeadsGenerados { get; set; }
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }

        // Información adicional para reportes
        public string? InmobiliariaNombre { get; set; }
        public string MesTexto { get; set; } = string.Empty; // Ej: "Enero 2025"
    }
}
