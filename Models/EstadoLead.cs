using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    [Table("estados_lead")]
    public class EstadoLead
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string Nombre { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();
        public virtual ICollection<LeadEstadoHistorial> HistorialEstadosAnteriores { get; set; } = new List<LeadEstadoHistorial>();
        public virtual ICollection<LeadEstadoHistorial> HistorialEstadosNuevos { get; set; } = new List<LeadEstadoHistorial>();
    }
}

// 4	cerrado
// 2	contactado
// 1	nuevo
// 5	perdido
// 3	visito