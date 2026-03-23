using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    [Table("localidades")]
    public class Localidad
    {
        [Key]
        [Column("id_localidad")]
        public long Id { get; set; }

        [Required]
        [Column("id_provincia")]
        public long IdProvincia { get; set; }

        [Required]
        [MaxLength(45)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(10)]
        [Column("codigo_postal")]
        public string? CodigoPostal { get; set; }

        [MaxLength(100)]
        public string? Latitud { get; set; }

        [MaxLength(100)]
        public string? Longitud { get; set; }

        [MaxLength(100)]
        public string? Municipio { get; set; }

        [Column("id_partido")]
        public long? IdPartido { get; set; }

        // Navigation
        [ForeignKey("IdProvincia")]
        public virtual Provincia? Provincia { get; set; }

        public virtual ICollection<Propiedad> Propiedades { get; set; } = new List<Propiedad>();
    }
}
