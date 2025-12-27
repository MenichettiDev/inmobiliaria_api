using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using inmobiliariaApi.Data;
using inmobiliariaApi.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;

namespace inmobiliariaApi.Repositories
{
    public class UsuarioRepository : GenericRepository<Usuario>
    {
        private readonly ILogger<UsuarioRepository> _logger;
        private readonly IConfiguration _configuration;

        public UsuarioRepository(ApplicationDbContext context, ILogger<UsuarioRepository> logger, IConfiguration configuration) : base(context)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<Usuario>> GetByRolAsync(int rolId)
        {
            return await _dbSet.Where(u => u.IdRol == rolId).ToListAsync();
        }

        public async Task<IEnumerable<Usuario>> GetAllWithRolAsync()
        {
            try
            {
                var usuarios = await _context.Usuario
                    .Include(u => u.Rol)
                    .Select(u => new Usuario
                    {
                        Id = u.Id,
                        Nombre = u.Nombre,
                        Email = u.Email,
                        Telefono = u.Telefono,
                        IdRol = u.IdRol,
                        IdInmobiliaria = u.IdInmobiliaria,
                        IdEstado = u.IdEstado,
                        CreadoEn = u.CreadoEn,
                        ActualizadoEn = u.ActualizadoEn,
                        Rol = u.Rol
                    })
                    .ToListAsync();

                return usuarios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios con roles");
                throw;
            }
        }

        public async Task<Usuario?> GetByIdWithRolAsync(int id)
        {
            return await _dbSet.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> ValidateCredentialsAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation("Iniciando validación de credenciales para email: {Email}", email);

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning("Intento de validación con email o contraseña vacíos");
                    return false;
                }

                var usuario = await _context.Usuario
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (usuario == null)
                {
                    _logger.LogWarning("No se encontró usuario con email: {Email}", email);
                    return false;
                }

                _logger.LogInformation("Usuario encontrado para validación. Tiene contraseña configurada: {HasPassword}",
                    !string.IsNullOrEmpty(usuario.HashContrasena));

                // Check if user has a password set
                if (string.IsNullOrEmpty(usuario.HashContrasena))
                {
                    _logger.LogWarning("El usuario con email {Email} no tiene contraseña configurada", email);
                    return false;
                }

                // Verify password using KeyDerivation (matching the hash method)
                bool isValid = VerifyPasswordWithKeyDerivation(password, usuario.HashContrasena);

                _logger.LogInformation("Validación de credenciales completada para email {Email}: {IsValid}", email, isValid);

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al validar credenciales para email: {Email}", email);
                return false;
            }
        }

        public async Task<IEnumerable<Usuario>> GetActiveUsersAsync()
        {
            return await _dbSet.Where(u => u.IdEstado == 1).ToListAsync();
        }

        public async Task<Usuario?> GetByEmailWithRolAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("Intento de búsqueda con email vacío o nulo");
                    return null;
                }

                _logger.LogInformation("Buscando usuario por email: {Email}", email);

                var usuario = await _context.Usuario
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.Email == email);

                _logger.LogInformation("Resultado de búsqueda por email {Email}: {Found}",
                    email, usuario != null ? "Usuario encontrado" : "Usuario no encontrado");

                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al buscar usuario por email: {Email}", email);
                throw;
            }
        }

        public async Task<(IEnumerable<Usuario> Data, int TotalRecords)> GetAllWithRolPagedAsync(int page, int pageSize)
        {
            var totalRecords = await _dbSet.CountAsync();
            var data = await _dbSet
                .Include(u => u.Rol)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalRecords);
        }

        public async Task<Usuario?> GetByIdWithRolAndTenantAsync(int id, int tenantId)
        {
            return await _dbSet
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id && u.IdInmobiliaria == tenantId);
        }

        public async Task<IEnumerable<Usuario>> GetActiveUsersByTenantAsync(int tenantId)
        {
            return await _dbSet
                .Include(u => u.Rol)
                .Where(u => u.IdEstado == 1 && u.IdInmobiliaria == tenantId)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Usuario> Data, int TotalRecords)> GetAllWithRolPagedByTenantAsync(
            int page, int pageSize, int tenantId, string? nombre = null, int? rolId = null, int? estadoId = null)
        {
            var query = _dbSet.Include(u => u.Rol).Where(u => u.IdInmobiliaria == tenantId);

            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(u => u.Nombre.Contains(nombre));
            }

            if (rolId.HasValue)
            {
                query = query.Where(u => u.IdRol == rolId.Value);
            }

            if (estadoId.HasValue)
            {
                query = query.Where(u => u.IdEstado == estadoId.Value);
            }

            var totalRecords = await query.CountAsync();
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalRecords);
        }

        public async Task<IEnumerable<Usuario>> GetActiveAndBlockedUsersByTenantAsync(int tenantId)
        {
            return await _dbSet
                .Include(u => u.Rol)
                .Where(u => (u.IdEstado == 1 || u.IdEstado == 2) && u.IdInmobiliaria == tenantId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Usuario>> GetInactiveUsersByTenantAsync(int tenantId)
        {
            return await _dbSet
                .Include(u => u.Rol)
                .Where(u => u.IdEstado == 3 && u.IdInmobiliaria == tenantId)
                .ToListAsync();
        }

        // Método auxiliar para verificar contraseña usando KeyDerivation
        private bool VerifyPasswordWithKeyDerivation(string password, string hashedPassword)
        {
            try
            {
                // Salt fijo (debe coincidir con el usado para hashear)
                string? salt = _configuration["Salt"]; // Lee el salt del archivo de configuración
                if (string.IsNullOrEmpty(salt))
                {
                    _logger.LogError("El valor de 'Salt' no está configurado en appsettings.json");
                    throw new InvalidOperationException("Configuración de seguridad incompleta. Contacte al administrador del sistema.");
                }

                // Hashear la contraseña ingresada con el mismo método
                string computedHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: Encoding.ASCII.GetBytes(salt),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));

                // Comparar los hashes
                return computedHash == hashedPassword;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar contraseña con KeyDerivation");
                return false;
            }
        }

        public async Task<bool> ValidateUserPasswordAsync(int userId, string password)
        {
            var usuario = await _context.Usuario.FindAsync(userId);
            if (usuario == null || string.IsNullOrEmpty(usuario.HashContrasena))
                return false;

            string salt = _configuration["Salt"] ?? string.Empty;
            if (string.IsNullOrEmpty(salt))
                throw new InvalidOperationException("El valor de 'Salt' no está configurado.");

            string hashedPassword = Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password: password,
                    salt: Encoding.ASCII.GetBytes(salt),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8
                )
            );

            return usuario.HashContrasena == hashedPassword;
        }
    }
}