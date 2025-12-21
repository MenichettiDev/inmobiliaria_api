using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.Lead
{
    public class CambiarEstadoLeadDto
    {
        [Required(ErrorMessage = "El nuevo estado es obligatorio")]
        public int IdNuevoEstado { get; set; }

        [StringLength(1000, ErrorMessage = "El comentario no puede exceder 1000 caracteres")]
        public string? Comentario { get; set; }
    }
}