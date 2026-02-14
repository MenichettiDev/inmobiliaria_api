using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.TipoTransaccion
{
    public class UpdateTipoTransaccionDto
    {
        [Required]
        public int Id { get; set; }

        [MaxLength(100)]
        public string? Descripcion { get; set; }
    }
}
