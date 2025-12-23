using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    [Table("estados_inmobiliaria")]
    public class EstadoInmobiliaria
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
        public virtual ICollection<Inmobiliaria> Inmobiliarias { get; set; } = new List<Inmobiliaria>();
    }
}

// 1	activa	Inmobiliaria operativa	1
// 2	suspendida	Suspendida por falta de pago o incumplimiento	1
// 3	cancelada	Cuenta cerrada definitivamente	1