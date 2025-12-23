using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.Usuario
{
        public class UpdateUsuarioDto
        {
                [Required(ErrorMessage = "El ID del usuario es obligatorio.")]
                public int Id { get; set; }

                [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
                public string? Nombre { get; set; }

                [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
                [MaxLength(150, ErrorMessage = "El email no puede superar los 150 caracteres.")]
                public string? Email { get; set; }

                [MaxLength(50, ErrorMessage = "El teléfono no puede superar los 50 caracteres.")]
                public string? Telefono { get; set; }

                public int? IdRol { get; set; }

                public int? IdInmobiliaria { get; set; }

                public int? IdEstado { get; set; }

                // Contraseña (opcional, solo si se quiere cambiar)
                public string? Password { get; set; }
        }
}
