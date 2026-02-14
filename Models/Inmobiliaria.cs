using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    [Table("inmobiliarias")]
    public class Inmobiliaria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Subdominio { get; set; } = string.Empty;

        [Column("dominio_personalizado")]
        [MaxLength(100)]
        public string? DominioPersonalizado { get; set; }

        [Required]
        [Column("id_plan")]
        public int IdPlan { get; set; }

        [Column("id_estado")]
        public int IdEstado { get; set; } = 1;

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        [Column("actualizado_en")]
        public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("IdPlan")]
        public virtual Plan? Plan { get; set; }

        [ForeignKey("IdEstado")]
        public virtual EstadoInmobiliaria? Estado { get; set; }

        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();
        public virtual ICollection<Propiedad> Propiedades { get; set; } = new List<Propiedad>();

        // Transacciones relacionadas con la inmobiliaria
        public virtual ICollection<TransaccionHistorial> TransaccionesHistorial { get; set; } = new List<TransaccionHistorial>();
    }
}