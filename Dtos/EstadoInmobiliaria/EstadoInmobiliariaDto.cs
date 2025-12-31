namespace inmobiliariaApi.DTOs.EstadoInmobiliaria
{
    public class EstadoInmobiliariaDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
