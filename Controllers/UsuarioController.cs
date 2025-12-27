using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using inmobiliariaApi.DTOs.Usuario;
using inmobiliariaApi.Services;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;

        public UsuarioController(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        private int GetTenantId()
        {
            var tenantClaim = User.FindFirst("IdInmobiliaria");
            return tenantClaim != null ? int.Parse(tenantClaim.Value) : 0;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? nombre = null,
            [FromQuery] int? rolId = null,
            [FromQuery] int? estadoId = null
        )
        {
            var tenantId = GetTenantId();

            // Log para debugging
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userId = GetUserId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            var response = await _usuarioService.GetAllUsuariosPaginatedAsync(
                page,
                pageSize,
                nombre,
                rolId,
                tenantId, // Pasar tenantId como inmobiliariaId
                estadoId
            );
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "El ID del usuario debe ser un número válido mayor a 0." });
            }

            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            var response = await _usuarioService.GetUsuarioByIdAndTenantAsync(id, tenantId);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyself()
        {
            var userId = GetUserId();
            var tenantId = GetTenantId();

            if (userId <= 0 || tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Usuario o tenant no válido." });
            }

            var response = await _usuarioService.GetMyselfAsync(userId, tenantId);
            if (response.Success)
                return Ok(response);
            return NotFound(response);
        }

        [HttpGet("active")]
        [Authorize(Roles = "Administrador,Supervisor,Operario")]
        public async Task<IActionResult> GetActiveUsers()
        {
            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            var response = await _usuarioService.GetActiveUsuariosByTenantAsync(tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create([FromBody] CreateUsuarioDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Success = false, Message = "Datos no válidos.", Errors = errors });
            }

            var tenantId = GetTenantId();

            if (tenantId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Tenant no válido." });
            }

            // Validaciones adicionales
            if (createDto.IdEstado <= 0)
            {
                createDto.IdEstado = 1; // Por defecto activo
            }

            if (createDto.IdEstado < 1 || createDto.IdEstado > 3)
            {
                return BadRequest(new { Success = false, Message = "El estado debe ser 1 (activo), 2 (bloqueado) o 3 (inactivo)." });
            }

            if (createDto.IdRol <= 0)
            {
                return BadRequest(new { Success = false, Message = "Debe especificar un rol válido." });
            }

            if (string.IsNullOrWhiteSpace(createDto.Password))
            {
                return BadRequest(new { Success = false, Message = "La contraseña es obligatoria." });
            }

            // Asegurar que se cree en el tenant correcto
            createDto.IdInmobiliaria = tenantId;

            var response = await _usuarioService.CreateUsuarioAsync(createDto);
            if (response.Success)
                return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
            return BadRequest(response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUsuarioDto updateDto)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Success = false, Message = "Datos no válidos.", Errors = errors });
            }

            if (id != updateDto.Id)
                return BadRequest(new { Success = false, Message = "ID no coincide." });

            var tenantId = GetTenantId();
            var userId = GetUserId();

            // Prevenir auto-modificación
            if (userId == id)
            {
                return BadRequest(new { Success = false, Message = "No puede modificar su propio usuario." });
            }

            var response = await _usuarioService.UpdateUsuarioAsync(updateDto, tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateCredentials([FromBody] LoginRequestDto loginRequest)
        {
            if (string.IsNullOrWhiteSpace(loginRequest.Email) || string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                return BadRequest(new { Success = false, Message = "Email y contraseña son requeridos." });
            }

            var response = await _usuarioService.AuthenticateAsync(loginRequest.Email, loginRequest.Password);
            if (response.Success)
                return Ok(response);
            return Unauthorized(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            var tenantId = GetTenantId();
            var userId = GetUserId();

            // Prevenir auto-eliminación
            if (userId == id)
            {
                return BadRequest(new { Success = false, Message = "No puede desvincular su propio usuario." });
            }

            var response = await _usuarioService.DeleteAsync(id, tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpPatch("{id}/toggle-estado")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ToggleEstado(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            var tenantId = GetTenantId();
            var userId = GetUserId();

            // Prevenir auto-modificación de estado
            if (userId == id)
            {
                return BadRequest(new { Success = false, Message = "No puede cambiar su propio estado." });
            }

            var response = await _usuarioService.ToggleEstadoAsync(id, tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpPost("{id}/reactivate")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ReactivateUser(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            var tenantId = GetTenantId();

            var response = await _usuarioService.ReactivateUserAsync(id, tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Success = false, Message = "Datos no válidos.", Errors = errors });
            }

            var userId = GetUserId();
            var tenantId = GetTenantId();

            var response = await _usuarioService.ChangePasswordAsync(userId, changePasswordDto, tenantId);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }
    }

    // Clase para login por email
    public class LoginRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
