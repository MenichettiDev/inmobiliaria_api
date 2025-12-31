using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.LeadEstadoHistorial
{
    public class CreateLeadEstadoHistorialDto
    {
        [Required(ErrorMessage = "El ID del lead es obligatorio")]
        public int IdLead { get; set; }

        [Required(ErrorMessage = "El estado anterior es obligatorio")]
        public int IdEstadoAnterior { get; set; }

        [Required(ErrorMessage = "El nuevo estado es obligatorio")]
        public int IdEstadoNuevo { get; set; }

        [MaxLength(1000, ErrorMessage = "El comentario no puede exceder los 1000 caracteres")]
        public string? Comentario { get; set; }

        // El IdUsuario se toma del token JWT (no del DTO)
        public int IdUsuario { get; set; }
    }
}
