namespace inmobiliariaApi.DTOs.Cliente
{
    // ...new file...
    public class ClienteResponseDto
    {
        public int Id { get; set; }
        public int IdInmobiliaria { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Dni { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public bool Activo { get; set; }
        public DateTime? CreadoEn { get; set; }
        public DateTime? ActualizadoEn { get; set; }
    }
}
