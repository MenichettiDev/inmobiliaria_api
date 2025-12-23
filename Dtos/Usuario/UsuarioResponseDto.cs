namespace inmobiliariaApi.DTOs.Usuario
{
    public class UsuarioResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public int IdInmobiliaria { get; set; }
        public int IdRol { get; set; }
        public int IdEstado { get; set; }
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }
        public string RolNombre { get; set; } = string.Empty;
    }
}
