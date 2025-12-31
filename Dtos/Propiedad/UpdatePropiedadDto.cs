using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.Propiedad
{
    public class UpdatePropiedadDto
    {
        [Required]
        public int Id { get; set; }

        [MaxLength(150, ErrorMessage = "El título no puede exceder los 150 caracteres")]
        public string? Titulo { get; set; }

        [MaxLength(1000, ErrorMessage = "La descripción no puede exceder los 1000 caracteres")]
        public string? Descripcion { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal? Precio { get; set; }

        [MaxLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
        public string? Direccion { get; set; }

        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }

        public int? IdAgenteResponsable { get; set; }
        public int? IdEstadoAdmin { get; set; }
        public int? IdEstadoOperativo { get; set; }

        // El IdInmobiliaria NO se puede cambiar y NO viene del usuario
        public int? IdInmobiliaria { get; set; }
    }
}
