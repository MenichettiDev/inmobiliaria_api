using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    [Table("leads")]
    public class Lead
    {
        [Key]
        public int Id { get; set; }

        [Column("id_propiedad")]
        public int? IdPropiedad { get; set; }

        [Required]
        [Column("id_inmobiliaria")]
        public int IdInmobiliaria { get; set; }

        [Column("id_usuario_asignado")]
        public int? IdUsuarioAsignado { get; set; }

        [Column("id_cliente")]
        public int? IdCliente { get; set; }

        [Required]
        [Column("nombre_completo")]
        [MaxLength(100)]
        public string NombreCompleto { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Telefono { get; set; }

        [MaxLength(1000)]
        public string? Mensaje { get; set; }

        [Required]
        [Column("id_fuente")]
        public int IdFuente { get; set; }

        [Column("id_estado")]
        public int IdEstado { get; set; } = 1;

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        [Column("actualizado_en")]
        public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("IdPropiedad")]
        public virtual Propiedad? Propiedad { get; set; }

        [ForeignKey("IdInmobiliaria")]
        public virtual Inmobiliaria? Inmobiliaria { get; set; }

        [ForeignKey("IdUsuarioAsignado")]
        public virtual Usuario? UsuarioAsignado { get; set; }

        [ForeignKey("IdCliente")]
        public virtual Cliente? Cliente { get; set; }

        [ForeignKey("IdFuente")]
        public virtual FuenteContacto? Fuente { get; set; }

        [ForeignKey("IdEstado")]
        public virtual EstadoLead? Estado { get; set; }

        public virtual ICollection<LeadEstadoHistorial> HistorialEstados { get; set; } = new List<LeadEstadoHistorial>();
    }
}