using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using inmobiliariaApi.Models;
using inmobiliariaApi.Repositories;
using inmobiliariaApi.DTOs.Plan;
using System.Text.Json;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PlanController : ControllerBase
    {
        private readonly GenericRepository<Plan> _planRepository;

        public PlanController(GenericRepository<Plan> planRepository)
        {
            _planRepository = planRepository;
        }

        // GET: api/plan (Lista de todos los planes)
        [HttpGet]
        [Authorize(Roles = "Programador,Administrador")] // Solo roles de alta administración
        public async Task<IActionResult> GetPlanes()
        {
            var planes = await _planRepository.GetAllAsync();

            // Mapear a DTO limpio
            var result = planes.Select(p => new PlanDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                PrecioUsd = p.PrecioUsd,
                MaxPropiedades = p.MaxPropiedades,
                MaxUsuarios = p.MaxUsuarios,
                MaxLeadsMes = p.MaxLeadsMes,
                WhiteLabel = p.WhiteLabel,
                DominioPersonalizado = p.DominioPersonalizado,
                Automatizaciones = p.Automatizaciones,
                ApiAcceso = p.ApiAcceso,
                Soporte = p.Soporte,
                Activo = p.Activo,
                CreadoEn = p.CreadoEn,
                ActualizadoEn = p.ActualizadoEn,
                HistorialEstados = p.HistorialEstados,
                ActividadesLead = p.ActividadesLead,
                TiposActividad = p.TiposActividad,
                AutomatizacionLeads = p.AutomatizacionLeads,
                LeadScoring = p.LeadScoring
            });

            return Ok(new { Success = true, Data = result, Message = "Planes obtenidos correctamente" });
        }

        // GET: api/plan/activos (Solo planes activos para suscripciones)
        [HttpGet("activos")]
        [Authorize(Roles = "Programador,Administrador,Supervisor")] // Para consultas comerciales
        public async Task<IActionResult> GetPlanesActivos()
        {
            var planes = await _planRepository.FindAsync(p => p.Activo == true);

            var result = planes.Select(p => new PlanDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                PrecioUsd = p.PrecioUsd,
                MaxPropiedades = p.MaxPropiedades,
                MaxUsuarios = p.MaxUsuarios,
                MaxLeadsMes = p.MaxLeadsMes,
                WhiteLabel = p.WhiteLabel,
                DominioPersonalizado = p.DominioPersonalizado,
                Automatizaciones = p.Automatizaciones,
                ApiAcceso = p.ApiAcceso,
                Soporte = p.Soporte,
                Activo = p.Activo,
                CreadoEn = p.CreadoEn,
                ActualizadoEn = p.ActualizadoEn,
                HistorialEstados = p.HistorialEstados,
                ActividadesLead = p.ActividadesLead,
                TiposActividad = p.TiposActividad,
                AutomatizacionLeads = p.AutomatizacionLeads,
                LeadScoring = p.LeadScoring
            });

            return Ok(new { Success = true, Data = result, Message = "Planes activos obtenidos correctamente" });
        }

        // GET: api/plan/{id} (Un plan específico)
        [HttpGet("{id}")]
        [Authorize(Roles = "Programador,Administrador")]
        public async Task<IActionResult> GetPlan(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "El ID debe ser mayor a 0." });
            }

            var plan = await _planRepository.GetByIdAsync(id);
            if (plan == null)
                return NotFound(new { Success = false, Message = "Plan no encontrado" });

            var result = new PlanDto
            {
                Id = plan.Id,
                Nombre = plan.Nombre,
                Descripcion = plan.Descripcion,
                PrecioUsd = plan.PrecioUsd,
                MaxPropiedades = plan.MaxPropiedades,
                MaxUsuarios = plan.MaxUsuarios,
                MaxLeadsMes = plan.MaxLeadsMes,
                WhiteLabel = plan.WhiteLabel,
                DominioPersonalizado = plan.DominioPersonalizado,
                Automatizaciones = plan.Automatizaciones,
                ApiAcceso = plan.ApiAcceso,
                Soporte = plan.Soporte,
                Activo = plan.Activo,
                CreadoEn = plan.CreadoEn,
                ActualizadoEn = plan.ActualizadoEn,
                HistorialEstados = plan.HistorialEstados,
                ActividadesLead = plan.ActividadesLead,
                TiposActividad = plan.TiposActividad,
                AutomatizacionLeads = plan.AutomatizacionLeads,
                LeadScoring = plan.LeadScoring
            };

            return Ok(new { Success = true, Data = result, Message = "Plan encontrado" });
        }

        // POST: api/plan (Crear un nuevo plan)
        [HttpPost]
        [Authorize(Roles = "Programador")] // Solo Programador puede crear planes
        public async Task<IActionResult> PostPlan([FromBody] CreatePlanDto planDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Success = false, Message = "Datos no válidos", Errors = errors });
            }

            // Validar que el nombre no exista
            var existingNombre = await _planRepository.FindAsync(p => p.Nombre.ToLower() == planDto.Nombre.ToLower());
            if (existingNombre.Any())
            {
                return BadRequest(new { Success = false, Message = "Ya existe un plan con ese nombre." });
            }

            // Validar JSON de tipos de actividad si se proporciona
            if (!string.IsNullOrEmpty(planDto.TiposActividad))
            {
                try
                {
                    JsonDocument.Parse(planDto.TiposActividad);
                }
                catch (JsonException)
                {
                    return BadRequest(new { Success = false, Message = "El campo TiposActividad debe ser un JSON válido." });
                }
            }

            // Validaciones lógicas de negocio
            if (planDto.PrecioUsd == 0 && (planDto.MaxPropiedades == null || planDto.MaxPropiedades > 10))
            {
                return BadRequest(new { Success = false, Message = "Los planes gratuitos no pueden tener más de 10 propiedades." });
            }

            if (planDto.WhiteLabel && planDto.PrecioUsd < 50)
            {
                return BadRequest(new { Success = false, Message = "La funcionalidad White Label requiere planes de al menos $50 USD." });
            }

            // Crear un objeto Plan a partir del DTO
            var plan = new Plan
            {
                Nombre = planDto.Nombre.Trim(),
                Descripcion = planDto.Descripcion?.Trim(),
                PrecioUsd = planDto.PrecioUsd,
                MaxPropiedades = planDto.MaxPropiedades,
                MaxUsuarios = planDto.MaxUsuarios,
                MaxLeadsMes = planDto.MaxLeadsMes,
                WhiteLabel = planDto.WhiteLabel,
                DominioPersonalizado = planDto.DominioPersonalizado,
                Automatizaciones = planDto.Automatizaciones,
                ApiAcceso = planDto.ApiAcceso,
                Soporte = planDto.Soporte,
                Activo = planDto.Activo,
                CreadoEn = DateTime.UtcNow,
                ActualizadoEn = DateTime.UtcNow,
                HistorialEstados = planDto.HistorialEstados,
                ActividadesLead = planDto.ActividadesLead,
                TiposActividad = planDto.TiposActividad ?? "[\"nota\"]",
                AutomatizacionLeads = planDto.AutomatizacionLeads,
                LeadScoring = planDto.LeadScoring
            };

            var result = await _planRepository.AddAsync(plan);

            var responseDto = new PlanDto
            {
                Id = result.Id,
                Nombre = result.Nombre,
                Descripcion = result.Descripcion,
                PrecioUsd = result.PrecioUsd,
                MaxPropiedades = result.MaxPropiedades,
                MaxUsuarios = result.MaxUsuarios,
                MaxLeadsMes = result.MaxLeadsMes,
                WhiteLabel = result.WhiteLabel,
                DominioPersonalizado = result.DominioPersonalizado,
                Automatizaciones = result.Automatizaciones,
                ApiAcceso = result.ApiAcceso,
                Soporte = result.Soporte,
                Activo = result.Activo,
                CreadoEn = result.CreadoEn,
                ActualizadoEn = result.ActualizadoEn,
                HistorialEstados = result.HistorialEstados,
                ActividadesLead = result.ActividadesLead,
                TiposActividad = result.TiposActividad,
                AutomatizacionLeads = result.AutomatizacionLeads,
                LeadScoring = result.LeadScoring
            };

            return CreatedAtAction(nameof(GetPlan), new { id = result.Id },
                new { Success = true, Data = responseDto, Message = "Plan creado correctamente" });
        }

        // PUT: api/plan/{id} (Actualizar plan)
        [HttpPut("{id}")]
        [Authorize(Roles = "Programador")] // Solo Programador puede actualizar planes
        public async Task<IActionResult> PutPlan(int id, [FromBody] UpdatePlanDto planDto)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Success = false, Message = "Datos no válidos", Errors = errors });
            }

            if (id != planDto.Id)
                return BadRequest(new { Success = false, Message = "El ID de la URL no coincide con el ID del objeto" });

            var existingPlan = await _planRepository.GetByIdAsync(id);
            if (existingPlan == null)
                return NotFound(new { Success = false, Message = "Plan no encontrado" });

            // Validar que el nombre no exista en otro registro
            var existingNombre = await _planRepository.FindAsync(p =>
                p.Nombre.ToLower() == planDto.Nombre.ToLower() && p.Id != id);
            if (existingNombre.Any())
            {
                return BadRequest(new { Success = false, Message = "Ya existe otro plan con ese nombre." });
            }

            // Validar JSON de tipos de actividad si se proporciona
            if (!string.IsNullOrEmpty(planDto.TiposActividad))
            {
                try
                {
                    JsonDocument.Parse(planDto.TiposActividad);
                }
                catch (JsonException)
                {
                    return BadRequest(new { Success = false, Message = "El campo TiposActividad debe ser un JSON válido." });
                }
            }

            // Validaciones lógicas de negocio
            if (planDto.PrecioUsd == 0 && (planDto.MaxPropiedades == null || planDto.MaxPropiedades > 10))
            {
                return BadRequest(new { Success = false, Message = "Los planes gratuitos no pueden tener más de 10 propiedades." });
            }

            if (planDto.WhiteLabel && planDto.PrecioUsd < 50)
            {
                return BadRequest(new { Success = false, Message = "La funcionalidad White Label requiere planes de al menos $50 USD." });
            }

            // Actualizar campos
            existingPlan.Nombre = planDto.Nombre.Trim();
            existingPlan.Descripcion = planDto.Descripcion?.Trim();
            existingPlan.PrecioUsd = planDto.PrecioUsd;
            existingPlan.MaxPropiedades = planDto.MaxPropiedades;
            existingPlan.MaxUsuarios = planDto.MaxUsuarios;
            existingPlan.MaxLeadsMes = planDto.MaxLeadsMes;
            existingPlan.WhiteLabel = planDto.WhiteLabel;
            existingPlan.DominioPersonalizado = planDto.DominioPersonalizado;
            existingPlan.Automatizaciones = planDto.Automatizaciones;
            existingPlan.ApiAcceso = planDto.ApiAcceso;
            existingPlan.Soporte = planDto.Soporte;
            existingPlan.Activo = planDto.Activo;
            existingPlan.ActualizadoEn = DateTime.UtcNow;
            existingPlan.HistorialEstados = planDto.HistorialEstados;
            existingPlan.ActividadesLead = planDto.ActividadesLead;
            existingPlan.TiposActividad = planDto.TiposActividad;
            existingPlan.AutomatizacionLeads = planDto.AutomatizacionLeads;
            existingPlan.LeadScoring = planDto.LeadScoring;

            await _planRepository.UpdateAsync(existingPlan);

            var responseDto = new PlanDto
            {
                Id = existingPlan.Id,
                Nombre = existingPlan.Nombre,
                Descripcion = existingPlan.Descripcion,
                PrecioUsd = existingPlan.PrecioUsd,
                MaxPropiedades = existingPlan.MaxPropiedades,
                MaxUsuarios = existingPlan.MaxUsuarios,
                MaxLeadsMes = existingPlan.MaxLeadsMes,
                WhiteLabel = existingPlan.WhiteLabel,
                DominioPersonalizado = existingPlan.DominioPersonalizado,
                Automatizaciones = existingPlan.Automatizaciones,
                ApiAcceso = existingPlan.ApiAcceso,
                Soporte = existingPlan.Soporte,
                Activo = existingPlan.Activo,
                CreadoEn = existingPlan.CreadoEn,
                ActualizadoEn = existingPlan.ActualizadoEn,
                HistorialEstados = existingPlan.HistorialEstados,
                ActividadesLead = existingPlan.ActividadesLead,
                TiposActividad = existingPlan.TiposActividad,
                AutomatizacionLeads = existingPlan.AutomatizacionLeads,
                LeadScoring = existingPlan.LeadScoring
            };

            return Ok(new { Success = true, Data = responseDto, Message = "Plan actualizado correctamente" });
        }

        // DELETE: api/plan/{id} (Eliminar plan - solo desactivar)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Programador")] // Solo Programador puede eliminar planes
        public async Task<IActionResult> DeletePlan(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            var plan = await _planRepository.GetByIdAsync(id);
            if (plan == null)
                return NotFound(new { Success = false, Message = "Plan no encontrado" });

            // Verificar que no sea el plan FREE (ID 1) que es fundamental
            if (id == 1)
            {
                return BadRequest(new { Success = false, Message = "No se puede eliminar el plan FREE." });
            }

            // TODO: Verificar que no haya inmobiliarias usando este plan
            // var inmobiliariasConPlan = await _planRepository.FindAsync(p => p.Id == id);
            // var planCompleto = inmobiliariasConPlan.FirstOrDefault();
            // if (planCompleto != null && planCompleto.Inmobiliarias.Any())
            // {
            //     // En lugar de eliminar físicamente, desactivar
            //     planCompleto.Activo = false;
            //     planCompleto.ActualizadoEn = DateTime.UtcNow;
            //     await _planRepository.UpdateAsync(planCompleto);
            //     return Ok(new { Success = true, Message = "Plan desactivado correctamente (hay inmobiliarias que lo usan)." });
            // }

            // Por ahora, simplemente desactivar el plan en lugar de eliminarlo físicamente
            plan.Activo = false;
            plan.ActualizadoEn = DateTime.UtcNow;
            await _planRepository.UpdateAsync(plan);

            return Ok(new { Success = true, Message = "Plan desactivado correctamente" });
        }

        // PATCH: api/plan/{id}/toggle (Activar/Desactivar plan)
        [HttpPatch("{id}/toggle")]
        [Authorize(Roles = "Programador")]
        public async Task<IActionResult> TogglePlan(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Success = false, Message = "ID no válido." });
            }

            var plan = await _planRepository.GetByIdAsync(id);
            if (plan == null)
                return NotFound(new { Success = false, Message = "Plan no encontrado" });

            plan.Activo = !plan.Activo;
            plan.ActualizadoEn = DateTime.UtcNow;
            await _planRepository.UpdateAsync(plan);

            string accion = plan.Activo ? "activado" : "desactivado";
            return Ok(new { Success = true, Message = $"Plan {accion} correctamente." });
        }
    }
}
