using System.Text;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using inmobiliariaApi.Dtos.Common;
using inmobiliariaApi.Dtos.UsuarioWeb;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;

namespace inmobiliariaApi.Services
{
    public class UsuarioWebService
    {
        private readonly UsuarioWebRepository _usuarioWebRepository;
        private readonly ILogger<UsuarioWebService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public UsuarioWebService(
            UsuarioWebRepository usuarioWebRepository,
            ILogger<UsuarioWebService> logger,
            IConfiguration configuration,
            HttpClient httpClient
        )
        {
            _usuarioWebRepository = usuarioWebRepository;
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Hashea una contraseña usando PBKDF2
        /// </summary>
        private string HashPassword(string password)
        {
            string salt = _configuration["Salt"] ?? string.Empty;
            if (string.IsNullOrEmpty(salt))
            {
                throw new InvalidOperationException("El valor de 'Salt' no está configurado en appsettings.json.");
            }

            string hashedPassword = Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password: password,
                    salt: Encoding.ASCII.GetBytes(salt),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8
                )
            );

            return hashedPassword;
        }

        /// <summary>
        /// Verifica una contraseña contra su hash
        /// </summary>
        private bool VerifyPassword(string password, string hash)
        {
            try
            {
                string hashOfInput = HashPassword(password);
                return hashOfInput == hash;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Mapea UsuarioWeb a UsuarioWebDto
        /// </summary>
        private UsuarioWebDto MapToDto(UsuarioWeb usuarioWeb)
        {
            return new UsuarioWebDto
            {
                Id = usuarioWeb.Id,
                Nombre = usuarioWeb.Nombre,
                Email = usuarioWeb.Email,
                ProveedorAuth = usuarioWeb.ProveedorAuth,
                EmailVerificado = usuarioWeb.EmailVerificado,
                CreadoEn = usuarioWeb.CreadoEn
            };
        }

        /// <summary>
        /// Registra un nuevo usuario web con contraseña local
        /// </summary>
        public async Task<BaseResponseDto<UsuarioWebDto>> RegisterAsync(RegisterUsuarioWebDto dto)
        {
            try
            {
                // Validar que el email sea único
                if (await _usuarioWebRepository.EmailExistsAsync(dto.Email))
                {
                    return new BaseResponseDto<UsuarioWebDto>
                    {
                        Success = false,
                        Message = "El email ya está registrado",
                        Errors = new List<string> { "Email duplicado" }
                    };
                }

                // Crear nuevo usuario web
                var usuarioWeb = new UsuarioWeb
                {
                    Nombre = dto.Nombre,
                    Email = dto.Email,
                    HashContrasena = HashPassword(dto.Password),
                    ProveedorAuth = "local",
                    Activo = true,
                    EmailVerificado = false,
                    CreadoEn = DateTime.UtcNow,
                    ActualizadoEn = DateTime.UtcNow
                };

                var resultado = await _usuarioWebRepository.AddAsync(usuarioWeb);
                _logger.LogInformation($"Usuario web registrado: {resultado.Email}");

                return new BaseResponseDto<UsuarioWebDto>
                {
                    Success = true,
                    Message = "Usuario registrado exitosamente",
                    Data = MapToDto(resultado)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario web");
                return new BaseResponseDto<UsuarioWebDto>
                {
                    Success = false,
                    Message = "Error al registrar el usuario",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Autentica un usuario web con email y contraseña
        /// </summary>
        public async Task<BaseResponseDto<UsuarioWebDto>> AuthenticateAsync(string email, string password)
        {
            try
            {
                var usuarioWeb = await _usuarioWebRepository.GetByEmailAsync(email);

                if (usuarioWeb == null || !VerifyPassword(password, usuarioWeb.HashContrasena ?? ""))
                {
                    return new BaseResponseDto<UsuarioWebDto>
                    {
                        Success = false,
                        Message = "Email o contraseña incorrectos",
                        Errors = new List<string> { "Credenciales inválidas" }
                    };
                }

                _logger.LogInformation($"Usuario web autenticado: {usuarioWeb.Email}");

                return new BaseResponseDto<UsuarioWebDto>
                {
                    Success = true,
                    Message = "Autenticación exitosa",
                    Data = MapToDto(usuarioWeb)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al autenticar usuario web");
                return new BaseResponseDto<UsuarioWebDto>
                {
                    Success = false,
                    Message = "Error al autenticar",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Autentica un usuario web con Google OAuth
        /// </summary>
        public async Task<BaseResponseDto<UsuarioWebDto>> GoogleAuthAsync(string credential)
        {
            try
            {
                // Validar el credential con Google
                var googleTokenInfo = await ValidateGoogleTokenAsync(credential);

                if (!googleTokenInfo.Success)
                {
                    return new BaseResponseDto<UsuarioWebDto>
                    {
                        Success = false,
                        Message = "Token de Google inválido",
                        Errors = new List<string> { googleTokenInfo.Message }
                    };
                }

                string googleId = googleTokenInfo.GoogleId!;
                string email = googleTokenInfo.Email!;
                string nombre = googleTokenInfo.Name!;

                // Buscar usuario existente por Google ID
                var usuarioExistente = await _usuarioWebRepository.GetByGoogleIdAsync(googleId);

                if (usuarioExistente != null)
                {
                    _logger.LogInformation($"Usuario Google autenticado: {usuarioExistente.Email}");
                    return new BaseResponseDto<UsuarioWebDto>
                    {
                        Success = true,
                        Message = "Autenticación exitosa",
                        Data = MapToDto(usuarioExistente)
                    };
                }

                // Si no existe, crear nuevo usuario
                var usuarioWeb = new UsuarioWeb
                {
                    Nombre = nombre,
                    Email = email,
                    GoogleId = googleId,
                    ProveedorAuth = "google",
                    HashContrasena = null,
                    Activo = true,
                    EmailVerificado = true, // Google ya verifica el email
                    CreadoEn = DateTime.UtcNow,
                    ActualizadoEn = DateTime.UtcNow
                };

                var resultado = await _usuarioWebRepository.AddAsync(usuarioWeb);
                _logger.LogInformation($"Usuario web creado desde Google: {resultado.Email}");

                return new BaseResponseDto<UsuarioWebDto>
                {
                    Success = true,
                    Message = "Usuario creado y autenticado exitosamente",
                    Data = MapToDto(resultado)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en autenticación Google");
                return new BaseResponseDto<UsuarioWebDto>
                {
                    Success = false,
                    Message = "Error en autenticación",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Valida un token de Google contra la API de Google
        /// </summary>
        private async Task<GoogleTokenValidationResult> ValidateGoogleTokenAsync(string credential)
        {
            try
            {
                var clientId = _configuration["Google:ClientId"];
                if (string.IsNullOrEmpty(clientId))
                {
                    return new GoogleTokenValidationResult
                    {
                        Success = false,
                        Message = "Google Client ID no está configurado"
                    };
                }

                var response = await _httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={credential}");

                if (!response.IsSuccessStatusCode)
                {
                    return new GoogleTokenValidationResult
                    {
                        Success = false,
                        Message = "Token de Google inválido"
                    };
                }

                var json = await response.Content.ReadAsAsync<Dictionary<string, object>>();

                // Verificar que el aud (audience) coincida con el Client ID
                if (json.TryGetValue("aud", out var aud) && aud?.ToString() != clientId)
                {
                    return new GoogleTokenValidationResult
                    {
                        Success = false,
                        Message = "Client ID no coincide"
                    };
                }

                string? googleId = json.TryGetValue("sub", out var sub) ? sub?.ToString() : null;
                string? email = json.TryGetValue("email", out var emailObj) ? emailObj?.ToString() : null;
                string? nombre = json.TryGetValue("name", out var nameObj) ? nameObj?.ToString() : null;

                if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(email))
                {
                    return new GoogleTokenValidationResult
                    {
                        Success = false,
                        Message = "Token no contiene información requerida"
                    };
                }

                return new GoogleTokenValidationResult
                {
                    Success = true,
                    GoogleId = googleId,
                    Email = email,
                    Name = nombre ?? "Usuario"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando token de Google");
                return new GoogleTokenValidationResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// Obtiene los favoritos de un usuario web
        /// </summary>
        public async Task<BaseResponseDto<IEnumerable<PropiedadFavoritaDto>>> GetFavoritosAsync(int idUsuarioWeb)
        {
            try
            {
                var favoritos = await _usuarioWebRepository.GetFavoritosAsync(idUsuarioWeb);

                var dtos = favoritos.Select(f => new PropiedadFavoritaDto
                {
                    Id = f.Id,
                    IdPropiedad = f.IdPropiedad,
                    CreadoEn = f.CreadoEn,
                    PropiedadTitulo = f.Propiedad?.Titulo ?? "Sin título",
                    PropiedadDireccion = f.Propiedad?.Direccion ?? "Sin dirección",
                    PropiedadPrecio = f.Propiedad?.Precio,
                    PropiedadImageUrl = f.Propiedad?.UrlImagenes?.FirstOrDefault()
                }).ToList();

                return new BaseResponseDto<IEnumerable<PropiedadFavoritaDto>>
                {
                    Success = true,
                    Data = dtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener favoritos");
                return new BaseResponseDto<IEnumerable<PropiedadFavoritaDto>>
                {
                    Success = false,
                    Message = "Error al obtener favoritos",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Toggle de favorito (agrega si no existe, remueve si existe)
        /// </summary>
        public async Task<BaseResponseDto<bool>> ToggleFavoritoAsync(int idUsuarioWeb, int idPropiedad)
        {
            try
            {
                var esFavorito = await _usuarioWebRepository.IsFavoritoAsync(idUsuarioWeb, idPropiedad);

                if (esFavorito)
                {
                    await _usuarioWebRepository.RemoveFavoritoAsync(idUsuarioWeb, idPropiedad);
                    _logger.LogInformation($"Favorito removido: usuario {idUsuarioWeb}, propiedad {idPropiedad}");

                    return new BaseResponseDto<bool>
                    {
                        Success = true,
                        Message = "Favorito removido",
                        Data = false
                    };
                }
                else
                {
                    var favorito = new PropiedadFavorita
                    {
                        IdUsuarioWeb = idUsuarioWeb,
                        IdPropiedad = idPropiedad,
                        CreadoEn = DateTime.UtcNow
                    };

                    await _usuarioWebRepository.AddFavoritoAsync(favorito);
                    _logger.LogInformation($"Favorito agregado: usuario {idUsuarioWeb}, propiedad {idPropiedad}");

                    return new BaseResponseDto<bool>
                    {
                        Success = true,
                        Message = "Favorito agregado",
                        Data = true
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al toggle de favorito");
                return new BaseResponseDto<bool>
                {
                    Success = false,
                    Message = "Error al procesar favorito",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Obtiene las consultas (leads) de un usuario web
        /// </summary>
        public async Task<BaseResponseDto<IEnumerable<object>>> GetConsultasAsync(int idUsuarioWeb)
        {
            try
            {
                var leads = await _usuarioWebRepository.GetConsultasAsync(idUsuarioWeb);

                var dtos = leads.Select(l => new
                {
                    l.Id,
                    l.NombreCompleto,
                    l.Email,
                    l.Telefono,
                    l.Mensaje,
                    EstadoNombre = l.Estado?.Nombre ?? "Desconocido",
                    PropiedadTitulo = l.Propiedad?.Titulo ?? "Sin propiedad",
                    l.CreadoEn,
                    l.ActualizadoEn
                }).ToList<object>();

                return new BaseResponseDto<IEnumerable<object>>
                {
                    Success = true,
                    Data = dtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener consultas");
                return new BaseResponseDto<IEnumerable<object>>
                {
                    Success = false,
                    Message = "Error al obtener consultas",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Obtiene un usuario web por ID
        /// </summary>
        public async Task<BaseResponseDto<UsuarioWebDto>> GetByIdAsync(int id)
        {
            try
            {
                var usuarioWeb = await _usuarioWebRepository.GetByIdAsync(id);

                if (usuarioWeb == null || !usuarioWeb.Activo)
                {
                    return new BaseResponseDto<UsuarioWebDto>
                    {
                        Success = false,
                        Message = "Usuario no encontrado"
                    };
                }

                return new BaseResponseDto<UsuarioWebDto>
                {
                    Success = true,
                    Data = MapToDto(usuarioWeb)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario web");
                return new BaseResponseDto<UsuarioWebDto>
                {
                    Success = false,
                    Message = "Error al obtener usuario",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }

    /// <summary>
    /// Clase auxiliar para el resultado de validación de Google
    /// </summary>
    internal class GoogleTokenValidationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? GoogleId { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
    }
}
