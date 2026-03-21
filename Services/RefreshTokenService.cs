using System.Security.Cryptography;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;

namespace inmobiliariaApi.Services
{
    public class RefreshTokenService
    {
        private readonly RefreshTokenRepository _repository;
        private readonly ILogger<RefreshTokenService> _logger;
        private const int RefreshTokenExpiryDays = 30;

        public RefreshTokenService(RefreshTokenRepository repository, ILogger<RefreshTokenService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<RefreshToken> GenerateAsync(int idUsuario, int idInmobiliaria, string? ipOrigen = null)
        {
            var token = new RefreshToken
            {
                IdUsuario = idUsuario,
                IdInmobiliaria = idInmobiliaria,
                Token = GenerateSecureToken(),
                ExpiraEn = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays),
                CreadoEn = DateTime.UtcNow,
                IpOrigen = ipOrigen
            };

            await _repository.AddAsync(token);
            _logger.LogInformation("Refresh token generado para usuario {IdUsuario}", idUsuario);
            return token;
        }

        public async Task<(bool Valid, RefreshToken? Token, string Error)> ValidateAsync(string token)
        {
            var stored = await _repository.GetActiveByTokenAsync(token);

            if (stored == null)
            {
                // Token no existe o ya fue usado: posible reuse attack
                var anyToken = await _repository.GetByTokenAsync(token);
                if (anyToken != null)
                {
                    _logger.LogWarning("Refresh token reutilizado detectado para usuario {IdUsuario}. Revocando todos.", anyToken.IdUsuario);
                    await _repository.RevokeAllByUsuarioAsync(anyToken.IdUsuario);
                    return (false, null, "Token inválido. Sesión terminada por seguridad.");
                }
                return (false, null, "Token inválido o expirado.");
            }

            return (true, stored, string.Empty);
        }

        public async Task<RefreshToken> RotateAsync(RefreshToken oldToken, string? ipOrigen = null)
        {
            // Generar nuevo token
            var newToken = await GenerateAsync(oldToken.IdUsuario, oldToken.IdInmobiliaria, ipOrigen);

            // Revocar el anterior y registrar su reemplazo
            oldToken.RevocadoEn = DateTime.UtcNow;
            oldToken.ReemplazadoPor = newToken.Token;
            await _repository.UpdateAsync(oldToken);

            _logger.LogInformation("Refresh token rotado para usuario {IdUsuario}", oldToken.IdUsuario);
            return newToken;
        }

        public async Task RevokeAsync(string token)
        {
            var stored = await _repository.GetByTokenAsync(token);
            if (stored != null && stored.RevocadoEn == null)
            {
                stored.RevocadoEn = DateTime.UtcNow;
                await _repository.UpdateAsync(stored);
                _logger.LogInformation("Refresh token revocado para usuario {IdUsuario}", stored.IdUsuario);
            }
        }

        public async Task RevokeAllByUsuarioAsync(int idUsuario)
        {
            await _repository.RevokeAllByUsuarioAsync(idUsuario);
            _logger.LogInformation("Todos los refresh tokens revocados para usuario {IdUsuario}", idUsuario);
        }

        private static string GenerateSecureToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}
