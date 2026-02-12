using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.Cliente
{
    // ...new file...
    public class UpdateClienteDto
    {
        [Required]
        public int Id { get; set; }

        [MaxLength(150)]
        public string? NombreCompleto { get; set; }

        [MaxLength(20)]
        public string? Dni { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        // Mantener tenant desde controller
        public int IdInmobiliaria { get; set; }

        public bool? Activo { get; set; }
    }
}
