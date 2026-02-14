using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    [Table("transacciones_historial")]
    public class TransaccionHistorial
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("id_cliente")]
        public int? IdCliente { get; set; }

        [Column("id_inmobiliaria")]
        public int? IdInmobiliaria { get; set; }

        [Column("id_propiedad")]
        public int? IdPropiedad { get; set; }

        [Column("id_agente")]
        public int? IdAgente { get; set; }

        [Column("id_tipo_transaccion")]
        public byte? IdTipoTransaccion { get; set; }

        [Column("precio", TypeName = "decimal(12,2)")]
        public decimal? Precio { get; set; }

        [Column("fecha_operacion", TypeName = "date")]
        public DateTime? FechaOperacion { get; set; }

        [Column("observaciones")]
        public string? Observaciones { get; set; }

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; }

        // Navegación a tipo de transacción
        [ForeignKey(nameof(IdTipoTransaccion))]
        public virtual TipoTransaccion? TipoTransaccion { get; set; }

        // Relaciones adicionales
        [ForeignKey(nameof(IdCliente))]
        public virtual Cliente? Cliente { get; set; }

        [ForeignKey(nameof(IdInmobiliaria))]
        public virtual Inmobiliaria? Inmobiliaria { get; set; }

        [ForeignKey(nameof(IdPropiedad))]
        public virtual Propiedad? Propiedad { get; set; }

        [ForeignKey(nameof(IdAgente))]
        public virtual Usuario? Agente { get; set; }
    }
}