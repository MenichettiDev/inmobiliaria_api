using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.BusquedasGuardadas
{
    public class UpdateBusquedasGuardadasDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Debe ser un email válido")]
        [MaxLength(100, ErrorMessage = "El email no puede exceder los 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los filtros son obligatorios")]
        public string FiltrosJson { get; set; } = string.Empty;

        // El IdInmobiliaria no se puede cambiar
        public int? IdInmobiliaria { get; set; }
    }
}
