using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using inmobiliariaApi.Dtos.UsuarioWeb;
using inmobiliariaApi.Models;
using inmobiliariaApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace inmobiliariaApi.Controllers
{
    [Route("api/auth-web")]
    [ApiController]
    public class AuthWebController : ControllerBase
    {
        private readonly UsuarioWebService _usuarioWebService;
        private readonly RefreshTokenService _refreshTokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthWebController> _logger;

        public AuthWebController(
            UsuarioWebService usuarioWebService,
            RefreshTokenService refreshTokenService,
            IConfiguration configuration,
            ILogger<AuthWebController> logger
        )
        {
            _usuarioWebService = usuarioWebService;
            _refreshTokenService = refreshTokenService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Registra un nuevo usuario web
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUsuarioWebDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { status = 400, message = "Datos inválidos", errors = ModelState.Values.SelectMany(v => v.Errors) });
            }

            var result = await _usuarioWebService.RegisterAsync(dto);

            if (!result.Success)
            {
                _logger.LogWarning($"Registro fallido: {result.Message}");
                return BadRequest(new { status = 400, message = result.Message, errors = result.Errors });
            }

            _logger.LogInformation($"Usuario web registrado: {result.Data?.Email}");

            return Ok(new
            {
                status = 201,
                message = "Usuario registrado exitosamente",
                usuario = result.Data
            });
        }

        /// <summary>
        /// Login con email y contraseña
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUsuarioWebDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { status = 400, message = "Datos inválidos" });
            }

            var result = await _usuarioWebService.AuthenticateAsync(dto.Email, dto.Password);

            if (!result.Success)
            {
                _logger.LogWarning($"Login fallido para: {dto.Email}");
                return Unauthorized(new { status = 401, message = result.Message });
            }

            var usuario = result.Data;
            if (usuario == null)
            {
                return Unauthorized(new { status = 401, message = "Usuario no encontrado" });
            }

            _logger.LogInformation($"Usuario web logueado: {usuario.Email}");

            var accessToken = GenerateJwtWebToken(usuario);
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Crear refresh token asociado al usuario web
            var refreshToken = await _refreshTokenService.GenerateWebAsync(usuario.Id, ip);

            return Ok(new
            {
                status = 200,
                message = "Inicio de sesión exitoso",
                token = accessToken,
                refresh_token = refreshToken.Token,
                expires_in = 900,
                usuario = usuario
            });
        }

        /// <summary>
        /// Login con Google OAuth
        /// </summary>
        [HttpPost("google")]
        public async Task<IActionResult> GoogleAuth([FromBody] GoogleAuthWebDto dto)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(dto.Credential))
            {
                return BadRequest(new { status = 400, message = "Credential requerida" });
            }

            var result = await _usuarioWebService.GoogleAuthAsync(dto.Credential);

            if (!result.Success)
            {
                _logger.LogWarning($"Autenticación Google fallida: {result.Message}");
                return Unauthorized(new { status = 401, message = result.Message });
            }

            var usuario = result.Data;
            if (usuario == null)
            {
                return Unauthorized(new { status = 401, message = "Error procesando usuario" });
            }

            _logger.LogInformation($"Usuario autenticado con Google: {usuario.Email}");

            var accessToken = GenerateJwtWebToken(usuario);
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var refreshToken = await _refreshTokenService.GenerateWebAsync(usuario.Id, ip);

            return Ok(new
            {
                status = 200,
                message = "Autenticación con Google exitosa",
                token = accessToken,
                refresh_token = refreshToken.Token,
                expires_in = 900,
                usuario = usuario
            });
        }

        /// <summary>
        /// Refresca el token de acceso
        /// </summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new { status = 400, message = "El refresh_token es obligatorio" });
            }

            var (valid, storedToken, error) = await _refreshTokenService.ValidateAsync(request.RefreshToken);
            if (!valid || storedToken == null)
            {
                return Unauthorized(new { status = 401, message = error });
            }

            // Obtener el usuario web
            var usuarioId = storedToken.IdUsuario;
            var usuarioWebResponse = await _usuarioWebService.GetByIdAsync(usuarioId);

            if (!usuarioWebResponse.Success || usuarioWebResponse.Data == null)
            {
                return Unauthorized(new { status = 401, message = "Usuario no encontrado" });
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var newRefreshToken = await _refreshTokenService.RotateAsync(storedToken, ip);
            var newAccessToken = GenerateJwtWebToken(usuarioWebResponse.Data);

            _logger.LogInformation($"Token renovado para usuario web {usuarioWebResponse.Data.Email}");

            return Ok(new
            {
                status = 200,
                message = "Token renovado correctamente",
                token = newAccessToken,
                refresh_token = newRefreshToken.Token,
                expires_in = 900
            });
        }

        /// <summary>
        /// Obtiene el perfil del usuario web autenticado
        /// </summary>
        [HttpGet("me")]
        [Authorize(Policy = "WebUser")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out int userId))
                {
                    return Unauthorized(new { status = 401, message = "Usuario no autenticado" });
                }

                var result = await _usuarioWebService.GetByIdAsync(userId);

                if (!result.Success)
                {
                    return NotFound(new { status = 404, message = "Usuario no encontrado" });
                }

                return Ok(new
                {
                    status = 200,
                    message = "Perfil obtenido",
                    usuario = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener perfil");
                return StatusCode(500, new { status = 500, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Genera un JWT para usuario web
        /// </summary>
        private string GenerateJwtWebToken(UsuarioWebDto usuario)
        {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? ""));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim("UserType", "web")  // Claim que diferencia usuarios web de usuarios internos
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
