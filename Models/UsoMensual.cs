using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    [Table("uso_mensual")]
    public class UsoMensual
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("id_inmobiliaria")]
        public int IdInmobiliaria { get; set; }

        [Required]
        [MaxLength(7)] // Formato YYYY-MM
        public string Mes { get; set; } = string.Empty;

        [Column("leads_generados")]
        public int LeadsGenerados { get; set; } = 0;

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        [Column("actualizado_en")]
        public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("IdInmobiliaria")]
        public virtual Inmobiliaria? Inmobiliaria { get; set; }
    }
}
