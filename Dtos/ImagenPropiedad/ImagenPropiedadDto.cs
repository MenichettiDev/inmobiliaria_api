namespace inmobiliariaApi.DTOs.ImagenPropiedad
{
    public class ImagenPropiedadDto
    {
        public int Id { get; set; }
        public int IdPropiedad { get; set; }
        public string Url { get; set; } = string.Empty;
        public int Orden { get; set; }
        public DateTime CreadoEn { get; set; }

        // Información adicional
        public string? PropiedadTitulo { get; set; }
    }
}
