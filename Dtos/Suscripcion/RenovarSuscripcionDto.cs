using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.Suscripcion
{
    public class RenovarSuscripcionDto
    {
        [Required]
        public int IdSuscripcion { get; set; }

        [Required(ErrorMessage = "La nueva fecha de fin es obligatoria")]
        public DateTime NuevaFechaFin { get; set; }

        public int? NuevoIdPlan { get; set; } // Opcional para cambiar plan durante renovación
        public bool? RenovacionAutomatica { get; set; }
    }
}
