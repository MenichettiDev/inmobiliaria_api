namespace pyreApi.DTOs.Lead
{
    public class LeadResponseDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? Mensaje { get; set; }
        public string? DireccionInteres { get; set; }
        public decimal? PresupuestoMinimo { get; set; }
        public decimal? PresupuestoMaximo { get; set; }
        public string? TipoOperacionInteres { get; set; }
        public DateTime FechaContacto { get; set; }
        public DateTime? FechaUltimaInteraccion { get; set; }
        public string? Notas { get; set; }
        public int? Puntuacion { get; set; }

        // Información de la propiedad relacionada
        public int? IdPropiedad { get; set; }
        public string? PropiedadTitulo { get; set; }
        public string? PropiedadDireccion { get; set; }

        // Información del estado
        public int IdEstado { get; set; }
        public string EstadoNombre { get; set; } = string.Empty;
        public string? EstadoColor { get; set; }

        // Información de la fuente
        public int IdFuenteContacto { get; set; }
        public string FuenteNombre { get; set; } = string.Empty;

        // Usuario asignado
        public int? IdUsuarioAsignado { get; set; }
        public string? UsuarioAsignadoNombre { get; set; }
        public string? UsuarioAsignadoEmail { get; set; }

        // Auditoría
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioCreaNombre { get; set; }
        public string? UsuarioModificaNombre { get; set; }
    }
}