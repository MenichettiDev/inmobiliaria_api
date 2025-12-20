using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.Imagen
{
    public class CreateImagenDto
    {
        [Required(ErrorMessage = "La ruta de la imagen es obligatoria")]
        [MaxLength(255, ErrorMessage = "La ruta no puede exceder los 255 caracteres")]
        public string Ruta { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de imagen es obligatorio")]
        [MaxLength(50, ErrorMessage = "El tipo no puede exceder los 50 caracteres")]
        public string TipoImagen { get; set; } = string.Empty;

        [Required(ErrorMessage = "El ID relacionado es obligatorio")]
        public int IdRelacionado { get; set; }

        public bool Activo { get; set; } = true;
    }
}
