using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.Lead
{
    public class AsignarLeadDto
    {
        [Required]
        public int IdLead { get; set; }

        [Required]
        public int IdUsuarioAsignado { get; set; }

        [MaxLength(1000, ErrorMessage = "El comentario no puede exceder los 1000 caracteres")]
        public string? Comentario { get; set; }
    }
}
