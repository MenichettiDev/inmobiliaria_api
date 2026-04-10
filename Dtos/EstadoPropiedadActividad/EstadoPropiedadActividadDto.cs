namespace inmobiliariaApi.DTOs.EstadoPropiedadActividad
{
    public class EstadoPropiedadActividadDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Visible { get; set; }
    }
}
