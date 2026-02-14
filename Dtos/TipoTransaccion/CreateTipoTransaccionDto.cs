using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.TipoTransaccion
{
    public class CreateTipoTransaccionDto
    {
        [Required]
        [MaxLength(100)]
        public string Descripcion { get; set; } = string.Empty;
    }
}
