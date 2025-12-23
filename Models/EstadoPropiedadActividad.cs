using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    [Table("estados_propiedad_actidad")]
    public class EstadoPropiedadActividad
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        public bool Visible { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Propiedad> Propiedades { get; set; } = new List<Propiedad>();
    }
}