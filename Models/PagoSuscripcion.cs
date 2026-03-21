using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    public enum EstadoPago
    {
        Pendiente,
        Aprobado,
        Rechazado,
        Cancelado,
        Reembolsado
    }

    [Table("pagos_suscripcion")]
    public class PagoSuscripcion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("id_inmobiliaria")]
        public int IdInmobiliaria { get; set; }

        [Required]
        [Column("id_plan")]
        public int IdPlan { get; set; }

        // Se llena después de que el pago es confirmado
        [Column("id_suscripcion")]
        public int? IdSuscripcion { get; set; }

        // ID de la preferencia de checkout generada en MP
        [Required]
        [Column("mp_preference_id")]
        [MaxLength(100)]
        public string MpPreferenceId { get; set; } = string.Empty;

        // ID del pago real (llega en el webhook)
        [Column("mp_payment_id")]
        public long? MpPaymentId { get; set; }

        [Column("estado")]
        public EstadoPago Estado { get; set; } = EstadoPago.Pendiente;

        [Column("monto", TypeName = "decimal(10,2)")]
        public decimal Monto { get; set; }

        [Column("moneda")]
        [MaxLength(3)]
        public string Moneda { get; set; } = "ARS";

        // Duración del plan comprado (días)
        [Column("dias_plan")]
        public int DiasPlan { get; set; } = 30;

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        [Column("actualizado_en")]
        public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("IdInmobiliaria")]
        public virtual Inmobiliaria? Inmobiliaria { get; set; }

        [ForeignKey("IdPlan")]
        public virtual Plan? Plan { get; set; }

        [ForeignKey("IdSuscripcion")]
        public virtual Suscripcion? Suscripcion { get; set; }
    }
}
