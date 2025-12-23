using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    [Table("planes")]
    public class Plan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [Column("precio_usd", TypeName = "decimal(10,2)")]
        public decimal PrecioUsd { get; set; }

        [Column("max_propiedades")]
        public int? MaxPropiedades { get; set; }

        [Column("max_usuarios")]
        public int? MaxUsuarios { get; set; }

        [Column("max_leads_mes")]
        public int? MaxLeadsMes { get; set; }

        [Column("white_label")]
        public bool WhiteLabel { get; set; } = false;

        [Column("dominio_personalizado")]
        public bool DominioPersonalizado { get; set; } = false;

        public bool Automatizaciones { get; set; } = false;

        [Column("api_acceso")]
        public bool ApiAcceso { get; set; } = false;

        [MaxLength(50)]
        public string Soporte { get; set; } = "email";

        public bool Activo { get; set; } = true;

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        [Column("actualizado_en")]
        public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

        [Column("historial_estados")]
        public bool HistorialEstados { get; set; } = false;

        [Column("actividades_lead")]
        public bool ActividadesLead { get; set; } = false;

        [Column("tipos_actividad")]
        [MaxLength(500)]
        public string? TiposActividad { get; set; }

        [Column("automatizacion_leads")]
        public bool AutomatizacionLeads { get; set; } = false;

        [Column("lead_scoring")]
        public bool LeadScoring { get; set; } = false;

        // Navigation properties
        public virtual ICollection<Inmobiliaria> Inmobiliarias { get; set; } = new List<Inmobiliaria>();
    }
}