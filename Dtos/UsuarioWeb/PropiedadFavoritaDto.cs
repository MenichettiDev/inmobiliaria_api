namespace inmobiliariaApi.Dtos.UsuarioWeb
{
    public class PropiedadFavoritaDto
    {
        public int Id { get; set; }
        public int IdPropiedad { get; set; }
        public DateTime CreadoEn { get; set; }

        // Datos desnormalizados de la propiedad
        public string PropiedadTitulo { get; set; } = string.Empty;
        public string PropiedadDireccion { get; set; } = string.Empty;
        public decimal? PropiedadPrecio { get; set; }
        public string? PropiedadImageUrl { get; set; }
    }
}
