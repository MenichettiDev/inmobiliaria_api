using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    [Table("refresh_tokens")]
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Required]
        [Column("id_inmobiliaria")]
        public int IdInmobiliaria { get; set; }

        [Required]
        [MaxLength(512)]
        public string Token { get; set; } = string.Empty;

        [Column("expira_en")]
        public DateTime ExpiraEn { get; set; }

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        [Column("revocado_en")]
        public DateTime? RevocadoEn { get; set; }

        [Column("reemplazado_por")]
        [MaxLength(512)]
        public string? ReemplazadoPor { get; set; }

        [Column("ip_origen")]
        [MaxLength(45)]
        public string? IpOrigen { get; set; }

        // Navigation
        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }

        // Computed (not mapped)
        [NotMapped]
        public bool EsActivo => RevocadoEn == null && ExpiraEn > DateTime.UtcNow;
    }
}
