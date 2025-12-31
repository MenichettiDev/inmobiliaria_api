using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    [Table("busquedas_guardadas")]
    public class BusquedasGuardadas
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("filtros_json")]
        public string FiltrosJson { get; set; } = string.Empty;

        [Column("ultimo_envio")]
        public DateTime? UltimoEnvio { get; set; }

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        [Column("actualizado_en")]
        public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("id_inmobiliaria")]
        public int IdInmobiliaria { get; set; }

        // Navigation properties
        [ForeignKey("IdInmobiliaria")]
        public virtual Inmobiliaria? Inmobiliaria { get; set; }
    }
}
