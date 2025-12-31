namespace inmobiliariaApi.DTOs.LeadEstadoHistorial
{
    public class LeadEstadoHistorialDto
    {
        public int Id { get; set; }
        public int IdLead { get; set; }
        public int IdEstadoAnterior { get; set; }
        public int IdEstadoNuevo { get; set; }
        public int IdUsuario { get; set; }
        public string? Comentario { get; set; }
        public DateTime CreadoEn { get; set; }

        // Información adicional para la respuesta
        public string? EstadoAnteriorNombre { get; set; }
        public string? EstadoNuevoNombre { get; set; }
        public string? UsuarioNombre { get; set; }
        public string? LeadNombre { get; set; }
    }
}
