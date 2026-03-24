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

        // GET /api/public/propiedades - Propiedades publicadas cross-tenant
        [HttpGet("propiedades")]
        public async Task<IActionResult> GetPublicPropiedades(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? titulo = null,
            [FromQuery] decimal? precioMin = null,
            [FromQuery] decimal? precioMax = null)
        {
            _logger.LogInformation("Request de propiedades públicas - página: {Page}, tamaño: {PageSize}", page, pageSize);

            var response = await _propiedadService.GetPublicadasCrossTenantPagedAsync(
                page, pageSize, titulo, precioMin, precioMax);

            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        // GET /api/public/propiedades/{id} - Detalle de una propiedad publicada
        [HttpGet("propiedades/{id}")]
        public async Task<IActionResult> GetPublicPropiedadDetalle(int id)
        {
            _logger.LogInformation("Request de detalle de propiedad pública - ID: {Id}", id);

            var response = await _propiedadService.GetPublicadaByIdAsync(id);

            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        // GET /api/public/tenant/{subdominio} - Propiedades publicadas de un tenant específico
        [HttpGet("tenant/{subdominio}")]
        public async Task<IActionResult> GetPublicPropiedadesByTenant(
            string subdominio,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? titulo = null,
            [FromQuery] decimal? precioMin = null,
            [FromQuery] decimal? precioMax = null)
        {
            _logger.LogInformation("Request de propiedades públicas por tenant - subdominio: {Subdominio}, página: {Page}", subdominio, page);

            var response = await _propiedadService.GetPublicadasBySubdominioPagedAsync(
                subdominio, page, pageSize, titulo, precioMin, precioMax);

            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }
    }
}
