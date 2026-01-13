using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.Lead
{
    public class UpdateLeadDto
    {
        [Required]
        public int Id { get; set; }

        public int? IdPropiedad { get; set; }

        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string? NombreCompleto { get; set; }

        [EmailAddress(ErrorMessage = "Debe ser un email válido")]
        [MaxLength(150, ErrorMessage = "El email no puede exceder los 150 caracteres")]
        public string? Email { get; set; }

        [MaxLength(50, ErrorMessage = "El teléfono no puede exceder los 50 caracteres")]
        public string? Telefono { get; set; }

        [MaxLength(1000, ErrorMessage = "El mensaje no puede exceder los 1000 caracteres")]
        public string? Mensaje { get; set; }

        public int? IdFuente { get; set; }
        public int? IdUsuarioAsignado { get; set; }
        public int? IdEstado { get; set; }
        public bool? Activo { get; set; }

        // El IdInmobiliaria no se puede cambiar
        public int? IdInmobiliaria { get; set; }
    }
}