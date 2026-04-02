using Microsoft.AspNetCore.Mvc;

namespace inmobiliariaApi.DTOs.ImagenPropiedad
{
    public class UploadImagenDto
    {
        [FromForm(Name = "idPropiedad")]
        public int IdPropiedad { get; set; }

        [FromForm(Name = "archivo")]
        public IFormFile Archivo { get; set; } = null!;
    }
}
