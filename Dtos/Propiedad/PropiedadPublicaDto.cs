namespace inmobiliariaApi.DTOs.Propiedad
{
    public class PropiedadPublicaDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal? Precio { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public string? LocalidadNombre { get; set; }
        public string? ProvinciaNombre { get; set; }
        public DateTime? PublicadaEn { get; set; }

        // Info de la inmobiliaria
        public int IdInmobiliaria { get; set; }
        public string InmobiliariaNombre { get; set; } = string.Empty;
        public string InmobiliariaSubdominio { get; set; } = string.Empty;

        // Para frontend
        public List<string>? UrlImagenes { get; set; } = new List<string>();
    }
}
