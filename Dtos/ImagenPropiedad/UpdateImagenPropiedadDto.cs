using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.ImagenPropiedad
{
    public class UpdateImagenPropiedadDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "La URL de la imagen es obligatoria")]
        [MaxLength(255, ErrorMessage = "La URL no puede exceder los 255 caracteres")]
        [Url(ErrorMessage = "Debe ser una URL válida")]
        public string Url { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "El orden debe estar entre 0 y 100")]
        public int Orden { get; set; }

        // El IdPropiedad no se puede cambiar
        public int? IdPropiedad { get; set; }
    }
}
