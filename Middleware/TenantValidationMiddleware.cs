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
            // Excluir rutas específicas (por ejemplo, login)
            var path = context.Request.Path.Value?.ToLower();
            if (path != null && (path.StartsWith("/api/auth/login") || path.StartsWith("/api/public")))
            {
                await _next(context);
                return;
            }

            var user = context.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                var idInmobiliaria = user.Claims.FirstOrDefault(c => c.Type == "IdInmobiliaria")?.Value;

                if (string.IsNullOrEmpty(idInmobiliaria))
                {
                    _logger.LogWarning("El token no contiene el claim 'IdInmobiliaria'.");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("No se pudo validar el IdInmobiliaria.");
                    return;
                }

                if (!int.TryParse(idInmobiliaria, out _))
                {
                    _logger.LogWarning("El claim 'IdInmobiliaria' no es un número válido.");
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("El IdInmobiliaria no es válido.");
                    return;
                }
            }

            // Continuar con el siguiente middleware
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió un error en el TenantValidationMiddleware.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("Ocurrió un error interno en el servidor.");
        }
    }
}