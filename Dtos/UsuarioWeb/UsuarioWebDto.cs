namespace inmobiliariaApi.Dtos.UsuarioWeb
{
    public class UsuarioWebDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ProveedorAuth { get; set; } = "local";
        public bool EmailVerificado { get; set; }
        public DateTime CreadoEn { get; set; }
    }
}
