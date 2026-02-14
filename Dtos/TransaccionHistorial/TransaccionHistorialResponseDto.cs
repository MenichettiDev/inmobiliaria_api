namespace inmobiliariaApi.DTOs.TransaccionHistorial
{
    public class TransaccionHistorialResponseDto
    {
        public int Id { get; set; }
        public int? IdCliente { get; set; }
        public string? ClienteNombre { get; set; }
        public int? IdPropiedad { get; set; }
        public string? PropiedadTitulo { get; set; }
        public int? IdAgente { get; set; }
        public string? AgenteNombre { get; set; }
        public byte? IdTipoTransaccion { get; set; }
        public string? TipoTransaccionDescripcion { get; set; }
        public decimal? Precio { get; set; }
        public DateTime? FechaOperacion { get; set; }
        public string? Observaciones { get; set; }
        public DateTime CreadoEn { get; set; }
    }
}
