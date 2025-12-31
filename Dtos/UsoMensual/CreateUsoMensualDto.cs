using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.UsoMensual
{
    public class CreateUsoMensualDto
    {
        [Required(ErrorMessage = "El mes es obligatorio")]
        [RegularExpression(@"^\d{4}-\d{2}$", ErrorMessage = "El mes debe tener formato YYYY-MM (ejemplo: 2025-01)")]
        public string Mes { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Los leads generados deben ser mayor o igual a 0")]
        public int LeadsGenerados { get; set; } = 0;

        // El IdInmobiliaria se toma del tenant (no del DTO)
        public int IdInmobiliaria { get; set; }
    }
}
