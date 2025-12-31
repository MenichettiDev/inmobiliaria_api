using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.Lead
{
    public class CambiarEstadoLeadDto
    {
        [Required(ErrorMessage = "El ID del lead es obligatorio")]
        public int IdLead { get; set; }

        [Required(ErrorMessage = "El nuevo estado es obligatorio")]
        public int IdEstadoNuevo { get; set; }

        [StringLength(1000, ErrorMessage = "El comentario no puede exceder 1000 caracteres")]
        public string? Comentario { get; set; }
    }
}