using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pyreApi.DTOs.Usuario;
using pyreApi.Services;

namespace pyreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación para todo el controller
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;

        public UsuarioController(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede ver todos los usuarios
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? legajo = null,
            [FromQuery] bool? estado = null,
            [FromQuery] string? nombre = null,
            [FromQuery] string? apellido = null,
            [FromQuery(Name = "rol")] int? rolId = null
        )
        {
            // Validar longitud del legajo si se proporcionó
            if (!string.IsNullOrWhiteSpace(legajo) && legajo.Length > 5)
            {
                return BadRequest(
                    new
                    {
                        Success = false,
                        Message = "El legajo no puede tener más de 5 caracteres.",
                        Errors = new List<string>
                        {
                            "Legajo excede la longitud máxima permitida (5 caracteres).",
                        },
                    }
                );
            }

            var response = await _usuarioService.GetAllUsuariosPaginatedAsync(
                page,
                pageSize,
                legajo,
                estado,
                nombre,
                apellido,
                rolId
            );
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("all-unpaginated")]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede ver todos los usuarios
        public async Task<IActionResult> GetAllUnpaginated()
        {
            var response = await _usuarioService.GetAllUsuariosAsync();
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede ver usuarios específicos
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    new
                    {
                        Success = false,
                        Message = "El ID del usuario debe ser un número válido mayor a 0.",
                    }
                );
            }

            var response = await _usuarioService.GetUsuarioByIdAsync(id);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }
        [HttpGet("{id}/myself")]
        [Authorize]
        public async Task<IActionResult> GetMyself(int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    new
                    {
                        Success = false,
                        Message = "El ID del usuario debe ser un número válido mayor a 0.",
                    }
                );
            }

            // Obtener el ID del usuario autenticado
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int authenticatedUserId))
            {
                return Unauthorized(
                    new
                    {
                        Success = false,
                        Message = "No se pudo identificar al usuario autenticado. Por favor, inicie sesión nuevamente."
                    }
                );
            }

            // Validar que el usuario solo pueda acceder a sus propios datos
            if (authenticatedUserId != id)
            {
                return Forbid();
            }

            var response = await _usuarioService.GetMyselfAsync(id);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        [HttpGet("dni/{dni}")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor")] // SuperAdmin, Administrador y Supervisor
        public async Task<IActionResult> GetByDni(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni))
            {
                return BadRequest(
                    new { Success = false, Message = "El DNI es requerido y no puede estar vacío." }
                );
            }

            var response = await _usuarioService.GetByDniAsync(dni);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        [HttpGet("active")]
        [Authorize(Roles = "SuperAdmin,Administrador,Supervisor,Operario")] // Todos los roles pueden ver usuarios activos
        public async Task<IActionResult> GetActiveUsers()
        {
            var response = await _usuarioService.GetActiveUsersAsync();
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede crear usuarios
        public async Task<IActionResult> Create([FromBody] CreateUsuarioDto createDto)
        {
            // Validar longitud del legajo
            if (!string.IsNullOrEmpty(createDto.Legajo) && createDto.Legajo.Length > 5)
            {
                return BadRequest(
                    new
                    {
                        Success = false,
                        Message = "El legajo no puede tener más de 5 caracteres.",
                        Errors = new List<string>
                        {
                            "Legajo excede la longitud máxima permitida (5 caracteres).",
                        },
                    }
                );
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(
                    new
                    {
                        Success = false,
                        Message = "Los datos proporcionados no son válidos. Por favor, revise la información ingresada.",
                        Errors = errors,
                    }
                );
            }

            var response = await _usuarioService.CreateUsuarioAsync(createDto);
            if (response.Success)
                return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
            return BadRequest(response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede actualizar usuarios
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUsuarioDto updateDto)
        {
            if (id <= 0)
            {
                return BadRequest(
                    new
                    {
                        Success = false,
                        Message = "El ID del usuario debe ser un número válido mayor a 0.",
                    }
                );
            }

            // Obtener el ID del usuario autenticado
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            int idUsuarioModifica = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
            updateDto.IdUsuarioModifica = idUsuarioModifica;

            // La validación del cambio de rol se maneja en el servicio

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(
                    new
                    {
                        Success = false,
                        Message = "Los datos proporcionados para la actualización no son válidos. Por favor, revise la información ingresada.",
                        Errors = errors,
                    }
                );
            }

            if (id != updateDto.Id)
                return BadRequest(
                    new
                    {
                        Success = false,
                        Message = "El ID proporcionado en la URL no coincide con el ID del usuario a actualizar.",
                    }
                );

            updateDto.IdUsuarioModifica = idUsuarioModifica;

            var response = await _usuarioService.UpdateUsuarioAsync(updateDto);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpPost("validate")]
        [AllowAnonymous] // Permitir acceso público para autenticación
        public async Task<IActionResult> ValidateCredentials(
            [FromBody] LoginRequestDto loginRequest
        )
        {
            if (string.IsNullOrWhiteSpace(loginRequest.Legajo))
            {
                return BadRequest(
                    new
                    {
                        Success = false,
                        Message = "El legajo es requerido para la validación de credenciales.",
                    }
                );
            }

            if (loginRequest.Legajo.Length > 5)
            {
                return BadRequest(
                    new
                    {
                        Success = false,
                        Message = "El legajo no puede tener más de 5 caracteres.",
                    }
                );
            }

            if (string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                return BadRequest(
                    new
                    {
                        Success = false,
                        Message = "La contraseña es requerida para la validación de credenciales.",
                    }
                );
            }

            var response = await _usuarioService.ValidateCredentialsAsync(
                loginRequest.Legajo,
                loginRequest.Password
            );
            if (response.Success)
                return Ok(response);
            return Unauthorized(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede eliminar usuarios
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    new
                    {
                        Success = false,
                        Message = "El ID del usuario debe ser un número válido mayor a 0.",
                    }
                );
            }

            // Obtener el ID del usuario autenticado
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            int idUsuarioActual = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;

            // Evitar que el usuario se elimine a sí mismo
            if (idUsuarioActual == id)
            {
                return BadRequest(
                    new
                    {
                        Success = false,
                        Message = "No está permitido que un usuario se elimine a sí mismo.",
                    }
                );
            }

            var response = await _usuarioService.DeleteAsyncLogico(id);
            if (response.Success)
                return Ok(response);

            if (response.Message?.Contains("no encontrado") == true)
                return NotFound(response);

            return BadRequest(response);
        }

        [HttpPatch("{id}/toggle-activo")]
        [Authorize(Roles = "SuperAdmin")] // Solo SuperAdmin puede cambiar estado activo
        public async Task<IActionResult> ToggleActivo(int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    new
                    {
                        Success = false,
                        Message = "El ID del usuario debe ser un número válido mayor a 0.",
                    }
                );
            }

            // Obtener el ID del usuario autenticado
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            int idUsuarioActual = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;

            // Evitar que el usuario cambie su propio estado activo
            if (idUsuarioActual == id)
            {
                return BadRequest(
                    new
                    {
                        Success = false,
                        Message = "No está permitido que un usuario cambie su propio estado activo.",
                    }
                );
            }

            var response = await _usuarioService.ToggleActivoAsync(id);
            if (response.Success)
                return Ok(response);

            if (
                response.Message?.Contains("No se encontró") == true
                || response.Message?.Contains("no encontrado") == true
            )
                return NotFound(response);

            return BadRequest(response);
        }

        [HttpPost("change-password")]
        [Authorize] // Solo requiere autenticación, cualquier rol puede cambiar su propia contraseña
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(
                    new
                    {
                        Success = false,
                        Message = "Los datos proporcionados no son válidos. Por favor, revise la información ingresada.",
                        Errors = errors,
                    }
                );
            }

            // Obtener el ID del usuario autenticado
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(
                    new
                    {
                        Success = false,
                        Message = "No se pudo identificar al usuario. Por favor, inicie sesión nuevamente."
                    }
                );
            }

            var response = await _usuarioService.ChangePasswordAsync(userId, changePasswordDto);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }
    }

    // Clase única para login por legajo
    public class LoginRequestDto
    {
        public string Legajo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
