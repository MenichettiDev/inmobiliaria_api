using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.Usuario
{
    public class CreateUsuarioDto
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Apellido { get; set; }

        [Required]
        public String? Legajo { get; set; }

        [Required]
        [MaxLength(20)]
        public string Dni { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Telefono { get; set; }

        [Required]
        public int RolId { get; set; }

        public bool AccedeAlSistema { get; set; } = false;

        [MaxLength(45)]
        public string? Avatar { get; set; }

        public int IdUsuarioCrea { get; set; }

        // Cambiar a Password para recibir la contraseña en texto plano
        public string? Password { get; set; }
    }
}
