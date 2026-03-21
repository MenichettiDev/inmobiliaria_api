using Microsoft.EntityFrameworkCore;
using inmobiliariaApi.Data;
using inmobiliariaApi.DTOs.Common;
using inmobiliariaApi.Models;

namespace inmobiliariaApi.Services
{
    public class CheckoutResult
    {
        public string PreferenceId { get; set; } = string.Empty;
        public string CheckoutUrl { get; set; } = string.Empty;
        public int PagoId { get; set; }
    }

    public class BillingService
    {
        private readonly ApplicationDbContext _context;
        private readonly MercadoPagoService _mpService;
        private readonly SuscripcionService _suscripcionService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BillingService> _logger;

        public BillingService(
            ApplicationDbContext context,
            MercadoPagoService mpService,
            SuscripcionService suscripcionService,
            IConfiguration configuration,
            ILogger<BillingService> logger)
        {
            _context = context;
            _mpService = mpService;
            _suscripcionService = suscripcionService;
            _configuration = configuration;
            _logger = logger;
        }

        // Crea la preferencia de pago en MP y registra el intento en DB
        public async Task<BaseResponseDto<CheckoutResult>> CreateCheckoutAsync(
            int idInmobiliaria, int idPlan, string payerEmail, string payerName)
        {
            try
            {
                var plan = await _context.Plan
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == idPlan && p.Activo);

                if (plan == null)
                    return Fail<CheckoutResult>("Plan no encontrado o inactivo.");

                var inmobiliaria = await _context.Inmobiliaria
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.Id == idInmobiliaria && i.IdEstado == 1);

                if (inmobiliaria == null)
                    return Fail<CheckoutResult>("Inmobiliaria no encontrada.");

                var frontendBase = _configuration["App:FrontendUrl"]
                    ?? "https://buscopropiedades.com.ar";
                var notificationUrl = _configuration["MercadoPago:NotificationUrl"]
                    ?? throw new InvalidOperationException("MercadoPago:NotificationUrl no configurado.");

                // ExternalReference permite identificar el pago en el webhook
                // Formato: {idInmobiliaria}|{idPlan}
                var externalRef = $"{idInmobiliaria}|{idPlan}";

                var mpRequest = new MpPreferenceRequest
                {
                    ExternalReference = externalRef,
                    NotificationUrl = notificationUrl,
                    AutoReturn = "approved",
                    ExpirationDateTo = DateTime.UtcNow.AddHours(24)
                        .ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                    Expires = true,
                    BackUrls = new MpBackUrls
                    {
                        Success = $"{frontendBase}/billing/resultado?estado=success",
                        Failure = $"{frontendBase}/billing/resultado?estado=failure",
                        Pending = $"{frontendBase}/billing/resultado?estado=pending"
                    },
                    Payer = new MpPayer
                    {
                        Email = payerEmail,
                        Name = payerName
                    },
                    Items = new List<MpItem>
                    {
                        new MpItem
                        {
                            Id = $"plan-{plan.Id}",
                            Title = $"Plan {plan.Nombre} — {inmobiliaria.Nombre}",
                            Description = plan.Descripcion,
                            Quantity = 1,
                            UnitPrice = plan.PrecioUsd,
                            CurrencyId = "ARS"
                        }
                    }
                };

                var preference = await _mpService.CreatePreferenceAsync(mpRequest);

                // Registrar el intento de pago
                var pago = new PagoSuscripcion
                {
                    IdInmobiliaria = idInmobiliaria,
                    IdPlan = idPlan,
                    MpPreferenceId = preference.Id,
                    Estado = EstadoPago.Pendiente,
                    Monto = plan.PrecioUsd,
                    Moneda = "ARS",
                    DiasPlan = 30,
                    CreadoEn = DateTime.UtcNow,
                    ActualizadoEn = DateTime.UtcNow
                };

                _context.PagosSuscripcion.Add(pago);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Checkout creado para inmobiliaria {IdInmobiliaria}, plan {IdPlan}. PreferenceId: {PrefId}",
                    idInmobiliaria, idPlan, preference.Id);

