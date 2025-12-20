using System; // agregado
using System.Linq; // agregado
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using pyreApi.DTOs.Common;
using pyreApi.DTOs.Usuario;
using pyreApi.Models;
using pyreApi.Repositories;

namespace pyreApi.Services
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
            // Salt fijo (a modo de aprendizaje)
            string salt = _configuration["Salt"] ?? string.Empty; // Asegura que no sea nulo
            if (string.IsNullOrEmpty(salt))
            {
                throw new InvalidOperationException(
                    "El valor de 'Salt' no está configurado en appsettings.json."
                );
            }

            // Hashear la contraseña usando el salt fijo
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
                Nombre = usuario.Nombre ?? string.Empty,
                Apellido = usuario.Apellido,
                Legajo = usuario.Legajo,
                Dni = usuario.Dni,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                AccedeAlSistema = usuario.AccedeAlSistema,
                Activo = usuario.Activo,
                Avatar = usuario.Avatar,
                FechaRegistro = usuario.FechaRegistro,
                FechaModificacion = usuario.FechaModificacion ?? DateTime.MinValue, // Manejo explícito de nulos
                RolNombre = usuario.Rol?.NombreRol ?? string.Empty, // Asegurarse de incluir el nombre del rol
            };
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
                    Message =
                        $"Error al buscar el usuario con ID {id}. Por favor, intente nuevamente.",
                    Errors = new List<string>
                    {
                        "Error interno del servidor al procesar la solicitud.",
                    },
                };
            }
        }

        public async Task<BaseResponseDto<Usuario>> CreateUsuarioAsync(CreateUsuarioDto createDto)
        {
            try
            {
                // Validar longitud del legajo
                if (!string.IsNullOrEmpty(createDto.Legajo) && createDto.Legajo.Length > 5)
                {
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message =
                            "El legajo no puede tener más de 5 caracteres. Por favor, ingrese un legajo válido.",
                    };
                }

                // Validar si el DNI ya existe
                var existingUser = await _usuarioRepository.GetByDniAsync(createDto.Dni);
                if (existingUser != null)
                {
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message =
                            $"Ya existe un usuario registrado con el DNI {createDto.Dni}. Por favor, verifique los datos ingresados.",
                    };
                }

                // Validar si el email ya existe
                if (!string.IsNullOrEmpty(createDto.Email))
                {
                    var existingEmail = await _usuarioRepository.GetByEmailAsync(createDto.Email);
                    if (existingEmail != null)
                    {
                        return new BaseResponseDto<Usuario>
                        {
                            Success = false,
                            Message =
                                $"Ya existe un usuario registrado con el email {createDto.Email}. Por favor, use un email diferente.",
                        };
                    }
                }

                // Validar si el legajo ya existe
                if (!string.IsNullOrEmpty(createDto.Legajo))
                {
                    var existingLegajo = await _usuarioRepository.GetByLegajoAsync(
                        createDto.Legajo
                    );
                    if (existingLegajo != null)
                    {
                        return new BaseResponseDto<Usuario>
                        {
                            Success = false,
                            Message =
                                $"Ya existe un usuario registrado con el legajo {createDto.Legajo}. Por favor, use un legajo diferente.",
                        };
                    }
                }

                var usuario = new Usuario
                {
                    Nombre = createDto.Nombre,
                    Apellido = createDto.Apellido,
                    Legajo = createDto.Legajo,
                    Dni = createDto.Dni,
                    Email = createDto.Email,
                    Telefono = createDto.Telefono,
                    RolId = createDto.RolId,
                    AccedeAlSistema = true,
                    Avatar = "default.png",
                    IdUsuarioCrea = createDto.IdUsuarioCrea,
                    FechaRegistro = DateTime.Now,
                    FechaModificacion = DateTime.Now,
                    Activo = true,
                };

                // Si el usuario accede al sistema, hashear la contraseña proporcionada
                if (usuario.AccedeAlSistema && !string.IsNullOrEmpty(createDto.Password))
                {
                    usuario.PasswordHash = HashPassword(createDto.Password);
                }

                var result = await _usuarioRepository.AddAsync(usuario);
                return new BaseResponseDto<Usuario>
                {
                    Success = true,
                    Data = result,
                    Message =
                        $"El usuario {createDto.Nombre} {createDto.Apellido} ha sido creado exitosamente.",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario: {Message}", ex.Message);
                return new BaseResponseDto<Usuario>
                {
                    Success = false,
                    Message =
                        "No se pudo crear el usuario. Por favor, verifique los datos ingresados e intente nuevamente.",
                    Errors = new List<string>
                    {
                        "Error interno del servidor al procesar la solicitud.",
                    },
                };
            }
        }

        public async Task<BaseResponseDto<UsuarioResponseDto>> UpdateUsuarioAsync(
            UpdateUsuarioDto updateDto
        )
        {
            try
            {
                var existingUser = await _usuarioRepository.GetByIdWithRolAsync(updateDto.Id);
                if (existingUser == null)
                {
                    return new BaseResponseDto<UsuarioResponseDto>
                    {
                        Success = false,
                        Message =
                            $"No se encontró un usuario con el ID {updateDto.Id} para actualizar.",
                    };
                }

                var modifierId = updateDto.IdUsuarioModifica;

                // 2️ Un SuperAdmin no puede darse de baja a sí mismo
                if (
                    modifierId == existingUser.Id
                    && updateDto.AccedeAlSistema.HasValue
                    && updateDto.AccedeAlSistema.Value == false
                )
                {
                    // Necesitamos cargar la relación Rol si no está cargada
                    if (existingUser.Rol == null)
                    {
                        var reloadedUser = await _usuarioRepository.GetByIdWithRolAsync(
                            existingUser.Id
                        );
                        if (reloadedUser != null)
                        {
                            existingUser = reloadedUser;
                        }
                    }

                    if (existingUser.Rol?.NombreRol != null)
                    {
                        var rolNombre = existingUser.Rol.NombreRol;
                        if (
                            string.Equals(
                                rolNombre,
                                "superadmin",
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                        {
                            return new BaseResponseDto<UsuarioResponseDto>
                            {
                                Success = false,
                                Message = "Un SuperAdmin no puede darse de baja a sí mismo.",
                            };
                        }
                    }
                }

                // 3️ Validar email único si se modifica
                if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != existingUser.Email)
                {
                    var existingEmail = await _usuarioRepository.GetByEmailAsync(updateDto.Email);
                    if (existingEmail != null)
                    {
                        return new BaseResponseDto<UsuarioResponseDto>
                        {
                            Success = false,
                            Message =
                                $"Ya existe otro usuario registrado con el email {updateDto.Email}. Por favor, use un email diferente.",
                        };
                    }
                }

                // 4️ Validar legajo único si se modifica
                if (
                    !string.IsNullOrEmpty(updateDto.Legajo)
                    && updateDto.Legajo != existingUser.Legajo
                )
                {
                    if (updateDto.Legajo.Length > 5)
                    {
                        return new BaseResponseDto<UsuarioResponseDto>
                        {
                            Success = false,
                            Message =
                                "El legajo no puede tener más de 5 caracteres. Por favor, ingrese un legajo válido.",
                        };
                    }

                    var existingLegajo = await _usuarioRepository.GetByLegajoAsync(
                        updateDto.Legajo
                    );
                    if (existingLegajo != null)
                    {
                        return new BaseResponseDto<UsuarioResponseDto>
                        {
                            Success = false,
                            Message =
                                $"Ya existe otro usuario registrado con el legajo {updateDto.Legajo}. Por favor, use un legajo diferente.",
                        };
                    }

                    existingUser.Legajo = updateDto.Legajo;
                }

                // 5️ Actualizar campos básicos
                if (!string.IsNullOrEmpty(updateDto.Nombre))
                    existingUser.Nombre = updateDto.Nombre;

                if (updateDto.Apellido != null)
                    existingUser.Apellido = updateDto.Apellido;

                if (updateDto.Email != null)
                    existingUser.Email = updateDto.Email;

                if (updateDto.Telefono != null)
                    existingUser.Telefono = updateDto.Telefono;

                // Solo actualizar el rol si realmente cambió Y no es el mismo usuario
                if (updateDto.RolId.HasValue && existingUser.RolId != updateDto.RolId.Value)
                {
                    if (modifierId == existingUser.Id)
                    {
                        return new BaseResponseDto<UsuarioResponseDto>
                        {
                            Success = false,
                            Message = "No está permitido que un usuario cambie su propio rol.",
                        };
                    }
                    existingUser.RolId = updateDto.RolId.Value;
                }

                // Verificación de seguridad para evitar advertencias de null reference
                if (existingUser == null)
                {
                    return new BaseResponseDto<UsuarioResponseDto>
                    {
                        Success = false,
                        Message = "Error inesperado: no se pudo cargar el usuario.",
                    };
                }

                if (updateDto.AccedeAlSistema.HasValue)
                    existingUser.AccedeAlSistema = updateDto.AccedeAlSistema.Value;

                if (updateDto.Avatar != null)
                    existingUser.Avatar = updateDto.Avatar;

                // Actualizar contraseña si se envía en el DTO
                if (!string.IsNullOrEmpty(updateDto.Password))
                {
                    // Solo hasheamos y guardamos la contraseña si el usuario puede acceder al sistema
                    if (existingUser.AccedeAlSistema)
                    {
                        existingUser.PasswordHash = HashPassword(updateDto.Password);
                    }
                    else
                    {
                        // Si se envía contraseña pero el usuario no tiene acceso, la ignoramos y logueamos
                        _logger.LogWarning(
                            "Se recibió una contraseña para el usuario {Id} pero 'AccedeAlSistema' es false. Ignorando cambio de contraseña.",
                            existingUser.Id
                        );
                    }
                }
                existingUser.IdUsuarioModifica = updateDto.IdUsuarioModifica;
                existingUser.FechaModificacion = DateTime.Now;

                await _usuarioRepository.UpdateAsync(existingUser);

                // Cargar el usuario actualizado con sus relaciones para el DTO de respuesta
                var updatedUser = await _usuarioRepository.GetByIdWithRolAsync(existingUser.Id);
                var responseDto = MapToResponseDto(updatedUser ?? existingUser);

                return new BaseResponseDto<UsuarioResponseDto>
                {
                    Success = true,
                    Data = responseDto,
                    Message =
                        $"Los datos del usuario {existingUser.Nombre} {existingUser.Apellido} han sido actualizados correctamente.",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario: {Id}", updateDto.Id);
                return new BaseResponseDto<UsuarioResponseDto>
                {
                    Success = false,
                    Message =
                        "No se pudieron actualizar los datos del usuario. Por favor, intente nuevamente.",
                    Errors = new List<string>
                    {
                        "Error interno del servidor al procesar la solicitud.",
                    },
                };
            }
        }

        public async Task<BaseResponseDto<Usuario>> GetByDniAsync(string dni)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByDniAsync(dni);
                if (usuario == null)
                {
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message = $"No se encontró un usuario con el DNI {dni}.",
                    };
                }

                return new BaseResponseDto<Usuario>
                {
                    Success = true,
                    Data = usuario,
                    Message = "Usuario encontrado correctamente.",
                };
            }
            catch (Exception)
            {
                return new BaseResponseDto<Usuario>
                {
                    Success = false,
                    Message =
                        $"Error al buscar el usuario con DNI {dni}. Por favor, intente nuevamente.",
                    Errors = new List<string>
                    {
                        "Error interno del servidor al procesar la solicitud.",
                    },
                };
            }
        }

        public async Task<BaseResponseDto<Usuario>> GetByLegajoAsync(string legajo)
        {
            try
            {
                // Validar longitud del legajo
                if (!string.IsNullOrEmpty(legajo) && legajo.Length > 5)
                {
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message =
                            "El legajo no puede tener más de 5 caracteres. Por favor, ingrese un legajo válido.",
                    };
                }

                var usuario = await _usuarioRepository.GetByLegajoAsync(legajo);
                if (usuario == null)
                {
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message = $"No se encontró un usuario con el legajo {legajo}.",
                    };
                }

                return new BaseResponseDto<Usuario>
                {
                    Success = true,
                    Data = usuario,
                    Message = "Usuario encontrado correctamente.",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<Usuario>
                {
                    Success = false,
                    Message =
                        $"Error al buscar el usuario con legajo {legajo}. Por favor, intente nuevamente.",
                    Errors = new List<string>
                    {
                        "Error interno del servidor al procesar la solicitud." + ex.Message,
                    },
                };
            }
        }

        public async Task<BaseResponseDto<bool>> ValidateCredentialsAsync(
            string legajo,
            string password
        )
        {
            try
            {
                // Validar longitud del legajo
                if (!string.IsNullOrEmpty(legajo) && legajo.Length > 5)
                {
                    return new BaseResponseDto<bool>
                    {
                        Success = false,
                        Message =
                            "El legajo no puede tener más de 5 caracteres. Por favor, ingrese un legajo válido.",
                    };
                }

                var isValid = await _usuarioRepository.ValidateCredentialsAsync(legajo, password);
                return new BaseResponseDto<bool>
                {
                    Success = true,
                    Data = isValid,
                    Message = isValid
                        ? "Las credenciales son válidas."
                        : "El legajo o la contraseña son incorrectos.",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<bool>
                {
                    Success = false,
                    Message =
                        "No se pudieron validar las credenciales. Por favor, intente nuevamente.",
                    Errors = new List<string>
                    {
                        "Error interno del servidor al procesar la solicitud." + ex.Message,
                    },
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<Usuario>>> GetActiveUsersAsync()
        {
            try
            {
                var users = await _usuarioRepository.GetActiveUsersAsync();
                return new BaseResponseDto<IEnumerable<Usuario>>
                {
                    Success = true,
                    Data = users,
                    Message = "Usuarios activos obtenidos correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<IEnumerable<Usuario>>
                {
                    Success = false,
                    Message =
                        "No se pudieron cargar los usuarios activos. Por favor, intente nuevamente.",
                    Errors = new List<string>
                    {
                        "Error interno del servidor al procesar la solicitud." + ex.Message,
                    },
                };
            }
        }

        public async Task<BaseResponseDto<Usuario>> AuthenticateAsync(
            string legajo,
            string password
        )
        {
            try
            {
                // Validar longitud del legajo
                if (!string.IsNullOrEmpty(legajo) && legajo.Length > 5)
                {
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message =
                            "El legajo no puede tener más de 5 caracteres. Por favor, ingrese un legajo válido.",
                    };
                }

                _logger.LogInformation("Iniciando autenticación para legajo: {Legajo}", legajo);

                var usuario = await _usuarioRepository.GetByLegajoWithRolAsync(legajo);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado con legajo: {Legajo}", legajo);
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message =
                            "Las credenciales ingresadas son incorrectas. Por favor, verifique su legajo y contraseña.",
                    };
                }

                _logger.LogInformation(
                    "Usuario encontrado: {UsuarioId}, Nombre: {Nombre}, Activo: {Activo}, AccedeAlSistema: {AccedeAlSistema}",
                    usuario.Id,
                    usuario.Nombre,
                    usuario.Activo,
                    usuario.AccedeAlSistema
                );

                // Check if user is active and has system access
                if (!usuario.Activo)
                {
                    _logger.LogWarning("Usuario inactivo para legajo: {Legajo}", legajo);
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message =
                            "Su cuenta se encuentra inactiva. Por favor, contacte al administrador del sistema para más información.",
                    };
                }

                if (!usuario.AccedeAlSistema)
                {
                    _logger.LogWarning(
                        "Usuario sin acceso al sistema para legajo: {Legajo}",
                        legajo
                    );
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message =
                            "Su cuenta no tiene permisos para acceder al sistema. Por favor, contacte al administrador.",
                    };
                }

                _logger.LogInformation("Validando credenciales para legajo: {Legajo}", legajo);

                var isValidPassword = await _usuarioRepository.ValidateCredentialsAsync(
                    legajo,
                    password
                );

                _logger.LogInformation(
                    "Resultado validación de credenciales para legajo {Legajo}: {IsValid}",
                    legajo,
                    isValidPassword
                );

                if (!isValidPassword)
                {
                    _logger.LogWarning("Contraseña incorrecta para legajo: {Legajo}", legajo);
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message =
                            "Las credenciales ingresadas son incorrectas. Por favor, verifique su legajo y contraseña.",
                    };
                }

                _logger.LogInformation("Autenticación exitosa para legajo: {Legajo}", legajo);

                return new BaseResponseDto<Usuario>
                {
                    Success = true,
                    Data = usuario,
                    Message = $"¡Bienvenido/a {usuario.Nombre}! Ha iniciado sesión correctamente.",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error durante la autenticación para legajo: {Legajo}",
                    legajo
                );
                return new BaseResponseDto<Usuario>
                {
                    Success = false,
                    Message =
                        "No se pudo completar el proceso de autenticación. Por favor, intente nuevamente.",
                    Errors = new List<string>
                    {
                        "Error interno del servidor al procesar la solicitud.",
                    },
                };
            }
        }

        public async Task<
            BaseResponseDto<PaginatedResponseDto<UsuarioResponseDto>>
        > GetAllUsuariosPaginatedAsync(
            int page,
            int pageSize,
            string? legajo = null,
            bool? estado = null,
            string? nombre = null,
            string? apellido = null,
            int? rolId = null
        )
        {
            try
            {
                if (page <= 0)
                    page = 1;
                if (pageSize <= 0)
                    pageSize = 10;

                // Obtener usuarios con filtros aplicados directamente en la base de datos
                var usuarios = await _usuarioRepository.GetFilteredUsuariosAsync(
                    legajo,
                    estado,
                    nombre,
                    apellido,
                    rolId
                );

                // Ordenar por Id antes de aplicar la paginación
                var usuariosOrdenados = usuarios.OrderBy(u => u.Id);

                var totalRecords = usuariosOrdenados.Count();

                var usuariosPage = usuariosOrdenados
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var usuariosDto = usuariosPage.Select(MapToResponseDto).ToList();
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
                _logger.LogError(ex, "Error al obtener usuarios paginados");
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

        // Cambiar la firma del método para indicar ocultación intencional del miembro base
        public new async Task<BaseResponseDto<Usuario>> ToggleActivoAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message = "El ID del usuario debe ser un número válido mayor a 0.",
                    };
                }

                var existingUser = await _usuarioRepository.GetByIdAsync(id);
                if (existingUser == null)
                {
                    return new BaseResponseDto<Usuario>
                    {
                        Success = false,
                        Message = $"No se encontró un usuario con el ID {id}.",
                    };
                }

                existingUser.Activo = !existingUser.Activo;
                existingUser.FechaModificacion = DateTime.Now;

                await _usuarioRepository.UpdateAsync(existingUser);

                return new BaseResponseDto<Usuario>
                {
                    Success = true,
                    Data = existingUser,
                    Message = existingUser.Activo
                        ? "Usuario activado correctamente."
                        : "Usuario desactivado correctamente.",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar el estado activo del usuario: {Id}", id);
                return new BaseResponseDto<Usuario>
                {
                    Success = false,
                    Message =
                        "No se pudo cambiar el estado del usuario. Por favor, intente nuevamente.",
                    Errors = new List<string>
                    {
                        "Error interno del servidor al procesar la solicitud.",
                    },
                };
            }
        }

        public async Task<BaseResponseDto<object>> DeleteAsyncLogico(int id)
        {
            try
            {
                var usuario = await _repository.GetByIdAsync(id);
                if (usuario == null || usuario.Eliminado)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Usuario no encontrado",
                    };
                }

                // Eliminación lógica
                usuario.Eliminado = true;
                usuario.FechaModificacion = DateTime.Now;
                await _repository.UpdateAsync(usuario);

                return new BaseResponseDto<object>
                {
                    Success = true,
                    Message = "Usuario eliminado correctamente",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<object>
                {
                    Success = false,
                    Message = "Error al eliminar el usuario",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<BaseResponseDto<object>> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                // Verificar que el usuario existe
                var usuario = await _usuarioRepository.GetByIdAsync(userId);
                if (usuario == null)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Usuario no encontrado."
                    };
                }

                // Verificar que el usuario está activo y puede acceder al sistema
                if (!usuario.Activo)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Su cuenta se encuentra inactiva. No puede cambiar la contraseña."
                    };
                }

                if (!usuario.AccedeAlSistema)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "Su cuenta no tiene permisos para acceder al sistema."
                    };
                }

                // Validar contraseña actual
                var isCurrentPasswordValid = await _usuarioRepository.ValidateUserPasswordAsync(userId, changePasswordDto.CurrentPassword);
                if (!isCurrentPasswordValid)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "La contraseña actual es incorrecta."
                    };
                }

                // Verificar que la nueva contraseña no sea igual a la actual
                var isSamePassword = await _usuarioRepository.ValidateUserPasswordAsync(userId, changePasswordDto.NewPassword);
                if (isSamePassword)
                {
                    return new BaseResponseDto<object>
                    {
                        Success = false,
                        Message = "La nueva contraseña debe ser diferente a la contraseña actual."
                    };
                }

                // Hashear la nueva contraseña
                usuario.PasswordHash = HashPassword(changePasswordDto.NewPassword);
                usuario.FechaModificacion = DateTime.Now;
                usuario.IdUsuarioModifica = userId; // El usuario se modifica a sí mismo

                await _usuarioRepository.UpdateAsync(usuario);

                _logger.LogInformation("Password cambiado exitosamente para usuario ID: {UserId}", userId);

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

                if (!usuario.Activo)
                {
                    return new BaseResponseDto<UsuarioResponseDto>
                    {
                        Success = false,
                        Message = "Su cuenta se encuentra inactiva. Por favor, contacte al administrador del sistema.",
                    };
                }

                if (!usuario.AccedeAlSistema)
                {
                    return new BaseResponseDto<UsuarioResponseDto>
                    {
                        Success = false,
                        Message = "Su cuenta no tiene permisos para acceder al sistema.",
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
                _logger.LogError(ex, "Error al obtener datos propios del usuario ID: {UserId}", userId);
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
    }
}
