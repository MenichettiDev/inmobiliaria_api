using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    [Table("estados_suscripcion")]
    public class EstadoSuscripcion
    {
        [Key]
        public byte Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Descripcion { get; set; } = string.Empty;

        [Column("permite_operar")]
        public bool PermiteOperar { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Suscripcion> Suscripciones { get; set; } = new List<Suscripcion>();
    }
}

// Estados predefinidos:
// 1	activa	Suscripción activa	1
// 2	pausada	Pausada temporalmente	0
// 3	vencida	Vencida por falta de pago	0
// 4	cancelada	Cancelada definitivamente	0