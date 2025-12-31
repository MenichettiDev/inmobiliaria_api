namespace inmobiliariaApi.DTOs.EstadoUsuario
{
    public class EstadoUsuarioDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
