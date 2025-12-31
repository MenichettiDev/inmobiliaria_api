using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.Propiedad
{
    public class CreatePropiedadDto
    {
        [Required(ErrorMessage = "El título es obligatorio")]
        [MaxLength(150, ErrorMessage = "El título no puede exceder los 150 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [MaxLength(1000, ErrorMessage = "La descripción no puede exceder los 1000 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal? Precio { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [MaxLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
        public string Direccion { get; set; } = string.Empty;

        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }

        public int? IdAgenteResponsable { get; set; }

        // El IdInmobiliaria se asigna automáticamente desde el tenant
        // NO viene del usuario, siempre se toma del token JWT
        public int IdInmobiliaria { get; set; }
    }
}
