using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using inmobiliariaApi.DTOs.Common;
using inmobiliariaApi.DTOs.Propiedad;
using inmobiliariaApi.Services;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class PublicController : ControllerBase
    {
        private readonly PropiedadService _propiedadService;
        private readonly ILogger<PublicController> _logger;

        public PublicController(PropiedadService propiedadService, ILogger<PublicController> logger)
        {
            _propiedadService = propiedadService;
            _logger = logger;
        }

        // GET /api/public/filtros — opciones de filtro disponibles
        [HttpGet("filtros")]
        public async Task<IActionResult> GetFiltrosOpciones()
        {
            var response = await _propiedadService.GetPublicFiltrosOpcionesAsync();
            return response.Success ? Ok(response) : StatusCode(500, response);
        }

        // GET /api/public/propiedades — propiedades publicadas cross-tenant
        [HttpGet("propiedades")]
        public async Task<IActionResult> GetPublicPropiedades(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? titulo = null,
            [FromQuery] decimal? precioMin = null,
            [FromQuery] decimal? precioMax = null,
            [FromQuery] int? idInmobiliaria = null,
            [FromQuery] long? idProvincia = null)
        {
            _logger.LogInformation("Request propiedades públicas - página: {Page}", page);

            var response = await _propiedadService.GetPublicadasCrossTenantPagedAsync(
                page, pageSize, titulo, precioMin, precioMax, idInmobiliaria, idProvincia);

            return response.Success ? Ok(response) : NotFound(response);
        }

        // GET /api/public/propiedades/{id} — detalle de una propiedad publicada
        [HttpGet("propiedades/{id}")]
        public async Task<IActionResult> GetPublicPropiedadDetalle(int id)
        {
            _logger.LogInformation("Request detalle propiedad pública - ID: {Id}", id);

            var response = await _propiedadService.GetPublicadaByIdAsync(id);
            return response.Success ? Ok(response) : NotFound(response);
        }

        // GET /api/public/tenant/{subdominio} — propiedades de un tenant específico
        [HttpGet("tenant/{subdominio}")]
        public async Task<IActionResult> GetPublicPropiedadesByTenant(
            string subdominio,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? titulo = null,
            [FromQuery] decimal? precioMin = null,
            [FromQuery] decimal? precioMax = null,
            [FromQuery] int? idInmobiliaria = null,
            [FromQuery] long? idProvincia = null)
        {
            _logger.LogInformation("Request propiedades por tenant - subdominio: {Subdominio}, página: {Page}", subdominio, page);

            var response = await _propiedadService.GetPublicadasBySubdominioPagedAsync(
                subdominio, page, pageSize, titulo, precioMin, precioMax, idInmobiliaria, idProvincia);

            return response.Success ? Ok(response) : NotFound(response);
        }
    }
}
