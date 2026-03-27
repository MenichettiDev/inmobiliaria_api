using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.Dtos.UsuarioWeb
{
    public class GoogleAuthWebDto
    {
        [Required(ErrorMessage = "El credential de Google es requerido")]
        public string Credential { get; set; } = string.Empty;
    }
}
