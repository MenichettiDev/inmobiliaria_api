using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.Plan
{
    public class UpdatePlanDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0, 9999.99, ErrorMessage = "El precio debe estar entre 0 y 9999.99")]
        public decimal PrecioUsd { get; set; }

        [Range(1, 10000, ErrorMessage = "El máximo de propiedades debe estar entre 1 y 10000")]
        public int? MaxPropiedades { get; set; }

        [Range(1, 1000, ErrorMessage = "El máximo de usuarios debe estar entre 1 y 1000")]
        public int? MaxUsuarios { get; set; }

        [Range(1, 100000, ErrorMessage = "El máximo de leads por mes debe estar entre 1 y 100000")]
        public int? MaxLeadsMes { get; set; }

        public bool WhiteLabel { get; set; }
        public bool DominioPersonalizado { get; set; }
        public bool Automatizaciones { get; set; }
        public bool ApiAcceso { get; set; }

        [Required(ErrorMessage = "El tipo de soporte es obligatorio")]
        [RegularExpression(@"^(email|prioritario|24x7)$", ErrorMessage = "El soporte debe ser: email, prioritario o 24x7")]
        public string Soporte { get; set; } = string.Empty;

        public bool Activo { get; set; }
        public bool HistorialEstados { get; set; }
        public bool ActividadesLead { get; set; }

        [MaxLength(500, ErrorMessage = "Los tipos de actividad no pueden exceder los 500 caracteres")]
        public string? TiposActividad { get; set; }

        public bool AutomatizacionLeads { get; set; }
        public bool LeadScoring { get; set; }
    }
}
