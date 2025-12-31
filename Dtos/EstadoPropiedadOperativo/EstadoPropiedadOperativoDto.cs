namespace inmobiliariaApi.DTOs.EstadoPropiedadOperativo
{
    public class EstadoPropiedadOperativoDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public string? ColorHex { get; set; }
    }
}
