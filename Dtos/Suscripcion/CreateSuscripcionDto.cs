using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.Suscripcion
{
    public class CreateSuscripcionDto
    {
        [Required(ErrorMessage = "La inmobiliaria es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una inmobiliaria válida")]
        public int IdInmobiliaria { get; set; }

        [Required(ErrorMessage = "El plan es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un plan válido")]
        public int IdPlan { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        public DateTime Inicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria")]
        public DateTime Fin { get; set; }

        public bool RenovacionAutomatica { get; set; } = true;
        public byte IdEstado { get; set; } = 1; // Por defecto activa
    }
}
