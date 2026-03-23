namespace inmobiliariaApi.DTOs.Localidad
{
    public class LocalidadDto
    {
        public long Id { get; set; }
        public long IdProvincia { get; set; }
        public string ProvinciaNombre { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? CodigoPostal { get; set; }
        public string? Latitud { get; set; }
        public string? Longitud { get; set; }
        public string? Municipio { get; set; }
        public long? IdPartido { get; set; }
    }

    public class CreateLocalidadDto
    {
        public long IdProvincia { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? CodigoPostal { get; set; }
        public string? Latitud { get; set; }
        public string? Longitud { get; set; }
        public string? Municipio { get; set; }
        public long? IdPartido { get; set; }
    }

    public class UpdateLocalidadDto
    {
        public long Id { get; set; }
        public long? IdProvincia { get; set; }
        public string? Nombre { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Latitud { get; set; }
        public string? Longitud { get; set; }
        public string? Municipio { get; set; }
        public long? IdPartido { get; set; }
    }
}
