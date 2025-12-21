using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pyreApi.Models
{
    [Table("estados_lead_admin")]
    public class EstadoLeadAdmin
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
        public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();
    }
}