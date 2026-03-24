using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using inmobiliariaApi.Services;
using inmobiliariaApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

namespace inmobiliariaApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;
        private readonly RefreshTokenService _refreshTokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UsuarioService usuarioService,
            RefreshTokenService refreshTokenService,
            IConfiguration configuration,
            ILogger<AuthController> logger
        )
        {
            _usuarioService = usuarioService;
            _refreshTokenService = refreshTokenService;
            _configuration = configuration;
            _logger = logger;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        [EnableRateLimiting("login-ip")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                _logger.LogWarning(
                    "Intento de login con campos vacíos. Email: '{Email}', Password vacío: {PasswordEmpty}",
                    request.Email ?? "(nulo)",
                    string.IsNullOrEmpty(request.Password)
                );
                return BadRequest(new
                {
                    status = 400,
                    error = "Bad Request",
                    message = "El email y la contraseña son obligatorios.",
                });
            }

            var authResponse = await _usuarioService.AuthenticateAsync(request.Email, request.Password);
            if (!authResponse.Success)
            {
                _logger.LogWarning("Login fallido para el email: {Email}. Error: {Error}",
                    request.Email, authResponse.Message);
                return Unauthorized(new
                {
                    status = 401,
                    error = "Unauthorized",
                    message = "Email o contraseña incorrectos.",
                });
            }

            var usuario = authResponse.Data;
            if (usuario == null)
            {
                _logger.LogError("El objeto usuario es null después de la autenticación.");
                return BadRequest(new { Message = "El usuario no existe." });
            }

            // Validar subdominio si está presente (multi-tenant validation)
            var subdomain = Request.Headers["X-Subdomain"].ToString();
            if (!string.IsNullOrEmpty(subdomain))
            {
                _logger.LogInformation("Login desde subdominio: {Subdomain} para usuario: {Email}", subdomain, usuario.Email);
                // El usuario debe pertenecer a la inmobiliaria del subdominio
                // Por ahora solo registramos, en producción validaríamos más estrictamente
            }

            _logger.LogInformation("Usuario logueado exitosamente: {Email}", usuario.Email);

            var accessToken = GenerateJwtToken(usuario);
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var refreshToken = await _refreshTokenService.GenerateAsync(usuario.Id, usuario.IdInmobiliaria, ip);

            return Ok(new
            {
                status = 200,
                message = "Inicio de sesión exitoso.",
                token = accessToken,
                refresh_token = refreshToken.Token,
                expires_in = 900, // 15 minutos en segundos
                subdomain = subdomain, // Devolver el subdominio para confirmación
                usuario = new
                {
                    usuario.Id,
                    usuario.IdInmobiliaria,
                    usuario.Nombre,
                    usuario.Email,
                    usuario.IdRol,
                    RolNombre = usuario.Rol?.Nombre,
                    usuario.IdEstado,
                },
            });
        }

        // POST: api/auth/refresh
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new { status = 400, message = "El refresh_token es obligatorio." });
            }

            var (valid, storedToken, error) = await _refreshTokenService.ValidateAsync(request.RefreshToken);
            if (!valid || storedToken == null)
            {
                return Unauthorized(new { status = 401, message = error });
            }

            var usuario = storedToken.Usuario;
            if (usuario == null)
            {
                return Unauthorized(new { status = 401, message = "Usuario no encontrado." });
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var newRefreshToken = await _refreshTokenService.RotateAsync(storedToken, ip);
            var newAccessToken = GenerateJwtToken(usuario);

            _logger.LogInformation("Access token renovado para usuario {Email}", usuario.Email);

            return Ok(new
            {
                status = 200,
                message = "Token renovado correctamente.",
                token = newAccessToken,
                refresh_token = newRefreshToken.Token,
                expires_in = 900,
            });
        }

        // POST: api/auth/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
        {
            if (!string.IsNullOrEmpty(request.RefreshToken))
            {
                await _refreshTokenService.RevokeAsync(request.RefreshToken);
            }

            _logger.LogInformation("Logout ejecutado.");
            return Ok(new { status = 200, message = "Sesión cerrada correctamente." });
        }

        // POST: api/auth/logout-all  (revoca todos los tokens del usuario)
        [HttpPost("logout-all")]
        [Authorize]
        public async Task<IActionResult> LogoutAll()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            await _refreshTokenService.RevokeAllByUsuarioAsync(userId);
            _logger.LogInformation("Todos los tokens revocados para usuario {UserId}", userId);
            return Ok(new { status = 200, message = "Todas las sesiones cerradas correctamente." });
        }

        // Genera JWT con expiración de 15 minutos
        private string GenerateJwtToken(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(
                _configuration["Jwt:Key"]
                    ?? throw new ArgumentNullException("Jwt:Key no está definido")
            );

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email ?? ""),
                new Claim(ClaimTypes.Name, usuario.Nombre ?? ""),
                new Claim(ClaimTypes.Role, usuario.Rol?.Nombre ?? "Usuario"),
                new Claim("IdInmobiliaria", usuario.IdInmobiliaria.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
