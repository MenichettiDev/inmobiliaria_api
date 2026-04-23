using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    [Table("estados_propiedad_actidad")]
    public class EstadoPropiedadActividad
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        public bool Visible { get; set; } = true;

    }
}

// 1	activa	Propiedad visible y operativa	1
// 2	eliminada	Propiedad eliminada lógicamente	0