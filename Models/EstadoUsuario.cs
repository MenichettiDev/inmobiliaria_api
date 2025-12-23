using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
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

// 1	activo	Usuario habilitado	1
// 2	bloqueado	Bloqueado por seguridad o sanción	1
// 3	inactivo	Usuario desvinculado de la inmobiliaria	1