using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.Lead
{
    public class UpdateLeadDto
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

        [StringLength(500, ErrorMessage = "El mensaje no puede exceder 500 caracteres")]
        public string? Mensaje { get; set; }

        [StringLength(200, ErrorMessage = "La dirección de interés no puede exceder 200 caracteres")]
        public string? DireccionInteres { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El presupuesto mínimo debe ser mayor o igual a 0")]
        public decimal? PresupuestoMinimo { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El presupuesto máximo debe ser mayor o igual a 0")]
        public decimal? PresupuestoMaximo { get; set; }

        [StringLength(50)]
        public string? TipoOperacionInteres { get; set; }

        public int? IdPropiedad { get; set; }

        [Required(ErrorMessage = "La fuente de contacto es obligatoria")]
        public int IdFuente { get; set; }

        [StringLength(1000, ErrorMessage = "Las notas no pueden exceder 1000 caracteres")]
        public string? Notas { get; set; }

        [Range(1, 5, ErrorMessage = "La puntuación debe estar entre 1 y 5")]
        public int? Puntuacion { get; set; }

        public int? IdUsuarioAsignado { get; set; }

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

            // Si se proporcionan ambos presupuestos, el mínimo debe ser menor al máximo
            if (PresupuestoMinimo.HasValue && PresupuestoMaximo.HasValue &&
                PresupuestoMinimo.Value > PresupuestoMaximo.Value)
            {
                yield return new ValidationResult(
                    "El presupuesto mínimo no puede ser mayor al máximo",
                    new[] { nameof(PresupuestoMinimo), nameof(PresupuestoMaximo) });
            }
        }
    }
}