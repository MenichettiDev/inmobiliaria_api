using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace inmobiliariaApi.DTOs.Propiedad
{
    public class UpdateImagenInPropiedadDto
    {
        public int Id { get; set; }
        public string? Url { get; set; }
        public int Orden { get; set; }
    }

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
        public long? IdLocalidad { get; set; }

        // Imágenes: agregar (URLs o data-urls), eliminar (ids) y actualizar (id + url + orden)
        public List<string>? ImagenesParaAgregar { get; set; } = new List<string>();
        // Si el cliente sube archivos via multipart/form-data
        public List<IFormFile>? ImagenesFiles { get; set; } = new List<IFormFile>();
        public List<int>? ImagenesParaEliminar { get; set; } = new List<int>();
        public List<UpdateImagenInPropiedadDto>? ImagenesParaActualizar { get; set; } = new List<UpdateImagenInPropiedadDto>();
    }
}
