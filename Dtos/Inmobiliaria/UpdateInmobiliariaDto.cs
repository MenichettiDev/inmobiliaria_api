using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.Inmobiliaria
{
    public class UpdateInmobiliariaDto
    {
        [Required]
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string? Nombre { get; set; }

        [MaxLength(50, ErrorMessage = "El subdominio no puede exceder los 50 caracteres")]
        [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "El subdominio solo puede contener letras minúsculas, números y guiones")]
        public string? Subdominio { get; set; }

        [MaxLength(100, ErrorMessage = "El dominio personalizado no puede exceder los 100 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*$",
            ErrorMessage = "El dominio personalizado debe tener un formato válido")]
        public string? DominioPersonalizado { get; set; }

        public int? IdPlan { get; set; }
        public int? IdEstado { get; set; }
    }
}
