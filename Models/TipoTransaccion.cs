using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    [Table("tipo_transaccion")]
    public class TipoTransaccion
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte Id { get; set; }

        [Column("nombre")]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Column("descripcion")]
        [StringLength(150)]
        public string Descripcion { get; set; } = string.Empty;

        [Column("activo", TypeName = "tinyint(1)")]
        public bool Activo { get; set; }

        // Navegación a historial de transacciones
        public virtual ICollection<TransaccionHistorial> TransaccionesHistorial { get; set; } = new List<TransaccionHistorial>();
    }
}