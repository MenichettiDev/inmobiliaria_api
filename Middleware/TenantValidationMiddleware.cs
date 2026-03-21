using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class TenantValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantValidationMiddleware> _logger;

    public TenantValidationMiddleware(RequestDelegate next, ILogger<TenantValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            // Excluir rutas específicas
            var path = context.Request.Path.Value?.ToLower();
            if (path != null && (
                path.StartsWith("/api/auth/login") ||
                path.StartsWith("/api/auth/refresh") ||
                path.StartsWith("/api/billing/webhook") ||
                path.StartsWith("/api/onboarding") ||
                path.StartsWith("/api/public") ||
                path.StartsWith("/api/usuario/validate") ||
                path.StartsWith("/api/usuario/debug") ||
                path.Contains("/swagger") ||
                path.StartsWith("/health")))
            {
                await _next(context);
                return;
            }

            var user = context.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                var idInmobiliaria = user.Claims.FirstOrDefault(c => c.Type == "IdInmobiliaria")?.Value;

                _logger.LogInformation("Validando tenant para path: {Path}, IdInmobiliaria: {IdInmobiliaria}", path, idInmobiliaria);

                if (string.IsNullOrEmpty(idInmobiliaria))
                {
                    _logger.LogWarning("El token no contiene el claim 'IdInmobiliaria' para path: {Path}", path);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("No se pudo validar el IdInmobiliaria.");
                    return;
                }

                if (!int.TryParse(idInmobiliaria, out _))
                {
                    _logger.LogWarning("El claim 'IdInmobiliaria' no es un número válido: {IdInmobiliaria}", idInmobiliaria);
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("El IdInmobiliaria no es válido.");
                    return;
                }

                _logger.LogInformation("Tenant válido: {IdInmobiliaria} para path: {Path}", idInmobiliaria, path);
            }

            // Continuar con el siguiente middleware
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en TenantValidationMiddleware para path: {Path}", context.Request.Path);
            
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            
            var errorResponse = new
            {
                status = 500,
                error = "Internal Server Error",
                message = "Ocurrió un error interno en el servidor."
            };
            
            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }
}