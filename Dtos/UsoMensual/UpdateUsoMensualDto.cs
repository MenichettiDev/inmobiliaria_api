using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.UsoMensual
{
    public class UpdateUsoMensualDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "El mes es obligatorio")]
        [RegularExpression(@"^\d{4}-\d{2}$", ErrorMessage = "El mes debe tener formato YYYY-MM (ejemplo: 2025-01)")]
        public string Mes { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Los leads generados deben ser mayor o igual a 0")]
        public int LeadsGenerados { get; set; }

        // El IdInmobiliaria no se puede cambiar
        public int? IdInmobiliaria { get; set; }
    }
}
