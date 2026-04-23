namespace inmobiliariaApi.DTOs.Propiedad
{
    public class FiltroOpcionDto
    {
        public long Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    public class PublicFiltrosDto
    {
        public List<FiltroOpcionDto> Inmobiliarias { get; set; } = new();
        public List<FiltroOpcionDto> Provincias { get; set; } = new();
    }
}
