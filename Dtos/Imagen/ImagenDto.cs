namespace pyreApi.DTOs.Imagen
{
    public class ImagenDto
    {
        public int IdImagen { get; set; }
        public string Ruta { get; set; } = string.Empty;
        public string TipoImagen { get; set; } = string.Empty;
        public int IdRelacionado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; }
    }
}
