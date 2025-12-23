using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.Lead
{
    public class CreateLeadDto
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre completo no puede exceder 100 caracteres")]
        public string NombreCompleto { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(150, ErrorMessage = "El email no puede exceder 150 caracteres")]
        public string? Email { get; set; }

        [StringLength(50, ErrorMessage = "El teléfono no puede exceder 50 caracteres")]
        [RegularExpression(@"^[\d\s\+\-\(\)]+$", ErrorMessage = "El teléfono solo puede contener números, espacios y los siguientes caracteres: +()-")]
        public string? Telefono { get; set; }

        [StringLength(1000, ErrorMessage = "El mensaje no puede exceder 1000 caracteres")]
        public string? Mensaje { get; set; }

        public int? IdPropiedad { get; set; }

        [Required(ErrorMessage = "La fuente de contacto es obligatoria")]
        public int IdFuente { get; set; }

        // Validación personalizada
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Al menos email o teléfono debe estar presente
            if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(Telefono))
            {
                yield return new ValidationResult(
                    "Debe proporcionar al menos un email o teléfono",
                    new[] { nameof(Email), nameof(Telefono) });
            }

        }
    }
}