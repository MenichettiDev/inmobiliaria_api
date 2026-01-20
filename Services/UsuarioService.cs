using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using inmobiliariaApi.DTOs.Common;
using inmobiliariaApi.DTOs.Usuario;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;

namespace inmobiliariaApi.Services
{
    public class UsuarioService : GenericService<Usuario>
    {
        private readonly UsuarioRepository _usuarioRepository;
        private readonly ILogger<UsuarioService> _logger;
        private readonly IConfiguration _configuration;

        public UsuarioService(
            UsuarioRepository usuarioRepository,
            ILogger<UsuarioService> logger,
            IConfiguration configuration
        )
            : base(usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
            _logger = logger;
            _configuration = configuration;
        }

        // Método privado para hashear contraseñas
        private string HashPassword(string password)
        {
            string salt = _configuration["Salt"] ?? string.Empty;
            if (string.IsNullOrEmpty(salt))
            {
                throw new InvalidOperationException(
                    "El valor de 'Salt' no está configurado en appsettings.json."
                );
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

        // Método para mapear Usuario a UsuarioResponseDto
        private UsuarioResponseDto MapToResponseDto(Usuario usuario)
        {
            return new UsuarioResponseDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                IdInmobiliaria = usuario.IdInmobiliaria,
                IdRol = usuario.IdRol,
                IdEstado = usuario.IdEstado,
                CreadoEn = usuario.CreadoEn,
                ActualizadoEn = usuario.ActualizadoEn,
                RolNombre = usuario.Rol?.Nombre ?? string.Empty,
                // Asegurar que no se incluyan datos sensibles:
                // NO incluir: HashContrasena, navegaciones innecesarias, etc.
            };
        }

        // Método para mapear Usuario a UsuarioComboDto
        private UsuarioComboDto MapToComboDto(Usuario usuario)
        {
            string estadoNombre = usuario.IdEstado switch
            {
                1 => "Activo",
                2 => "Bloqueado",
                3 => "Inactivo",
                _ => "Desconocido"
            };

            return new UsuarioComboDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                IdRol = usuario.IdRol,
                RolNombre = usuario.Rol?.Nombre ?? string.Empty,
                IdEstado = usuario.IdEstado,
                EstadoNombre = estadoNombre
            };
        }

