using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using inmobiliariaApi.Data;
using inmobiliariaApi.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClienteController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public ClienteController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private int GetTenantId()
        {
            var tenantClaim = User.FindFirst("IdInmobiliaria");
            return tenantClaim != null && int.TryParse(tenantClaim.Value, out var t) ? t : 0;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Supervisor,Agente")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? nombre = null, [FromQuery] bool? activo = null)
        {
            var tenant = GetTenantId();
            if (tenant <= 0) return BadRequest(new { Success = false, Message = "Tenant no válido." });

            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var query = _dbContext.Clientes.AsQueryable().Where(c => c.IdInmobiliaria == tenant);

            if (!string.IsNullOrWhiteSpace(nombre))
                query = query.Where(c => EF.Functions.Like(c.NombreCompleto, $"%{nombre}%"));

            if (activo.HasValue)
                query = query.Where(c => c.Activo == activo.Value);

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(c => c.CreadoEn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.Id,
                    c.IdInmobiliaria,
                    c.NombreCompleto,
                    c.Dni,
                    c.Email,
                    c.Telefono,
                    c.Activo,
                    c.CreadoEn,
                    c.ActualizadoEn
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)total / pageSize);

            return Ok(new
            {
                Success = true,
                Data = data,
                Page = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Supervisor,Agente")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest(new { Success = false, Message = "ID no válido." });

            var tenant = GetTenantId();
            if (tenant <= 0) return BadRequest(new { Success = false, Message = "Tenant no válido." });

            var cliente = await _dbContext.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.IdInmobiliaria == tenant);
            if (cliente == null) return NotFound(new { Success = false, Message = "Cliente no encontrado." });

            return Ok(new { Success = true, Data = cliente });
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> Create([FromBody] Cliente dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { Success = false, Message = "Datos no válidos.", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            var tenant = GetTenantId();
            if (tenant <= 0) return BadRequest(new { Success = false, Message = "Tenant no válido." });

            dto.IdInmobiliaria = tenant;
            dto.CreadoEn = DateTime.UtcNow;
            dto.ActualizadoEn = DateTime.UtcNow;
            dto.Activo = true;

            _dbContext.Clientes.Add(dto);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, new { Success = true, Data = dto });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Supervisor")]
        public async Task<IActionResult> Update(int id, [FromBody] Cliente dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { Success = false, Message = "Datos no válidos." });
            if (id != dto.Id) return BadRequest(new { Success = false, Message = "ID no coincide." });

            var tenant = GetTenantId();
            if (tenant <= 0) return BadRequest(new { Success = false, Message = "Tenant no válido." });

            var existing = await _dbContext.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.IdInmobiliaria == tenant);
            if (existing == null) return NotFound(new { Success = false, Message = "Cliente no encontrado." });

            // Actualizar campos permitidos
            if (!string.IsNullOrWhiteSpace(dto.NombreCompleto)) existing.NombreCompleto = dto.NombreCompleto.Trim();
            if (dto.Dni != null) existing.Dni = dto.Dni;
            if (dto.Email != null) existing.Email = dto.Email;
            if (dto.Telefono != null) existing.Telefono = dto.Telefono;
            existing.ActualizadoEn = DateTime.UtcNow;
            existing.Activo = dto.Activo;

            _dbContext.Clientes.Update(existing);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Success = true, Data = existing });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest(new { Success = false, Message = "ID no válido." });

            var tenant = GetTenantId();
            if (tenant <= 0) return BadRequest(new { Success = false, Message = "Tenant no válido." });

            var existing = await _dbContext.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.IdInmobiliaria == tenant);
            if (existing == null) return NotFound(new { Success = false, Message = "Cliente no encontrado." });

            if (!existing.Activo) return BadRequest(new { Success = false, Message = "Cliente ya inactivo." });

            existing.Activo = false;
            existing.ActualizadoEn = DateTime.UtcNow;

            _dbContext.Clientes.Update(existing);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Cliente desactivado correctamente." });
        }
    }
}