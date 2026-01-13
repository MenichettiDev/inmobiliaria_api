namespace inmobiliariaApi.DTOs.Lead
{
    public class LeadDto
    {
        public int Id { get; set; }
        public int? IdPropiedad { get; set; }
        public int IdInmobiliaria { get; set; }
        public int? IdUsuarioAsignado { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? Mensaje { get; set; }
        public int IdFuente { get; set; }
        public int IdEstado { get; set; }
        public bool Activo { get; set; }
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }

        // Información adicional
        public string? PropiedadTitulo { get; set; }
        public string? InmobiliariaNombre { get; set; }
        public string? UsuarioAsignadoNombre { get; set; }
        public string? FuenteNombre { get; set; }
        public string? EstadoNombre { get; set; }
        public string? EstadoAdminDescripcion { get; set; }
    }
}