                return Ok(new CheckoutResult
                {
                    PreferenceId = preference.Id,
                    CheckoutUrl = _mpService.GetCheckoutUrl(preference),
                    PagoId = pago.Id
                }, "Checkout generado correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando checkout para inmobiliaria {Id}", idInmobiliaria);
                return Fail<CheckoutResult>("No se pudo generar el checkout de pago.");
            }
        }

        // Procesa la notificación del webhook de MercadoPago
        public async Task<bool> ProcessWebhookAsync(long paymentId)
        {
            try
            {
                // Evitar reprocesamiento: idempotencia
                var yaProcessado = await _context.PagosSuscripcion
                    .AnyAsync(p => p.MpPaymentId == paymentId && p.Estado == EstadoPago.Aprobado);

                if (yaProcessado)
                {
                    _logger.LogInformation("Pago {PaymentId} ya fue procesado. Skipping.", paymentId);
                    return true;
                }

                // Verificar el pago directamente con MP
                var mpPayment = await _mpService.GetPaymentAsync(paymentId);

                // ExternalReference: "{idInmobiliaria}|{idPlan}"
                if (string.IsNullOrEmpty(mpPayment.ExternalReference))
                {
                    _logger.LogWarning("Pago {PaymentId} sin ExternalReference.", paymentId);
                    return false;
                }

                var parts = mpPayment.ExternalReference.Split('|');
                if (parts.Length != 2 ||
                    !int.TryParse(parts[0], out var idInmobiliaria) ||
                    !int.TryParse(parts[1], out var idPlan))
                {
                    _logger.LogWarning("ExternalReference inválido: {Ref}", mpPayment.ExternalReference);
                    return false;
                }

                // Buscar el registro de pago pendiente
                var pago = await _context.PagosSuscripcion
                    .Where(p => p.IdInmobiliaria == idInmobiliaria &&
                                p.IdPlan == idPlan &&
                                p.Estado == EstadoPago.Pendiente)
                    .OrderByDescending(p => p.CreadoEn)
                    .FirstOrDefaultAsync();

                if (pago == null)
                {
                    // Crear registro si llegó el webhook pero no se creó el pago localmente
                    pago = new PagoSuscripcion
                    {
                        IdInmobiliaria = idInmobiliaria,
                        IdPlan = idPlan,
                        MpPreferenceId = "webhook-direct",
                        Monto = mpPayment.TransactionAmount,
                        Moneda = mpPayment.CurrencyId,
                        DiasPlan = 30,
                        CreadoEn = DateTime.UtcNow,
                    };
                    _context.PagosSuscripcion.Add(pago);
                }

                pago.MpPaymentId = mpPayment.Id;
                pago.ActualizadoEn = DateTime.UtcNow;

                switch (mpPayment.Status)
                {
                    case "approved":
                        pago.Estado = EstadoPago.Aprobado;
                        await _context.SaveChangesAsync();
                        await ActivarSuscripcionAsync(pago);
                        break;

                    case "rejected":
                        pago.Estado = EstadoPago.Rechazado;
                        await _context.SaveChangesAsync();
                        break;

                    case "cancelled":
                        pago.Estado = EstadoPago.Cancelado;
                        await _context.SaveChangesAsync();
                        break;

                    default:
                        // pending, in_process, etc → mantener pendiente
                        await _context.SaveChangesAsync();
                        break;
                }

                _logger.LogInformation(
                    "Webhook procesado: PaymentId={PaymentId}, Status={Status}, Inmobiliaria={Id}",
                    paymentId, mpPayment.Status, idInmobiliaria);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando webhook para PaymentId {PaymentId}", paymentId);
                return false;
            }
        }

        // Activa/renueva la suscripción después de un pago aprobado
        private async Task ActivarSuscripcionAsync(PagoSuscripcion pago)
        {
            var ahora = DateTime.UtcNow;

            // Si hay suscripción activa, extenderla; si no, crear nueva
            var suscripcionActiva = await _context.Suscripciones
                .Where(s => s.IdInmobiliaria == pago.IdInmobiliaria &&
                            s.IdEstado == 1 &&
                            s.Fin >= ahora)
                .OrderByDescending(s => s.Fin)
                .FirstOrDefaultAsync();

            if (suscripcionActiva != null)
            {
                // Extender desde la fecha de fin actual
                suscripcionActiva.Fin = suscripcionActiva.Fin.AddDays(pago.DiasPlan);
                suscripcionActiva.IdPlan = pago.IdPlan;
                suscripcionActiva.ActualizadoEn = ahora;
                await _context.SaveChangesAsync();

                pago.IdSuscripcion = suscripcionActiva.Id;
                pago.ActualizadoEn = ahora;
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Suscripción {Id} extendida hasta {Fin} para inmobiliaria {Tenant}",
                    suscripcionActiva.Id, suscripcionActiva.Fin, pago.IdInmobiliaria);
            }
            else
            {
                var nuevaSuscripcion = new Suscripcion
                {
                    IdInmobiliaria = pago.IdInmobiliaria,
                    IdPlan = pago.IdPlan,
                    Inicio = ahora,
                    Fin = ahora.AddDays(pago.DiasPlan),
                    RenovacionAutomatica = false,
                    IdEstado = 1, // activa
                    CreadoEn = ahora,
                    ActualizadoEn = ahora
                };

                _context.Suscripciones.Add(nuevaSuscripcion);
                await _context.SaveChangesAsync();

                pago.IdSuscripcion = nuevaSuscripcion.Id;
                pago.ActualizadoEn = ahora;
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Nueva suscripción {Id} creada para inmobiliaria {Tenant} hasta {Fin}",
                    nuevaSuscripcion.Id, pago.IdInmobiliaria, nuevaSuscripcion.Fin);
            }
        }

        public async Task<BaseResponseDto<object>> GetEstadoPagoAsync(int idInmobiliaria, string preferenceId)
        {
            var pago = await _context.PagosSuscripcion
                .AsNoTracking()
                .Include(p => p.Plan)
                .FirstOrDefaultAsync(p => p.IdInmobiliaria == idInmobiliaria &&
                                         p.MpPreferenceId == preferenceId);

            if (pago == null)
                return Fail<object>("Pago no encontrado.");

            return Ok<object>(new
            {
                pago.Id,
                pago.Estado,
                pago.MpPaymentId,
                pago.Monto,
                pago.Moneda,
                PlanNombre = pago.Plan?.Nombre,
                pago.IdSuscripcion,
                pago.CreadoEn,
                pago.ActualizadoEn
            }, "Estado del pago obtenido.");
        }

        // Helpers
        private static BaseResponseDto<T> Ok<T>(T data, string message) =>
            new() { Success = true, Data = data, Message = message };

        private static BaseResponseDto<T> Fail<T>(string message) =>
            new() { Success = false, Message = message };
    }
}
