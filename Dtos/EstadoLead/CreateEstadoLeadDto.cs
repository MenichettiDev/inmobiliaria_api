using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.EstadoLead
{
    public class CreateEstadoLeadDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(30, ErrorMessage = "El nombre no puede exceder los 30 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }
}
