using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.Lead
{
    public class CreateLeadDto
    {
        public int? IdPropiedad { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string NombreCompleto { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Debe ser un email válido")]
        [MaxLength(150, ErrorMessage = "El email no puede exceder los 150 caracteres")]
        public string? Email { get; set; }

        [MaxLength(50, ErrorMessage = "El teléfono no puede exceder los 50 caracteres")]
        public string? Telefono { get; set; }

        [MaxLength(1000, ErrorMessage = "El mensaje no puede exceder los 1000 caracteres")]
        public string? Mensaje { get; set; }

        [Required(ErrorMessage = "La fuente de contacto es obligatoria")]
        public int IdFuente { get; set; }

        public int? IdUsuarioAsignado { get; set; }
        public int IdEstado { get; set; } = 1; // Por defecto "nuevo"

        // El IdInmobiliaria se toma del tenant (no del DTO)
        public int IdInmobiliaria { get; set; }
    }
}