using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.Rol
{
    public class UpdateRolDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del rol es obligatorio")]
        [MaxLength(30, ErrorMessage = "El nombre no puede exceder los 30 caracteres")]
        public string Nombre { get; set; } = string.Empty;
    }
}