        public async Task<BaseResponseDto<IEnumerable<UsuarioComboDto>>> GetUsuariosForComboByTenantAsync(int tenantId)
        {
            try
            {
                var usuarios = await _usuarioRepository.GetUsersForComboByTenantAsync(tenantId);
                var usuariosCombo = usuarios.Select(MapToComboDto).ToList();

                return new BaseResponseDto<IEnumerable<UsuarioComboDto>>
                {
                    Success = true,
                    Data = usuariosCombo,
                    Message = "Personal obtenido correctamente para combo",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios para combo del tenant: {TenantId}", tenantId);
                return new BaseResponseDto<IEnumerable<UsuarioComboDto>>
                {
                    Success = false,
                    Message = "Error al cargar el personal.",
                    Errors = new List<string> { "Error interno del servidor." },
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<UsuarioResponseDto>>> GetAllUsuariosAsync()
        {
            try
            {
                var usuarios = await _usuarioRepository.GetAllWithRolAsync();
                var usuariosDto = usuarios.Select(MapToResponseDto).ToList();

                return new BaseResponseDto<IEnumerable<UsuarioResponseDto>>
                {
                    Success = true,
                    Data = usuariosDto,
                    Message = "Usuarios obtenidos correctamente",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los usuarios");
                return new BaseResponseDto<IEnumerable<UsuarioResponseDto>>
                {
                    Success = false,
                    Message = "No se pudieron cargar los usuarios. Por favor, intente nuevamente.",
                    Errors = new List<string>
                    {
                        "Error interno del servidor al procesar la solicitud.",
                    },
                };
            }
        }

        public async Task<BaseResponseDto<PaginatedResponseDto<UsuarioResponseDto>>> GetAllUsuariosPaginatedAsync(
            int page,
            int pageSize,
            string? nombre = null,
            int? rolId = null,
            int? inmobiliariaId = null,
            int? estadoId = null
        )
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;

                // Usar el método que filtra por tenant
                var (usuarios, totalRecords) = await _usuarioRepository.GetAllWithRolPagedByTenantAsync(
                    page,
                    pageSize,
                    inmobiliariaId ?? 0, // Usar el tenantId del parámetro
                    nombre,
                    rolId,
                    estadoId
                );

                var usuariosDto = usuarios.Select(MapToResponseDto).ToList();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var paginatedResponse = new PaginatedResponseDto<UsuarioResponseDto>
                {
                    Data = usuariosDto,
                    Page = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1,
                };

                return new BaseResponseDto<PaginatedResponseDto<UsuarioResponseDto>>
                {
                    Success = true,
                    Data = paginatedResponse,
                    Message = "Usuarios obtenidos correctamente",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios paginados para tenant: {TenantId}", inmobiliariaId);
                return new BaseResponseDto<PaginatedResponseDto<UsuarioResponseDto>>
                {
                    Success = false,
                    Message = "No se pudieron cargar los usuarios. Por favor, intente nuevamente.",
                    Errors = new List<string>
                    {
                        "Error interno del servidor al procesar la solicitud.",
                    },
                };
            }
        }

        public async Task<BaseResponseDto<UsuarioResponseDto>> GetUsuarioByIdAsync(int id)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdWithRolAsync(id);
                if (usuario == null)
                {
                    return new BaseResponseDto<UsuarioResponseDto>
                    {
                        Success = false,
                        Message = $"No se encontró un usuario con el ID {id}.",
                    };
                }

                var usuarioDto = MapToResponseDto(usuario);
                return new BaseResponseDto<UsuarioResponseDto>
                {
                    Success = true,
                    Data = usuarioDto,
                    Message = "Usuario encontrado",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por ID: {Id}", id);
                return new BaseResponseDto<UsuarioResponseDto>
                {
                    Success = false,
                    Message = $"Error al buscar el usuario con ID {id}. Por favor, intente nuevamente.",
                    Errors = new List<string>
                    {
                        "Error interno del servidor al procesar la solicitud.",
                    },
                };
            }
        }

        public async Task<BaseResponseDto<UsuarioResponseDto>> GetUsuarioByIdAndTenantAsync(int id, int tenantId)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdWithRolAndTenantAsync(id, tenantId);
                if (usuario == null)
                {
                    return new BaseResponseDto<UsuarioResponseDto>
                    {
                        Success = false,
                        Message = $"No se encontró un usuario con el ID {id} en su organización.",
                    };
                }

                var usuarioDto = MapToResponseDto(usuario);
                return new BaseResponseDto<UsuarioResponseDto>
                {
                    Success = true,
                    Data = usuarioDto,
                    Message = "Usuario encontrado",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por ID: {Id} y Tenant: {TenantId}", id, tenantId);
                return new BaseResponseDto<UsuarioResponseDto>
                {
                    Success = false,
                    Message = $"Error al buscar el usuario. Por favor, intente nuevamente.",
                    Errors = new List<string> { "Error interno del servidor." },
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<UsuarioResponseDto>>> GetActiveUsuariosByTenantAsync(int tenantId)
        {
            try
            {
                var usuarios = await _usuarioRepository.GetActiveUsersByTenantAsync(tenantId);
                var usuariosDto = usuarios.Select(MapToResponseDto).ToList();

                return new BaseResponseDto<IEnumerable<UsuarioResponseDto>>
                {
                    Success = true,
                    Data = usuariosDto,
                    Message = "Usuarios activos obtenidos correctamente",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios activos del tenant: {TenantId}", tenantId);
                return new BaseResponseDto<IEnumerable<UsuarioResponseDto>>
                {
                    Success = false,
                    Message = "Error al cargar usuarios activos.",
                    Errors = new List<string> { "Error interno del servidor." },
                };
            }
        }

        public async Task<BaseResponseDto<UsuarioResponseDto>> CreateUsuarioAsync(CreateUsuarioDto createDto)
        {
            try
            {
                _logger.LogInformation("Iniciando creación de usuario con email: {Email} para inmobiliaria: {IdInmobiliaria}",
                    createDto.Email, createDto.IdInmobiliaria);

                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(createDto.Email))
                {
                    return new BaseResponseDto<UsuarioResponseDto>
                    {
                        Success = false,
                        Message = "El email es obligatorio."
                    };
                }

                if (string.IsNullOrWhiteSpace(createDto.Nombre))
                {
                    return new BaseResponseDto<UsuarioResponseDto>
                    {
                        Success = false,
                        Message = "El nombre es obligatorio."
                    };
                }

                if (string.IsNullOrWhiteSpace(createDto.Password))
                {
                    return new BaseResponseDto<UsuarioResponseDto>
                    {
                        Success = false,
                        Message = "La contraseña es obligatoria."
                    };
                }

                if (createDto.IdRol <= 0)
                {
                    return new BaseResponseDto<UsuarioResponseDto>
                    {
                        Success = false,
                        Message = "Debe especificar un rol válido."
                    };
                }

                if (createDto.IdEstado < 1 || createDto.IdEstado > 3)
                {
                    return new BaseResponseDto<UsuarioResponseDto>
                    {
                        Success = false,
                        Message = "El estado debe ser 1 (activo), 2 (bloqueado) o 3 (inactivo)."
                    };
                }

                // Validar si el email ya existe
                var existingEmail = await _usuarioRepository.GetByEmailAsync(createDto.Email);
                if (existingEmail != null)
                {
                    _logger.LogWarning("Intento de crear usuario con email existente: {Email}", createDto.Email);
                    return new BaseResponseDto<UsuarioResponseDto>
                    {
                        Success = false,
                        Message = $"Ya existe un usuario registrado con el email {createDto.Email}. Por favor, use un email diferente.",
                    };
                }

                var usuario = new Usuario
                {
                    Nombre = createDto.Nombre,
                    Email = createDto.Email,
                    Telefono = createDto.Telefono,
                    IdRol = createDto.IdRol,
                    IdInmobiliaria = createDto.IdInmobiliaria, // Ya viene asignado por el controller
                    IdEstado = createDto.IdEstado,
                    CreadoEn = DateTime.UtcNow,
                    ActualizadoEn = DateTime.UtcNow,
                };

                // Hashear la contraseña
                _logger.LogInformation("Hasheando contraseña para usuario: {Email}", createDto.Email);
                usuario.HashContrasena = HashPassword(createDto.Password);

                _logger.LogInformation("Guardando usuario en base de datos: {Email} con inmobiliaria: {IdInmobiliaria}",
                    createDto.Email, createDto.IdInmobiliaria);
                var result = await _usuarioRepository.AddAsync(usuario);

                _logger.LogInformation("Usuario creado exitosamente con ID: {Id} para inmobiliaria: {IdInmobiliaria}",
                    result.Id, result.IdInmobiliaria);

                // Cargar el usuario con rol para la respuesta
                var usuarioConRol = await _usuarioRepository.GetByIdWithRolAsync(result.Id);
                var responseDto = MapToResponseDto(usuarioConRol ?? result);

                return new BaseResponseDto<UsuarioResponseDto>
                {
                    Success = true,
                    Data = responseDto,
                    Message = $"El usuario {createDto.Nombre} ha sido creado exitosamente.",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario: {Email}. Error: {Message}", createDto?.Email, ex.Message);
                return new BaseResponseDto<UsuarioResponseDto>
                {
                    Success = false,
                    Message = "No se pudo crear el usuario. Por favor, verifique los datos ingresados e intente nuevamente.",
                    Errors = new List<string>
                    {
                        $"Error interno: {ex.Message}",
                    },
                };
            }
        }

        public async Task<BaseResponseDto<UsuarioResponseDto>> UpdateUsuarioAsync(UpdateUsuarioDto updateDto)
        {
            try
            {
                var existingUser = await _usuarioRepository.GetByIdWithRolAsync(updateDto.Id);
                if (existingUser == null)
                {
                    return new BaseResponseDto<UsuarioResponseDto>
                    {
                        Success = false,
                        Message = $"No se encontró un usuario con el ID {updateDto.Id} para actualizar.",
                    };
                }

                // Validar email único si se modifica
                if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != existingUser.Email)
                {
                    var existingEmail = await _usuarioRepository.GetByEmailAsync(updateDto.Email);
                    if (existingEmail != null)
                    {
                        return new BaseResponseDto<UsuarioResponseDto>
                        {
                            Success = false,
                            Message = $"Ya existe otro usuario registrado con el email {updateDto.Email}. Por favor, use un email diferente.",
                        };
                    }
                }

                // Actualizar campos
                if (!string.IsNullOrEmpty(updateDto.Nombre))
                    existingUser.Nombre = updateDto.Nombre;

                if (!string.IsNullOrEmpty(updateDto.Email))
                    existingUser.Email = updateDto.Email;

                if (updateDto.Telefono != null)
                    existingUser.Telefono = updateDto.Telefono;

                if (updateDto.IdRol.HasValue)
                    existingUser.IdRol = updateDto.IdRol.Value;

                if (updateDto.IdInmobiliaria.HasValue)
                    existingUser.IdInmobiliaria = updateDto.IdInmobiliaria.Value;

                if (updateDto.IdEstado.HasValue)
                    existingUser.IdEstado = updateDto.IdEstado.Value;

                // Actualizar contraseña si se envía
                if (!string.IsNullOrEmpty(updateDto.Password))
                {
                    existingUser.HashContrasena = HashPassword(updateDto.Password);
                }

                existingUser.ActualizadoEn = DateTime.UtcNow;

                await _usuarioRepository.UpdateAsync(existingUser);

                // Cargar el usuario actualizado para la respuesta
                var updatedUser = await _usuarioRepository.GetByIdWithRolAsync(existingUser.Id);
                var responseDto = MapToResponseDto(updatedUser ?? existingUser);

                return new BaseResponseDto<UsuarioResponseDto>
                {
                    Success = true,
                    Data = responseDto,
                    Message = $"Los datos del usuario {existingUser.Nombre} han sido actualizados correctamente.",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario: {Id}", updateDto.Id);
                return new BaseResponseDto<UsuarioResponseDto>
                {
                    Success = false,
                    Message = "No se pudieron actualizar los datos del usuario. Por favor, intente nuevamente.",
                    Errors = new List<string>
                    {
                        "Error interno del servidor al procesar la solicitud.",
                    },
                };
            }
        }

        public async Task<BaseResponseDto<UsuarioResponseDto>> UpdateUsuarioAsync(UpdateUsuarioDto updateDto, int tenantId)
        {
            try
            {
                _logger.LogInformation("Iniciando actualización de usuario ID: {Id} en tenant: {TenantId}", updateDto.Id, tenantId);

                var existingUser = await _usuarioRepository.GetByIdWithRolAndTenantAsync(updateDto.Id, tenantId);
                if (existingUser == null)
                {
                    _logger.LogWarning("Intento de actualizar usuario inexistente: ID {Id} en tenant {TenantId}", updateDto.Id, tenantId);
                    return new BaseResponseDto<UsuarioResponseDto>
                    {
                        Success = false,
                        Message = $"No se encontró un usuario con el ID {updateDto.Id} en su organización.",
                    };
                }

                // Validar email único si se modifica (solo dentro del mismo tenant para mayor seguridad)
                if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != existingUser.Email)
                {
                    var existingEmail = await _usuarioRepository.GetByEmailAsync(updateDto.Email);
                    if (existingEmail != null && existingEmail.Id != existingUser.Id)
                    {
                        _logger.LogWarning("Intento de usar email existente: {Email} para usuario {Id}", updateDto.Email, updateDto.Id);
                        return new BaseResponseDto<UsuarioResponseDto>
                        {
                            Success = false,
                            Message = $"Ya existe otro usuario registrado con el email {updateDto.Email}. Por favor, use un email diferente.",
                        };
                    }
                }

                // Actualizar campos
                if (!string.IsNullOrEmpty(updateDto.Nombre))
                    existingUser.Nombre = updateDto.Nombre;

                if (!string.IsNullOrEmpty(updateDto.Email))
                    existingUser.Email = updateDto.Email;

                if (updateDto.Telefono != null)
                    existingUser.Telefono = updateDto.Telefono;

                if (updateDto.IdRol.HasValue)
                    existingUser.IdRol = updateDto.IdRol.Value;

                if (updateDto.IdEstado.HasValue)
                    existingUser.IdEstado = updateDto.IdEstado.Value;

                // IMPORTANTE: NO permitir cambio de inmobiliaria
                // Mantener siempre el tenant original
                existingUser.IdInmobiliaria = tenantId;

                // Actualizar contraseña si se envía
                if (!string.IsNullOrEmpty(updateDto.Password))
                {
                    _logger.LogInformation("Actualizando contraseña para usuario ID: {Id}", updateDto.Id);
                    existingUser.HashContrasena = HashPassword(updateDto.Password);
                }

                existingUser.ActualizadoEn = DateTime.UtcNow;

                await _usuarioRepository.UpdateAsync(existingUser);

                _logger.LogInformation("Usuario ID: {Id} actualizado exitosamente en tenant: {TenantId}", updateDto.Id, tenantId);

                // Cargar el usuario actualizado para la respuesta
                var updatedUser = await _usuarioRepository.GetByIdWithRolAndTenantAsync(existingUser.Id, tenantId);
                var responseDto = MapToResponseDto(updatedUser ?? existingUser);

                return new BaseResponseDto<UsuarioResponseDto>
                {
                    Success = true,
                    Data = responseDto,
                    Message = $"Los datos del usuario {existingUser.Nombre} han sido actualizados correctamente.",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario: {Id} en tenant: {TenantId}", updateDto.Id, tenantId);
                return new BaseResponseDto<UsuarioResponseDto>
                {
                    Success = false,
                    Message = "No se pudieron actualizar los datos del usuario. Por favor, intente nuevamente.",
                    Errors = new List<string>
                    {
                        $"Error interno: {ex.Message}",
                    },
                };
            }
        }

        public async Task<BaseResponseDto<Usuario>> AuthenticateAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation("Iniciando autenticación para email: {Email}", email);

                var usuario = await _usuarioRepository.GetByEmailWithRolAsync(email);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado con email: {Email}", email);
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message = "Las credenciales ingresadas son incorrectas. Por favor, verifique su email y contraseña.",
                    };
                }

                var isValidPassword = await _usuarioRepository.ValidateCredentialsAsync(email, password);

                if (!isValidPassword)
                {
                    _logger.LogWarning("Contraseña incorrecta para email: {Email}", email);
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message = "Las credenciales ingresadas son incorrectas. Por favor, verifique su email y contraseña.",
                    };
                }

                // Verificar que el usuario esté activo
                if (usuario.IdEstado != 1)
                {
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message = "Su cuenta se encuentra inactiva. Por favor, contacte al administrador del sistema.",
                    };
                }

                _logger.LogInformation("Autenticación exitosa para email: {Email}", email);

                return new BaseResponseDto<Usuario>
                {
                    Success = true,
                    Data = usuario,
                    Message = $"¡Bienvenido/a {usuario.Nombre}! Ha iniciado sesión correctamente.",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la autenticación para email: {Email}", email);
                return new BaseResponseDto<Usuario>
                {
                    Success = false,
                    Message = "No se pudo completar el proceso de autenticación. Por favor, intente nuevamente.",
                    Errors = new List<string>
                    {
                        "Error interno del servidor al procesar la solicitud.",
                    },
                };
            }
        }

        public async Task<BaseResponseDto<object>> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(userId);
                if (usuario == null)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Usuario no encontrado."
                    };
                }

