using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.Usuario
{
    public class UsuarioComboDto
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        public int IdRol { get; set; }

        public string RolNombre { get; set; } = string.Empty;

        public int IdEstado { get; set; }

        public string EstadoNombre { get; set; } = string.Empty;
    }
}
