using System.Security.Claims;
using inmobiliariaApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace inmobiliariaApi.Controllers
{
    [Route("api/portal/usuario")]
    [ApiController]
    [Authorize(Policy = "WebUser")]
    public class PortalUsuarioController : ControllerBase
    {
        private readonly UsuarioWebService _usuarioWebService;
        private readonly ILogger<PortalUsuarioController> _logger;

        public PortalUsuarioController(
            UsuarioWebService usuarioWebService,
            ILogger<PortalUsuarioController> logger
        )
        {
            _usuarioWebService = usuarioWebService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene el ID del usuario web actual desde el JWT
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdString, out int userId) ? userId : 0;
        }

        /// <summary>
        /// Obtiene los favoritos del usuario autenticado
        /// </summary>
        [HttpGet("favoritos")]
        public async Task<IActionResult> GetFavoritos()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(new { status = 401, message = "Usuario no autenticado" });
                }

                var result = await _usuarioWebService.GetFavoritosAsync(userId);

                if (!result.Success)
                {
                    return StatusCode(500, new { status = 500, message = result.Message });
                }

                return Ok(new
                {
                    status = 200,
                    message = "Favoritos obtenidos",
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener favoritos");
                return StatusCode(500, new { status = 500, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Agrega o remueve un favorito (toggle)
        /// </summary>
        [HttpPost("favoritos/{propiedadId}")]
        public async Task<IActionResult> ToggleFavorito(int propiedadId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(new { status = 401, message = "Usuario no autenticado" });
                }

                if (propiedadId <= 0)
                {
                    return BadRequest(new { status = 400, message = "ID de propiedad inválido" });
                }

                var result = await _usuarioWebService.ToggleFavoritoAsync(userId, propiedadId);

                if (!result.Success)
                {
                    return StatusCode(500, new { status = 500, message = result.Message });
                }

                var message = result.Data ? "Favorito agregado" : "Favorito removido";

                return Ok(new
                {
                    status = 200,
                    message = message,
                    isFavorito = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al toggle de favorito");
                return StatusCode(500, new { status = 500, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Remueve un favorito específico
        /// </summary>
        [HttpDelete("favoritos/{propiedadId}")]
        public async Task<IActionResult> RemoveFavorito(int propiedadId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(new { status = 401, message = "Usuario no autenticado" });
                }

                if (propiedadId <= 0)
                {
                    return BadRequest(new { status = 400, message = "ID de propiedad inválido" });
                }

                var result = await _usuarioWebService.ToggleFavoritoAsync(userId, propiedadId);

                if (!result.Success)
                {
                    return StatusCode(500, new { status = 500, message = result.Message });
                }

                return Ok(new
                {
                    status = 200,
                    message = "Favorito removido"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al remover favorito");
                return StatusCode(500, new { status = 500, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene las consultas (leads) enviadas por el usuario
        /// </summary>
        [HttpGet("consultas")]
        public async Task<IActionResult> GetConsultas()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(new { status = 401, message = "Usuario no autenticado" });
                }

                var result = await _usuarioWebService.GetConsultasAsync(userId);

                if (!result.Success)
                {
                    return StatusCode(500, new { status = 500, message = result.Message });
                }

                return Ok(new
                {
                    status = 200,
                    message = "Consultas obtenidas",
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener consultas");
                return StatusCode(500, new { status = 500, message = "Error interno del servidor" });
            }
        }
    }
}
