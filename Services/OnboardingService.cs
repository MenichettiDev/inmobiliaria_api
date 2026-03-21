using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using inmobiliariaApi.Data;
using inmobiliariaApi.DTOs.Common;
using inmobiliariaApi.Models;

namespace inmobiliariaApi.Services
{
    public class OnboardingRequest
    {
        public string NombreInmobiliaria { get; set; } = string.Empty;
        public string Subdominio { get; set; } = string.Empty;
        public string NombreAdmin { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Telefono { get; set; }
    }

    public class OnboardingResult
    {
        public int IdInmobiliaria { get; set; }
        public int IdUsuario { get; set; }
        public string NombreInmobiliaria { get; set; } = string.Empty;
        public string Subdominio { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; } = 900;
        public DateTime TrialHasta { get; set; }
    }

    public class OnboardingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OnboardingService> _logger;

        private const int TrialDays = 14;
        private const string AdminRolNombre = "Administrador";
        private static readonly Regex SubdomainRegex = new(@"^[a-z0-9][a-z0-9\-]{1,28}[a-z0-9]$", RegexOptions.Compiled);

        public OnboardingService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<OnboardingService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<BaseResponseDto<string>> CheckSubdomainAsync(string subdominio)
        {
            var normalized = subdominio.ToLower().Trim();

            if (!SubdomainRegex.IsMatch(normalized))
                return Fail<string>("El subdominio solo puede contener letras minúsculas, números y guiones (mín. 3, máx. 30 caracteres).");

            var reserved = new[] { "www", "api", "admin", "app", "mail", "ftp", "test", "dev", "staging" };
            if (reserved.Contains(normalized))
                return Fail<string>("Ese subdominio está reservado. Elige otro.");

            var exists = await _context.Inmobiliaria
                .AnyAsync(i => i.Subdominio == normalized);

            if (exists)
                return Fail<string>("El subdominio ya está en uso.");

            return Ok<string>(normalized, "Subdominio disponible.");
        }

