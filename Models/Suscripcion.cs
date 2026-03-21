using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    [Table("suscripciones")]
    public class Suscripcion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("id_inmobiliaria")]
        public int IdInmobiliaria { get; set; }

        [Required]
        [Column("id_plan")]
        public int IdPlan { get; set; }

        [Required]
        [Column("inicio")]
        public DateTime Inicio { get; set; }

        [Required]
        [Column("fin")]
        public DateTime Fin { get; set; }

        [Column("renovacion_automatica")]
        public bool RenovacionAutomatica { get; set; } = true;

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        [Column("actualizado_en")]
        public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("id_estado")]
        public byte IdEstado { get; set; } = 1; // Por defecto activa

        // Navigation properties
        [ForeignKey("IdInmobiliaria")]
        public virtual Inmobiliaria? Inmobiliaria { get; set; }

        [ForeignKey("IdEstado")]
        public virtual EstadoSuscripcion? Estado { get; set; }

        [ForeignKey("IdPlan")]
        public virtual Plan? Plan { get; set; }
    }
}