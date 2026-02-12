namespace inmobiliariaApi.DTOs.Lead
{
    public class LeadResponseDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? Mensaje { get; set; }

        // Información de la propiedad relacionada
        public int? IdPropiedad { get; set; }
        public string? PropiedadTitulo { get; set; }
        public string? PropiedadDireccion { get; set; }

        // Información del estado
        public int IdEstado { get; set; }
        public string EstadoNombre { get; set; } = string.Empty;

        // Información de la fuente
        public int IdFuente { get; set; }
        public string FuenteNombre { get; set; } = string.Empty;

        // Usuario asignado
        public int? IdUsuarioAsignado { get; set; }
        public string? UsuarioAsignadoNombre { get; set; }
        public string? UsuarioAsignadoEmail { get; set; }

        // Cliente
        public int? IdCliente { get; set; }
        public string? ClienteNombre { get; set; }
        public string? ClienteEmail { get; set; }

        // Estado administrativo
        public bool Activo { get; set; }
        public string EstadoAdminDescripcion { get; set; } = string.Empty;

        // Auditoría
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }
    }
}