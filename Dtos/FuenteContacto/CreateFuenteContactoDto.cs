using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.FuenteContacto
{
    public class CreateFuenteContactoDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(30, ErrorMessage = "El nombre no puede exceder los 30 caracteres")]
        public string Nombre { get; set; } = string.Empty;
    }
}
