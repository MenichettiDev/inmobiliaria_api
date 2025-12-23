using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using inmobiliariaApi.DTOs.Lead;
using inmobiliariaApi.Services;
using inmobiliariaApi.Extensions;
using System.ComponentModel.DataAnnotations;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeadController : ControllerBase
    {
        private readonly LeadService _leadService;
        private readonly IUsoMensualService _usoMensualService;

        public LeadController(LeadService leadService, IUsoMensualService usoMensualService)
        {
            _leadService = leadService;
            _usoMensualService = usoMensualService;
        }

        /// <summary>
        /// Crear un nuevo lead desde formulario
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Store([FromBody] CreateLeadDto createLeadDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Datos de entrada no válidos",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            var usuarioId = User.GetUserId();
            if (!usuarioId.HasValue)
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "Usuario no autenticado"
                });
            }

            var response = await _leadService.CrearLeadDesdeFormularioAsync(createLeadDto, usuarioId.Value);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return CreatedAtAction(nameof(GetById), new { id = response.Data!.Id }, response);
        }

        /// <summary>
        /// Actualizar un lead existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLeadDto updateLeadDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Datos de entrada no válidos",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            var usuarioId = User.GetUserId();
            if (!usuarioId.HasValue)
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "Usuario no autenticado"
                });
            }

            var response = await _leadService.ActualizarLeadAsync(id, updateLeadDto, usuarioId.Value);

            if (!response.Success)
            {
                if (response.Message.Contains("no encontrado"))
                    return NotFound(response);
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Obtener lista de leads con filtros y paginación
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] LeadFiltrosDto filtros)
        {
            // Validar parámetros de paginación
            if (filtros.Page < 1)
                filtros.Page = 1;

            if (filtros.PageSize < 1 || filtros.PageSize > 100)
                filtros.PageSize = 10;

            var response = await _leadService.GetLeadsConFiltrosAsync(filtros);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Obtener un lead específico por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _leadService.GetByIdAsync(id);

            if (!response.Success)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Asignar lead a un usuario
        /// </summary>
        [HttpPost("{id}/asignar")]
        public async Task<IActionResult> AsignarUsuario(int id, [FromBody] AsignarUsuarioDto asignarDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Datos de entrada no válidos",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            var usuarioActualId = User.GetUserId();
            if (!usuarioActualId.HasValue)
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "Usuario no autenticado"
                });
            }

            var response = await _leadService.AsignarLeadAUsuarioAsync(id, asignarDto.IdUsuario, usuarioActualId.Value);

            if (!response.Success)
            {
                if (response.Message.Contains("no encontrado"))
                    return NotFound(response);
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Cambiar estado de un lead
        /// </summary>
        [HttpPost("{id}/cambiar-estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoLeadDto cambiarEstadoDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Datos de entrada no válidos",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            var usuarioId = User.GetUserId();
            if (!usuarioId.HasValue)
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "Usuario no autenticado"
                });
            }

            var response = await _leadService.ActualizarEstadoConHistorialAsync(
                id,
                cambiarEstadoDto.IdNuevoEstado,
                usuarioId.Value,
                cambiarEstadoDto.Comentario);

            if (!response.Success)
            {
                if (response.Message.Contains("no encontrado"))
                    return NotFound(response);
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Eliminar (marcar como eliminado) un lead
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var usuarioId = User.GetUserId();
            if (!usuarioId.HasValue)
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "Usuario no autenticado"
                });
            }

            var response = await _leadService.EliminarLeadAsync(id, usuarioId.Value);

            if (!response.Success)
            {
                if (response.Message.Contains("no encontrado"))
                    return NotFound(response);
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Obtener estadísticas de uso de leads para el mes actual
        /// </summary>
        [HttpGet("estadisticas/uso-mensual")]
        public async Task<IActionResult> GetUsoMensual()
        {
            try
            {
                var (usados, limite) = await _usoMensualService.GetUsoLeadsActualAsync();
                var porcentaje = limite > 0 ? (double)usados / limite * 100 : 0;

                return Ok(new
                {
                    Success = true,
                    Data = new
                    {
                        LeadsUsados = usados,
                        LimiteLeads = limite,
                        PorcentajeUso = Math.Round(porcentaje, 2),
                        LeadsDisponibles = Math.Max(0, limite - usados)
                    },
                    Message = "Estadísticas obtenidas exitosamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Error al obtener estadísticas de uso",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }

    // DTO para asignación de usuario
    public class AsignarUsuarioDto
    {
        [Required(ErrorMessage = "El ID del usuario es obligatorio")]
        public int IdUsuario { get; set; }
    }
}