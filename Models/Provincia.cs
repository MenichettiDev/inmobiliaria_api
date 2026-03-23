using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    [Table("provincias")]
    public class Provincia
    {
        [Key]
        [Column("id_provincia")]
        public long Id { get; set; }

        [Required]
        [MaxLength(45)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("codigo_indec")]
        public string? CodigoIndec { get; set; }

        public bool Activo { get; set; } = true;

        // Navigation
        public virtual ICollection<Localidad> Localidades { get; set; } = new List<Localidad>();
        public virtual ICollection<Inmobiliaria> Inmobiliarias { get; set; } = new List<Inmobiliaria>();
    }
}
