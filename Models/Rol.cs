using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace inmobiliariaApi.Models
{
    [Table("roles")]
    public class Rol
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string Nombre { get; set; } = string.Empty;

        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
// 1	Programador
// 2	Administrador
// 3	Supervisor
// 4	Agente
// 6	Asistente