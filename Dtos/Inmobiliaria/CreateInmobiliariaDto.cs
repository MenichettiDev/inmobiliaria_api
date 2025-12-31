using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.Inmobiliaria
{
    public class CreateInmobiliariaDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El subdominio es obligatorio")]
        [MaxLength(50, ErrorMessage = "El subdominio no puede exceder los 50 caracteres")]
        [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "El subdominio solo puede contener letras minúsculas, números y guiones")]
        public string Subdominio { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "El dominio personalizado no puede exceder los 100 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*$",
            ErrorMessage = "El dominio personalizado debe tener un formato válido")]
        public string? DominioPersonalizado { get; set; }

        [Required(ErrorMessage = "El plan es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un plan válido")]
        public int IdPlan { get; set; }

        public int IdEstado { get; set; } = 1; // Por defecto activa
    }
}
