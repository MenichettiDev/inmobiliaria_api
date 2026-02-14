namespace inmobiliariaApi.DTOs.TransaccionHistorial
{
    public class TransaccionHistorialComboDto
    {
        public int Id { get; set; }
        public DateTime? FechaOperacion { get; set; }
        public decimal? Precio { get; set; }
        public string? Observaciones { get; set; }
    }
}
