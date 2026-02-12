using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    [Table("clientes")]
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Column("id_inmobiliaria")]
        public int IdInmobiliaria { get; set; }

        [Required]
        [Column("nombre_completo")]
        [MaxLength(150)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Column("dni")]
        [MaxLength(20)]
        public string? Dni { get; set; }

        [Column("email")]
        [MaxLength(100)]
        public string? Email { get; set; }

        [Column("telefono")]
        [MaxLength(20)]
        public string? Telefono { get; set; }

        [Column("creado_en")]
        public DateTime? CreadoEn { get; set; } = DateTime.UtcNow;

        [Column("actualizado_en")]
        public DateTime? ActualizadoEn { get; set; } = DateTime.UtcNow;

        [Column("activo")]
        public bool Activo { get; set; } = true;

        // Navigation properties
        [ForeignKey("IdInmobiliaria")]
        public virtual Inmobiliaria? Inmobiliaria { get; set; }

        public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();
    }
}