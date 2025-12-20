using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.Usuario
{
        public class UpdateUsuarioDto
        {
                [Required(ErrorMessage = "El ID del usuario es obligatorio.")]
                public int Id { get; set; }

                [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
                public string? Nombre { get; set; }

                [MaxLength(100, ErrorMessage = "El apellido no puede superar los 100 caracteres.")]
                public string? Apellido { get; set; }

                [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
                [MaxLength(150, ErrorMessage = "El email no puede superar los 150 caracteres.")]
                public string? Email { get; set; }

                [MaxLength(50, ErrorMessage = "El teléfono no puede superar los 50 caracteres.")]
                public string? Telefono { get; set; }

                [MaxLength(5, ErrorMessage = "El legajo no puede tener más de 5 caracteres.")]
                public string? Legajo { get; set; } // ✅ NUEVO CAMPO AGREGADO

                public int? RolId { get; set; }

                public bool? AccedeAlSistema { get; set; }

                [MaxLength(255, ErrorMessage = "El nombre del avatar no puede superar los 255 caracteres.")]
                public string? Avatar { get; set; }

                [Required(ErrorMessage = "Debe especificar el ID del usuario que modifica.")]
                public int IdUsuarioModifica { get; set; }

                [MaxLength(255, ErrorMessage = "La contraseña no puede superar los 255 caracteres.")]
                public string? Password { get; set; }
        }
}
