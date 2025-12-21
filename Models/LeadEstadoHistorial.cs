using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pyreApi.Models
{
    [Table("lead_estados_historial")]
    public class LeadEstadoHistorial
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("id_lead")]
        public int IdLead { get; set; }

        [Column("id_estado_anterior")]
        public int IdEstadoAnterior { get; set; }

        [Required]
        [Column("id_estado_nuevo")]
        public int IdEstadoNuevo { get; set; }

        [Required]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [MaxLength(1000)]
        public string? Comentario { get; set; }

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("IdLead")]
        public virtual Lead? Lead { get; set; }

        [ForeignKey("IdEstadoAnterior")]
        public virtual EstadoLead? EstadoAnterior { get; set; }

        [ForeignKey("IdEstadoNuevo")]
        public virtual EstadoLead? EstadoNuevo { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }
    }
}