                // Verificar que el usuario está activo
                if (usuario.IdEstado != 1)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Su cuenta se encuentra inactiva. No puede cambiar la contraseña."
                    };
                }

                // Validar contraseña actual
                var isCurrentPasswordValid = await _usuarioRepository.ValidateCredentialsAsync(usuario.Email, changePasswordDto.CurrentPassword);
                if (!isCurrentPasswordValid)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "La contraseña actual es incorrecta."
                    };
                }

                // Hashear la nueva contraseña
                usuario.HashContrasena = HashPassword(changePasswordDto.NewPassword);
                usuario.ActualizadoEn = DateTime.UtcNow;

                await _usuarioRepository.UpdateAsync(usuario);

                _logger.LogInformation("Contraseña cambiada exitosamente para usuario ID: {UserId}", userId);

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Message = "Su contraseña ha sido cambiada exitosamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña para usuario ID: {UserId}", userId);
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "No se pudo cambiar la contraseña. Por favor, intente nuevamente.",
                    Errors = new List<string>
                    {
                        "Error interno del servidor al procesar la solicitud."
                    }
                };
            }
        }

        public async Task<BaseResponseDto<UsuarioResponseDto>> GetMyselfAsync(int userId)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdWithRolAsync(userId);

                if (usuario == null)
                {
                    return new BaseResponseDto<UsuarioResponseDto>
                    {
                        Success = false,
                        Message = $"No se encontró un usuario con el ID {userId}.",
                    };
                }

                if (usuario.IdEstado != 1)
                {
                    return new BaseResponseDto<UsuarioResponseDto>
                    {
                        Success = false,
                        Message = "Su cuenta se encuentra inactiva. Por favor, contacte al administrador del sistema.",
                    };
                }

                var usuarioDto = MapToResponseDto(usuario);

                return new BaseResponseDto<UsuarioResponseDto>
                {
                    Success = true,
                    Data = usuarioDto,
                    Message = "Datos del usuario obtenidos correctamente.",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos del usuario ID: {UserId}", userId);
                return new BaseResponseDto<UsuarioResponseDto>
                {
                    Success = false,
                    Message = "No se pudieron obtener sus datos. Por favor, intente nuevamente.",
                    Errors = new List<string>
                    {
                        "Error interno del servidor al procesar la solicitud.",
                    },
                };
            }
        }

        public async Task<BaseResponseDto<UsuarioResponseDto>> GetMyselfAsync(int userId, int tenantId)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdWithRolAndTenantAsync(userId, tenantId);

                if (usuario == null)
                {
                    return new BaseResponseDto<UsuarioResponseDto>
                    {
                        Success = false,
                        Message = "No se encontró el usuario en su organización.",
                    };
                }

                if (usuario.IdEstado != 1)
                {
                    return new BaseResponseDto<UsuarioResponseDto>
                    {
                        Success = false,
                        Message = "Su cuenta se encuentra inactiva. Por favor, contacte al administrador del sistema.",
                    };
                }

                var usuarioDto = MapToResponseDto(usuario);

                return new BaseResponseDto<UsuarioResponseDto>
                {
                    Success = true,
                    Data = usuarioDto,
                    Message = "Datos del usuario obtenidos correctamente.",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos del usuario ID: {UserId} en tenant: {TenantId}", userId, tenantId);
                return new BaseResponseDto<UsuarioResponseDto>
                {
                    Success = false,
                    Message = "No se pudieron obtener sus datos. Por favor, intente nuevamente.",
                    Errors = new List<string>
                    {
                        "Error interno del servidor al procesar la solicitud.",
                    },
                };
            }
        }

        public async Task<BaseResponseDto<object>> DeleteAsync(int id, int tenantId)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdWithRolAndTenantAsync(id, tenantId);
                if (usuario == null)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Usuario no encontrado en su organización."
                    };
                }

                // Verificar que el usuario no esté ya inactivo
                if (usuario.IdEstado == 3)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "El usuario ya se encuentra inactivo."
                    };
                }

                // Eliminación lógica: cambiar estado a 3 (inactivo)
                usuario.IdEstado = 3;
                usuario.ActualizadoEn = DateTime.Now;

                await _usuarioRepository.UpdateAsync(usuario);

                _logger.LogInformation("Usuario ID: {Id} marcado como inactivo en tenant: {TenantId}", id, tenantId);

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Message = "Usuario desvinculado correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desvincular usuario: {Id} en tenant: {TenantId}", id, tenantId);
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error al desvincular usuario.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<object>> ToggleEstadoAsync(int id, int tenantId)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdWithRolAndTenantAsync(id, tenantId);
                if (usuario == null)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Usuario no encontrado en su organización."
                    };
                }

                // Toggle entre activo (1) y bloqueado (2), no tocar inactivo (3)
                if (usuario.IdEstado == 3)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "No se puede cambiar el estado de un usuario inactivo. Debe reactivarlo primero."
                    };
                }

                usuario.IdEstado = usuario.IdEstado == 1 ? 2 : 1;
                usuario.ActualizadoEn = DateTime.UtcNow;

                await _usuarioRepository.UpdateAsync(usuario);

                string nuevoEstado = usuario.IdEstado switch
                {
                    1 => "activo",
                    2 => "bloqueado",
                    _ => "desconocido"
                };

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Message = $"Estado del usuario actualizado a {nuevoEstado}."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado del usuario: {Id} en tenant: {TenantId}", id, tenantId);
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error al cambiar estado.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        // Método adicional para reactivar usuario inactivo
        public async Task<BaseResponseDto<object>> ReactivateUserAsync(int id, int tenantId)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdWithRolAndTenantAsync(id, tenantId);
                if (usuario == null)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Usuario no encontrado en su organización."
                    };
                }

                if (usuario.IdEstado != 3)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Solo se pueden reactivar usuarios inactivos."
                    };
                }

                // Reactivar como activo
                usuario.IdEstado = 1;
                usuario.ActualizadoEn = DateTime.UtcNow;

                await _usuarioRepository.UpdateAsync(usuario);

                _logger.LogInformation("Usuario ID: {Id} reactivado en tenant: {TenantId}", id, tenantId);

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Message = "Usuario reactivado correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reactivar usuario: {Id} en tenant: {TenantId}", id, tenantId);
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error al reactivar usuario.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<object>> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto, int tenantId)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdWithRolAndTenantAsync(userId, tenantId);
                if (usuario == null)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Usuario no encontrado en su organización."
                    };
                }

                // Verificar que el usuario está activo
                if (usuario.IdEstado != 1)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Su cuenta se encuentra inactiva. No puede cambiar la contraseña."
                    };
                }

                // Validar contraseña actual
                var isCurrentPasswordValid = await _usuarioRepository.ValidateCredentialsAsync(usuario.Email, changePasswordDto.CurrentPassword);
                if (!isCurrentPasswordValid)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "La contraseña actual es incorrecta."
                    };
                }

                // Hashear la nueva contraseña
                usuario.HashContrasena = HashPassword(changePasswordDto.NewPassword);
                usuario.ActualizadoEn = DateTime.UtcNow;
                usuario.HashContrasena = HashPassword(changePasswordDto.NewPassword);
                await _usuarioRepository.UpdateAsync(usuario);

                _logger.LogInformation("Contraseña cambiada exitosamente para usuario ID: {UserId}", userId);

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Message = "Su contraseña ha sido cambiada exitosamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña para usuario ID: {UserId} en tenant: {TenantId}", userId, tenantId);
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "No se pudo cambiar la contraseña. Por favor, intente nuevamente.",
                    Errors = new List<string>
                    {
                        "Error interno del servidor al procesar la solicitud."
                    }
                };
            }
        }
    }
}