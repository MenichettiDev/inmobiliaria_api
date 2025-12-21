namespace pyreApi.DTOs.Lead
{
    public class LeadFiltrosDto
    {
        public string? NombreCompleto { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public int? IdEstado { get; set; }
        public int? IdFuenteContacto { get; set; }
        public int? IdUsuarioAsignado { get; set; }
        public int? IdPropiedad { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int? PuntuacionMinima { get; set; }
        public int? PuntuacionMaxima { get; set; }
        public string? TipoOperacion { get; set; }
        public decimal? PresupuestoMinimo { get; set; }
        public decimal? PresupuestoMaximo { get; set; }

        // Parámetros de paginación
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Ordenamiento
        public string? OrderBy { get; set; } = "FechaCreacion";
        public bool OrderDescending { get; set; } = true;
    }
}