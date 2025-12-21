using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pyreApi.Models
{
    [Table("usuarios")]
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("hash_contrasena")]
        [MaxLength(255)]
        public string HashContrasena { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Telefono { get; set; }

        [Required]
        [Column("id_rol")]
        public int IdRol { get; set; }

        [Required]
        [Column("id_inmobiliaria")]
        public int IdInmobiliaria { get; set; }

        [Column("id_estado")]
        public int IdEstado { get; set; } = 1;

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        [Column("actualizado_en")]
        public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("IdRol")]
        public virtual Rol? Rol { get; set; }

        [ForeignKey("IdInmobiliaria")]
        public virtual Inmobiliaria? Inmobiliaria { get; set; }

        [ForeignKey("IdEstado")]
        public virtual EstadoUsuario? Estado { get; set; }

        public virtual ICollection<Lead> LeadsAsignados { get; set; } = new List<Lead>();
        public virtual ICollection<Propiedad> PropiedadesResponsable { get; set; } = new List<Propiedad>();
        public virtual ICollection<LeadEstadoHistorial> CambiosEstadoLead { get; set; } = new List<LeadEstadoHistorial>();
    }
}
