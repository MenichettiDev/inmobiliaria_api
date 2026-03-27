using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    [Table("propiedades_favoritas")]
    public class PropiedadFavorita
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("id_usuario_web")]
        public int IdUsuarioWeb { get; set; }

        [Required]
        [Column("id_propiedad")]
        public int IdPropiedad { get; set; }

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("IdUsuarioWeb")]
        public virtual UsuarioWeb? UsuarioWeb { get; set; }

        [ForeignKey("IdPropiedad")]
        public virtual Propiedad? Propiedad { get; set; }
    }
}
