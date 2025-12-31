using Microsoft.Extensions.Logging;
using inmobiliariaApi.DTOs.Common;
using inmobiliariaApi.DTOs.LeadEstadoHistorial;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;

namespace inmobiliariaApi.Services
{
    public class LeadEstadoHistorialService : GenericService<LeadEstadoHistorial>
    {
        private readonly LeadEstadoHistorialRepository _historialRepository;
        private readonly ILogger<LeadEstadoHistorialService> _logger;

        public LeadEstadoHistorialService(LeadEstadoHistorialRepository historialRepository, ILogger<LeadEstadoHistorialService> logger)
            : base(historialRepository)
        {
            _historialRepository = historialRepository;
            _logger = logger;
        }

        private LeadEstadoHistorialDto MapToResponseDto(LeadEstadoHistorial historial)
        {
            return new LeadEstadoHistorialDto
            {
                Id = historial.Id,
                IdLead = historial.IdLead,
                IdEstadoAnterior = historial.IdEstadoAnterior,
                IdEstadoNuevo = historial.IdEstadoNuevo,
                IdUsuario = historial.IdUsuario,
                Comentario = historial.Comentario,
                CreadoEn = historial.CreadoEn,
                EstadoAnteriorNombre = historial.EstadoAnterior?.Nombre,
                EstadoNuevoNombre = historial.EstadoNuevo?.Nombre,
                UsuarioNombre = historial.Usuario?.Nombre,
                LeadNombre = historial.Lead?.NombreCompleto
            };
        }

        public async Task<BaseResponseDto<PaginatedResponseDto<LeadEstadoHistorialDto>>> GetHistorialPaginatedAsync(
            int page, int pageSize, int tenantId, int? leadId = null, int? usuarioId = null, int? estadoId = null)
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;

                var (historial, totalRecords) = await _historialRepository.GetHistorialPagedByTenantAsync(
                    page, pageSize, tenantId, leadId, usuarioId, estadoId);

                var historialDto = historial.Select(MapToResponseDto).ToList();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var paginatedResponse = new PaginatedResponseDto<LeadEstadoHistorialDto>
                {
                    Data = historialDto,
                    Page = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };

                return new BaseResponseDto<PaginatedResponseDto<LeadEstadoHistorialDto>>
                {
                    Success = true,
                    Data = paginatedResponse,
                    Message = "Historial de estados obtenido correctamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial paginado para tenant: {TenantId}", tenantId);
                return new BaseResponseDto<PaginatedResponseDto<LeadEstadoHistorialDto>>
                {
                    Success = false,
                    Message = "No se pudo cargar el historial de estados.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<IEnumerable<LeadEstadoHistorialDto>>> GetHistorialByLeadAsync(int leadId, int tenantId)
        {
            try
            {
                // Validar que el lead pertenezca al tenant
                var leadExists = await _historialRepository.ValidateLeadInTenantAsync(leadId, tenantId);
                if (!leadExists)
                {
                    return new BaseResponseDto<IEnumerable<LeadEstadoHistorialDto>>
                    {
                        Success = false,
                        Message = "Lead no encontrado en su organización."
                    };
                }

                var historial = await _historialRepository.GetHistorialByLeadAndTenantAsync(leadId, tenantId);
                var historialDto = historial.Select(MapToResponseDto).ToList();

                return new BaseResponseDto<IEnumerable<LeadEstadoHistorialDto>>
                {
                    Success = true,
                    Data = historialDto,
                    Message = "Historial del lead obtenido correctamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial del lead: {LeadId} en tenant: {TenantId}", leadId, tenantId);
                return new BaseResponseDto<IEnumerable<LeadEstadoHistorialDto>>
                {
                    Success = false,
                    Message = "Error al cargar historial del lead.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<LeadEstadoHistorialDto>> GetByIdAndTenantAsync(int id, int tenantId)
        {
            try
            {
                var historial = await _historialRepository.GetByIdWithDetailsAndTenantAsync(id, tenantId);
                if (historial == null)
                {
                    return new BaseResponseDto<LeadEstadoHistorialDto>
                    {
                        Success = false,
                        Message = "Registro de historial no encontrado en su organización."
                    };
                }

                var historialDto = MapToResponseDto(historial);
                return new BaseResponseDto<LeadEstadoHistorialDto>
                {
                    Success = true,
                    Data = historialDto,
                    Message = "Registro de historial encontrado"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial por ID: {Id} en tenant: {TenantId}", id, tenantId);
                return new BaseResponseDto<LeadEstadoHistorialDto>
                {
                    Success = false,
                    Message = "Error al buscar el registro de historial.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }

        public async Task<BaseResponseDto<LeadEstadoHistorialDto>> CreateHistorialAsync(CreateLeadEstadoHistorialDto createDto, int tenantId)
        {
            try
            {
                _logger.LogInformation("Creando historial de estado para lead: {LeadId} de {EstadoAnterior} a {EstadoNuevo}",
                    createDto.IdLead, createDto.IdEstadoAnterior, createDto.IdEstadoNuevo);

                // Validar que el lead pertenezca al tenant
                var leadExists = await _historialRepository.ValidateLeadInTenantAsync(createDto.IdLead, tenantId);
                if (!leadExists)
                {
                    return new BaseResponseDto<LeadEstadoHistorialDto>
                    {
                        Success = false,
                        Message = "Lead no encontrado en su organización."
                    };
                }

                var historial = new LeadEstadoHistorial
                {
                    IdLead = createDto.IdLead,
                    IdEstadoAnterior = createDto.IdEstadoAnterior,
                    IdEstadoNuevo = createDto.IdEstadoNuevo,
                    IdUsuario = createDto.IdUsuario,
                    Comentario = createDto.Comentario,
                    CreadoEn = DateTime.UtcNow
                };

                var result = await _historialRepository.AddAsync(historial);

                _logger.LogInformation("Historial creado exitosamente con ID: {Id}", result.Id);

                var historialConDetalles = await _historialRepository.GetByIdWithDetailsAsync(result.Id);
                var responseDto = MapToResponseDto(historialConDetalles ?? result);

                return new BaseResponseDto<LeadEstadoHistorialDto>
                {
                    Success = true,
                    Data = responseDto,
                    Message = "Historial de cambio de estado registrado correctamente."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear historial de estado para lead: {LeadId}", createDto?.IdLead);
                return new BaseResponseDto<LeadEstadoHistorialDto>
                {
                    Success = false,
                    Message = "No se pudo registrar el historial de estado.",
                    Errors = new List<string> { "Error interno del servidor." }
                };
            }
        }
    }
}
