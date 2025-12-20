namespace pyreApi.DTOs.Usuario
{
    public class UsuarioResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Apellido { get; set; }
        public string? Legajo { get; set; }
        public string? Dni { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public bool AccedeAlSistema { get; set; }
        public bool Activo { get; set; }
        public string? Avatar { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime FechaModificacion { get; set; }
        public string RolNombre { get; set; } = string.Empty;
    }
}
