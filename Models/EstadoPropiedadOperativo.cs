using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    [Table("estados_propiedades_operativas")]
    public class EstadoPropiedadOperativo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;

        [Column("color_hex")]
        [MaxLength(7)]
        public string? ColorHex { get; set; } = "#CCCCCC";

        // Navigation properties
        public virtual ICollection<Propiedad> Propiedades { get; set; } = new List<Propiedad>();
    }
}