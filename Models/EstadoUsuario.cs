using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pyreApi.Models
{
    [Table("estados_usuario")]
    public class EstadoUsuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Descripcion { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}