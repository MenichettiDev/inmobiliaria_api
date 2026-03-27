using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    [Table("usuarios_web")]
    public class UsuarioWeb
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

        [Column("hash_contrasena")]
        [MaxLength(255)]
        public string? HashContrasena { get; set; }

        [Column("google_id")]
        [MaxLength(255)]
        public string? GoogleId { get; set; }

        [Required]
        [Column("proveedor_auth")]
        [MaxLength(50)]
        public string ProveedorAuth { get; set; } = "local";

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("email_verificado")]
        public bool EmailVerificado { get; set; } = false;

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        [Column("actualizado_en")]
        public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<PropiedadFavorita> Favoritos { get; set; } = new List<PropiedadFavorita>();
        public virtual ICollection<Lead> LeadsEnviados { get; set; } = new List<Lead>();
    }
}
