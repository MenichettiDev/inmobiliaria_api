namespace inmobiliariaApi.DTOs.Propiedad
{
    public class PropiedadResponseDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal? Precio { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public DateTime? PublicadaEn { get; set; }
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }
        public int IdInmobiliaria { get; set; }
        public int? IdAgenteResponsable { get; set; }
        public string? AgenteResponsableNombre { get; set; }
        public int IdEstadoAdmin { get; set; }
        public int IdEstadoOperativo { get; set; }
        public string? EstadoAdminNombre { get; set; }
        public string? EstadoOperativoNombre { get; set; }
    }
}
