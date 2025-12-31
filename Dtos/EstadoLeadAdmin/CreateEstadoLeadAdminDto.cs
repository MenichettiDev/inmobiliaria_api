using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.EstadoLeadAdmin
{
    public class CreateEstadoLeadAdminDto
    {
        [Required(ErrorMessage = "El código es obligatorio")]
        [MaxLength(50, ErrorMessage = "El código no puede exceder los 50 caracteres")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [MaxLength(100, ErrorMessage = "La descripción no puede exceder los 100 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }
}
