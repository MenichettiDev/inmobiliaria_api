using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    [Table("propiedades")]
    public class Propiedad
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("id_inmobiliaria")]
        public int IdInmobiliaria { get; set; }

        [Column("id_agente_responsable")]
        public int? IdAgenteResponsable { get; set; }

        [Required]
        [MaxLength(150)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Descripcion { get; set; } = string.Empty;

        [Column(TypeName = "decimal(15,2)")]
        public decimal? Precio { get; set; }

        [Required]
        [MaxLength(200)]
        public string Direccion { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,8)")]
        public decimal? Latitud { get; set; }

        [Column(TypeName = "decimal(10,8)")]
        public decimal? Longitud { get; set; }

        [Column("publicada_en")]
        public DateTime? PublicadaEn { get; set; }

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        [Column("actualizado_en")]
        public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

        [Column("id_estado_admin")]
        public int IdEstadoAdmin { get; set; } = 1;

        [Column("id_estado_operativo")]
        public int IdEstadoOperativo { get; set; } = 1;

        [Column("id_localidad")]
        public long? IdLocalidad { get; set; }

        // Navigation properties
        [ForeignKey("IdLocalidad")]
        public virtual Localidad? Localidad { get; set; }
        [ForeignKey("IdInmobiliaria")]
        public virtual Inmobiliaria? Inmobiliaria { get; set; }

        [ForeignKey("IdAgenteResponsable")]
        public virtual Usuario? AgenteResponsable { get; set; }

        [ForeignKey("IdEstadoAdmin")]
        public virtual EstadoPropiedadActividad? EstadoAdmin { get; set; }

        [ForeignKey("IdEstadoOperativo")]
        public virtual EstadoPropiedadOperativo? EstadoOperativo { get; set; }

        public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();
        public virtual ICollection<ImagenPropiedad> Imagenes { get; set; } = new List<ImagenPropiedad>();

        // Transacciones relacionadas con la propiedad
        public virtual ICollection<TransaccionHistorial> TransaccionesHistorial { get; set; } = new List<TransaccionHistorial>();
    }
}