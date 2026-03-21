using inmobiliariaApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/onboarding")]
    [AllowAnonymous]
    public class OnboardingController : ControllerBase
    {
        private readonly OnboardingService _onboardingService;
        private readonly ILogger<OnboardingController> _logger;

        public OnboardingController(
            OnboardingService onboardingService,
            ILogger<OnboardingController> logger)
        {
            _onboardingService = onboardingService;
            _logger = logger;
        }

        // POST: api/onboarding/register
        // Crea inmobiliaria + usuario admin + trial en una sola llamada.
        // Responde con JWT listo para usar (sin segundo login).
        [HttpPost("register")]
        [EnableRateLimiting("login-ip")] // misma restricción anti-spam que login
        public async Task<IActionResult> Register([FromBody] OnboardingRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { status = 400, message = "Datos inválidos." });

            var result = await _onboardingService.RegisterAsync(request);

            if (!result.Success)
                return BadRequest(new { status = 400, message = result.Message });

            var data = result.Data!;

            _logger.LogInformation(
                "Nuevo tenant registrado: {Nombre} (subdominio={Sub})",
                data.NombreInmobiliaria, data.Subdominio);

            return Ok(new
            {
                status = 201,
                message = result.Message,
                data = new
                {
                    data.IdInmobiliaria,
                    data.IdUsuario,
                    data.NombreInmobiliaria,
                    data.Subdominio,
                    data.TrialHasta,
                    auth = new
                    {
                        token = data.AccessToken,
                        refresh_token = data.RefreshToken,
                        expires_in = data.ExpiresIn
                    }
                }
            });
        }

        // GET: api/onboarding/check-subdomain?subdominio=miinmobiliaria
        // Verifica disponibilidad del subdominio en tiempo real (para el form).
        [HttpGet("check-subdomain")]
        [EnableRateLimiting("api-ip")]
        public async Task<IActionResult> CheckSubdomain([FromQuery] string subdominio)
        {
            if (string.IsNullOrWhiteSpace(subdominio))
                return BadRequest(new { status = 400, message = "El subdominio es obligatorio." });

            var result = await _onboardingService.CheckSubdomainAsync(subdominio);

            return Ok(new
            {
                status = 200,
                disponible = result.Success,
                message = result.Message,
                subdominio = result.Data ?? subdominio.ToLower().Trim()
            });
        }
    }
}
