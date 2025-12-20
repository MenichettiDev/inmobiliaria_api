using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.Rol
{
    public class CreateRolDto
    {
        [Required(ErrorMessage = "El nombre del rol es requerido")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        public string NombreRol { get; set; } = string.Empty;
    }
}
