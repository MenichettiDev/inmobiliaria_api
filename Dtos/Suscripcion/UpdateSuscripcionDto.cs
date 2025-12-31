using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.Suscripcion
{
    public class UpdateSuscripcionDto
    {
        [Required]
        public int Id { get; set; }

        public DateTime? Inicio { get; set; }
        public DateTime? Fin { get; set; }
        public bool? RenovacionAutomatica { get; set; }
        public byte? IdEstado { get; set; }
        public int? IdPlan { get; set; }
        
        // El IdInmobiliaria no se puede cambiar
        public int? IdInmobiliaria { get; set; }
    }
}
