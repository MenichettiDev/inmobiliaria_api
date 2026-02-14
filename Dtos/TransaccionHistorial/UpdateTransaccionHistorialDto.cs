using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.DTOs.TransaccionHistorial
{
    public class UpdateTransaccionHistorialDto
    {
        [Required]
        public int Id { get; set; }
        public int? IdCliente { get; set; }
        public int? IdPropiedad { get; set; }
        public int? IdAgente { get; set; }
        public byte? IdTipoTransaccion { get; set; }
        public decimal? Precio { get; set; }
        public DateTime? FechaOperacion { get; set; }
        [MaxLength(1000)]
        public string? Observaciones { get; set; }

        // Forzar tenant desde controller
        public int? IdInmobiliaria { get; set; }
    }
}
