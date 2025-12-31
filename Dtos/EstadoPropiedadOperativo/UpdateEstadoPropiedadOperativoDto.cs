using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.EstadoPropiedadOperativo
{
    public class UpdateEstadoPropiedadOperativoDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio")]
        [MaxLength(50, ErrorMessage = "El código no puede exceder los 50 caracteres")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "La descripción no puede exceder los 200 caracteres")]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; }

        [MaxLength(7, ErrorMessage = "El color debe ser un código hex válido (#FFFFFF)")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "El color debe ser un código hex válido (ejemplo: #FF0000)")]
        public string? ColorHex { get; set; }
    }
}
