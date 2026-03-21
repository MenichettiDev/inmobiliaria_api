using System.Security.Claims;
using System.Text.Json;
using inmobiliariaApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace inmobiliariaApi.Controllers
{
    [ApiController]
    [Route("api/billing")]
    public class BillingController : ControllerBase
    {
        private readonly BillingService _billingService;
        private readonly PlanGateService _planGateService;
        private readonly ILogger<BillingController> _logger;

        public BillingController(
            BillingService billingService,
            PlanGateService planGateService,
            ILogger<BillingController> logger)
        {
            _billingService = billingService;
            _planGateService = planGateService;
            _logger = logger;
        }

        // POST: api/billing/checkout
        // Genera la preferencia MP y devuelve la URL de checkout
        [HttpPost("checkout")]
        [Authorize(Roles = "Administrador,SuperAdmin")]
        [EnableRateLimiting("api-tenant")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            if (request.IdPlan <= 0)
                return BadRequest(new { status = 400, message = "IdPlan no válido." });

            var idInmobiliaria = GetTenantId();
            if (idInmobiliaria == 0)
                return Unauthorized();

            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var nombre = User.FindFirst(ClaimTypes.Name)?.Value ?? "";

            var result = await _billingService.CreateCheckoutAsync(idInmobiliaria, request.IdPlan, email, nombre);

            if (!result.Success)
                return BadRequest(new { status = 400, message = result.Message });

            return Ok(new
            {
                status = 200,
                message = result.Message,
                data = result.Data
            });
        }

        // POST: api/billing/webhook
        // MercadoPago llama este endpoint cuando ocurre un evento de pago.
        // IMPORTANTE: debe ser público (sin [Authorize]) y excluido del TenantValidationMiddleware.
        [HttpPost("webhook")]
        [AllowAnonymous]
        [DisableRateLimiting]
        public async Task<IActionResult> Webhook(
            [FromQuery] string? type,
            [FromQuery] string? data_id,
            [FromServices] MercadoPagoService mpService)
        {
            // MP envía POST con query: ?type=payment&data.id=xxx  (v2 notifications)
            // o body JSON con topic/id  (v1 notifications)
            // Soportamos ambas formas.

            var xSignature = Request.Headers["x-signature"].FirstOrDefault() ?? "";
            var xRequestId = Request.Headers["x-request-id"].FirstOrDefault() ?? "";

            // Intentar leer body si data_id no vino en query
            string? paymentIdStr = data_id;
            string? notificationType = type;

            if (string.IsNullOrEmpty(paymentIdStr))
            {
                try
                {
                    using var reader = new StreamReader(Request.Body);
                    var body = await reader.ReadToEndAsync();
                    if (!string.IsNullOrEmpty(body))
                    {
                        var doc = JsonDocument.Parse(body);
                        if (doc.RootElement.TryGetProperty("data", out var dataEl) &&
                            dataEl.TryGetProperty("id", out var idEl))
                        {
                            paymentIdStr = idEl.GetRawText().Trim('"');
                        }
                        if (doc.RootElement.TryGetProperty("topic", out var topicEl))
                        {
                            notificationType = topicEl.GetString();
                        }
                    }
                }
                catch { /* body vacío o inválido */ }
            }

            // Solo procesamos notificaciones de pagos
            if (!string.Equals(notificationType, "payment", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(notificationType, "payment_v2", StringComparison.OrdinalIgnoreCase))
            {
                return Ok(); // Ignorar otros tipos (subscriptions, etc)
            }

            if (!long.TryParse(paymentIdStr, out var paymentId))
            {
                _logger.LogWarning("Webhook recibido con data_id inválido: {DataId}", paymentIdStr);
                return Ok(); // Responder 200 para que MP no reintente
            }

            // Verificar firma HMAC (si está configurada)
            if (!string.IsNullOrEmpty(xSignature))
            {
                var valid = mpService.VerifyWebhookSignature(xSignature, xRequestId, paymentIdStr!);
                if (!valid)
                {
                    _logger.LogWarning("Firma de webhook inválida para PaymentId {PaymentId}", paymentId);
                    return Unauthorized();
                }
            }

            // Procesar en background para responder rápido a MP (< 5s)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _billingService.ProcessWebhookAsync(paymentId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando webhook en background para PaymentId {Id}", paymentId);
                }
            });

            // MP espera 200 inmediatamente
            return Ok();
        }

        // GET: api/billing/estado-pago/{preferenceId}
        // El frontend consulta esto después del redirect de MP para conocer el resultado
        [HttpGet("estado-pago/{preferenceId}")]
        [Authorize(Roles = "Administrador,SuperAdmin")]
        [EnableRateLimiting("api-tenant")]
        public async Task<IActionResult> EstadoPago(string preferenceId)
        {
            var idInmobiliaria = GetTenantId();
            if (idInmobiliaria == 0) return Unauthorized();

            var result = await _billingService.GetEstadoPagoAsync(idInmobiliaria, preferenceId);

            if (!result.Success)
                return NotFound(new { status = 404, message = result.Message });

            return Ok(new { status = 200, data = result.Data });
        }

        // GET: api/billing/mi-plan
        // Resumen de plan y uso actual del tenant
        [HttpGet("mi-plan")]
        [Authorize(Roles = "Administrador,SuperAdmin,Supervisor")]
        [EnableRateLimiting("api-tenant")]
        public async Task<IActionResult> MiPlan()
        {
            var idInmobiliaria = GetTenantId();
            if (idInmobiliaria == 0) return Unauthorized();

            var resumen = await _planGateService.GetResumenPlanAsync(idInmobiliaria);

            if (resumen == null)
                return Ok(new { status = 200, message = "Sin suscripción activa.", data = (object?)null });

            return Ok(new { status = 200, data = resumen });
        }

        private int GetTenantId()
        {
            var claim = User.FindFirst("IdInmobiliaria")?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }
    }

    public class CheckoutRequest
    {
        public int IdPlan { get; set; }
    }
}
