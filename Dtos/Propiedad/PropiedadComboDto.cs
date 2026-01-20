namespace inmobiliariaApi.DTOs.Propiedad
{
    public class PropiedadComboDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public decimal? Precio { get; set; }
    }
}
