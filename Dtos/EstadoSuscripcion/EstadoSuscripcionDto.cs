namespace inmobiliariaApi.DTOs.EstadoSuscripcion
{
    public class EstadoSuscripcionDto
    {
        public byte Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool PermiteOperar { get; set; }
    }
}
