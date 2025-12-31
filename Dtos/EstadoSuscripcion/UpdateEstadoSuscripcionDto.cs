using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.EstadoSuscripcion
{
    public class UpdateEstadoSuscripcionDto
    {
        [Required]
        public byte Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio")]
        [MaxLength(30, ErrorMessage = "El código no puede exceder los 30 caracteres")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [MaxLength(100, ErrorMessage = "La descripción no puede exceder los 100 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        public bool PermiteOperar { get; set; }
    }
}
