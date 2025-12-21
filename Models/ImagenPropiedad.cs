using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pyreApi.Models
{
    [Table("imagenes_propiedades")]
    public class ImagenPropiedad
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("id_propiedad")]
        public int IdPropiedad { get; set; }

        [Required]
        [MaxLength(255)]
        public string Url { get; set; } = string.Empty;

        public int Orden { get; set; } = 0;

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("IdPropiedad")]
        public virtual Propiedad? Propiedad { get; set; }
    }
}