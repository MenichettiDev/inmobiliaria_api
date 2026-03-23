namespace inmobiliariaApi.DTOs.Provincia
{
    public class ProvinciaDto
    {
        public long Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? CodigoIndec { get; set; }
        public bool Activo { get; set; }
    }

    public class CreateProvinciaDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? CodigoIndec { get; set; }
    }

    public class UpdateProvinciaDto
    {
        public long Id { get; set; }
        public string? Nombre { get; set; }
        public string? CodigoIndec { get; set; }
        public bool? Activo { get; set; }
    }
}