        public async Task<BaseResponseDto<OnboardingResult>> RegisterAsync(OnboardingRequest request)
        {
            // ── Validaciones previas ──────────────────────────────────────────

            var subdominio = request.Subdominio.ToLower().Trim();

            if (string.IsNullOrWhiteSpace(request.NombreInmobiliaria) ||
                request.NombreInmobiliaria.Length < 2 || request.NombreInmobiliaria.Length > 100)
                return Fail<OnboardingResult>("El nombre de la inmobiliaria debe tener entre 2 y 100 caracteres.");

            var subdCheck = await CheckSubdomainAsync(subdominio);
            if (!subdCheck.Success)
                return Fail<OnboardingResult>(subdCheck.Message);

            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
                return Fail<OnboardingResult>("Email inválido.");

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
                return Fail<OnboardingResult>("La contraseña debe tener al menos 8 caracteres.");

            if (string.IsNullOrWhiteSpace(request.NombreAdmin) || request.NombreAdmin.Length < 2)
                return Fail<OnboardingResult>("El nombre del administrador debe tener al menos 2 caracteres.");

            var emailExists = await _context.Usuario
                .AnyAsync(u => u.Email == request.Email.ToLower().Trim());

            if (emailExists)
                return Fail<OnboardingResult>("El email ya está registrado.");

            // ── Obtener dependencias ──────────────────────────────────────────

            var rolAdmin = await _context.Rol
                .FirstOrDefaultAsync(r => r.Nombre == AdminRolNombre);

            if (rolAdmin == null)
                return Fail<OnboardingResult>($"El rol '{AdminRolNombre}' no está configurado en el sistema.");

            // Plan trial: precio 0, o en su defecto el primer plan activo
            var planTrial = await _context.Plan
                .Where(p => p.Activo && p.PrecioUsd == 0)
                .FirstOrDefaultAsync()
                ?? await _context.Plan
                    .Where(p => p.Activo)
                    .OrderBy(p => p.PrecioUsd)
                    .FirstOrDefaultAsync();

            if (planTrial == null)
                return Fail<OnboardingResult>("No hay planes disponibles. Contacte al administrador.");

            // EstadoInmobiliaria activo = 1
            var estadoInmobiliariaActivo = 1;
            var estadoUsuarioActivo = 1;

            // ── Transacción atómica ───────────────────────────────────────────

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var ahora = DateTime.UtcNow;

                // 1. Crear Inmobiliaria
                var inmobiliaria = new Inmobiliaria
                {
                    Nombre = request.NombreInmobiliaria.Trim(),
                    Subdominio = subdominio,
                    IdPlan = planTrial.Id,
                    IdEstado = estadoInmobiliariaActivo,
                    CreadoEn = ahora,
                    ActualizadoEn = ahora
                };
                _context.Inmobiliaria.Add(inmobiliaria);
                await _context.SaveChangesAsync();

                // 2. Crear usuario Administrador
                var hashContrasena = HashPassword(request.Password);
                var usuario = new Usuario
                {
                    Nombre = request.NombreAdmin.Trim(),
                    Email = request.Email.ToLower().Trim(),
                    HashContrasena = hashContrasena,
                    Telefono = request.Telefono?.Trim(),
                    IdRol = rolAdmin.Id,
                    IdInmobiliaria = inmobiliaria.Id,
                    IdEstado = estadoUsuarioActivo,
                    CreadoEn = ahora,
                    ActualizadoEn = ahora
                };
                _context.Usuario.Add(usuario);
                await _context.SaveChangesAsync();

                // 3. Crear suscripción trial
                var suscripcion = new Suscripcion
                {
                    IdInmobiliaria = inmobiliaria.Id,
                    IdPlan = planTrial.Id,
                    Inicio = ahora,
                    Fin = ahora.AddDays(TrialDays),
                    RenovacionAutomatica = false,
                    IdEstado = 1, // activa
                    CreadoEn = ahora,
                    ActualizadoEn = ahora
                };
                _context.Suscripciones.Add(suscripcion);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();

                // 4. Cargar rol para el JWT
                usuario.Rol = rolAdmin;

                // 5. Generar tokens
                var accessToken = GenerateJwtToken(usuario);
                var refreshTokenValue = GenerateSecureToken();
                var refreshToken = new RefreshToken
                {
                    IdUsuario = usuario.Id,
                    IdInmobiliaria = inmobiliaria.Id,
                    Token = refreshTokenValue,
                    ExpiraEn = ahora.AddDays(30),
                    CreadoEn = ahora
                };
                _context.RefreshTokens.Add(refreshToken);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Onboarding completado: inmobiliaria {Nombre} (subdominio={Sub}), usuario {Email}, trial hasta {TrialFin}",
                    inmobiliaria.Nombre, subdominio, usuario.Email, suscripcion.Fin);

                return Ok(new OnboardingResult
                {
                    IdInmobiliaria = inmobiliaria.Id,
                    IdUsuario = usuario.Id,
                    NombreInmobiliaria = inmobiliaria.Nombre,
                    Subdominio = subdominio,
                    AccessToken = accessToken,
                    RefreshToken = refreshTokenValue,
                    ExpiresIn = 900,
                    TrialHasta = suscripcion.Fin
                }, $"Registro exitoso. Tu prueba gratuita de {TrialDays} días está activa.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Error en onboarding para email {Email}", request.Email);
                return Fail<OnboardingResult>("No se pudo completar el registro. Intente nuevamente.");
            }
        }

        // ── Helpers privados ─────────────────────────────────────────────────

        private string HashPassword(string password)
        {
            var salt = _configuration["Salt"]
                ?? throw new InvalidOperationException("Salt no configurado.");
            return Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password: password,
                    salt: Encoding.ASCII.GetBytes(salt),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var key = Encoding.ASCII.GetBytes(
                _configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException("Jwt:Key no configurado."));

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Email, usuario.Email ?? ""),
                new(ClaimTypes.Name, usuario.Nombre ?? ""),
                new(ClaimTypes.Role, usuario.Rol?.Nombre ?? AdminRolNombre),
                new("IdInmobiliaria", usuario.IdInmobiliaria.ToString())
            };

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(handler.CreateToken(descriptor));
        }

        private static string GenerateSecureToken() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        private static BaseResponseDto<T> Ok<T>(T data, string message) =>
            new() { Success = true, Data = data, Message = message };

        private static BaseResponseDto<T> Fail<T>(string message) =>
            new() { Success = false, Message = message };
    }
}
