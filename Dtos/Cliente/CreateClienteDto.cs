using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.Cliente
{
    // ...new file...
    public class CreateClienteDto
    {
        [Required]
        [MaxLength(150)]
        public string NombreCompleto { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Dni { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        // Se asigna en controller desde el tenant autenticado
        public int IdInmobiliaria { get; set; }
    }
}
