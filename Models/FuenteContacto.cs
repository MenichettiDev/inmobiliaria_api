using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    [Table("fuentes_contacto")]
    public class FuenteContacto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string Nombre { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();
    }
}