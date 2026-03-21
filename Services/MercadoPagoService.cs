using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace inmobiliariaApi.Services
{
    // ── DTOs internos de MercadoPago ─────────────────────────────────────────

    public class MpPreferenceRequest
    {
        [JsonPropertyName("items")]
        public List<MpItem> Items { get; set; } = new();

        [JsonPropertyName("payer")]
        public MpPayer? Payer { get; set; }

        [JsonPropertyName("back_urls")]
        public MpBackUrls? BackUrls { get; set; }

        [JsonPropertyName("auto_return")]
        public string AutoReturn { get; set; } = "approved";

        [JsonPropertyName("notification_url")]
        public string? NotificationUrl { get; set; }

        [JsonPropertyName("external_reference")]
        public string? ExternalReference { get; set; }

        [JsonPropertyName("expires")]
        public bool Expires { get; set; } = true;

        [JsonPropertyName("expiration_date_to")]
        public string? ExpirationDateTo { get; set; }
    }

    public class MpItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; } = 1;

        [JsonPropertyName("unit_price")]
        public decimal UnitPrice { get; set; }

        [JsonPropertyName("currency_id")]
        public string CurrencyId { get; set; } = "ARS";
    }

    public class MpPayer
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class MpBackUrls
    {
        [JsonPropertyName("success")]
        public string Success { get; set; } = string.Empty;

        [JsonPropertyName("failure")]
        public string Failure { get; set; } = string.Empty;

        [JsonPropertyName("pending")]
        public string Pending { get; set; } = string.Empty;
    }

    public class MpPreferenceResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("init_point")]
        public string InitPoint { get; set; } = string.Empty;

        [JsonPropertyName("sandbox_init_point")]
        public string SandboxInitPoint { get; set; } = string.Empty;
    }

    public class MpPaymentResponse
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("status_detail")]
        public string StatusDetail { get; set; } = string.Empty;

        [JsonPropertyName("external_reference")]
        public string? ExternalReference { get; set; }

        [JsonPropertyName("transaction_amount")]
        public decimal TransactionAmount { get; set; }

        [JsonPropertyName("currency_id")]
        public string CurrencyId { get; set; } = string.Empty;
    }

    // ── Servicio ─────────────────────────────────────────────────────────────

    public class MercadoPagoService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MercadoPagoService> _logger;

        private string AccessToken =>
            _configuration["MercadoPago:AccessToken"]
                ?? throw new InvalidOperationException("MercadoPago:AccessToken no configurado.");

        private string WebhookSecret =>
            _configuration["MercadoPago:WebhookSecret"] ?? string.Empty;

        private bool IsSandbox =>
            !string.Equals(_configuration["MercadoPago:Environment"], "production", StringComparison.OrdinalIgnoreCase);

        public MercadoPagoService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<MercadoPagoService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<MpPreferenceResponse> CreatePreferenceAsync(MpPreferenceRequest request)
        {
            var client = CreateClient();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.mercadopago.com/checkout/preferences", content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("MercadoPago CreatePreference error {Status}: {Body}", response.StatusCode, body);
                throw new Exception($"MercadoPago error {response.StatusCode}: {body}");
            }

            return JsonSerializer.Deserialize<MpPreferenceResponse>(body)
                ?? throw new Exception("Respuesta inválida de MercadoPago.");
        }

        public async Task<MpPaymentResponse> GetPaymentAsync(long paymentId)
        {
            var client = CreateClient();
            var response = await client.GetAsync($"https://api.mercadopago.com/v1/payments/{paymentId}");
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("MercadoPago GetPayment error {Status}: {Body}", response.StatusCode, body);
                throw new Exception($"MercadoPago GetPayment error {response.StatusCode}");
            }

            return JsonSerializer.Deserialize<MpPaymentResponse>(body)
                ?? throw new Exception("Respuesta inválida de MercadoPago.");
        }

        // Verifica la firma HMAC-SHA256 del webhook
        // Documentación MP: https://www.mercadopago.com.ar/developers/es/docs/your-integrations/notifications/webhooks
        public bool VerifyWebhookSignature(string xSignature, string xRequestId, string dataId)
        {
            if (string.IsNullOrEmpty(WebhookSecret)) return true; // sin secret configurado: skip

            try
            {
                // Extraer ts y v1 del header "ts=...,v1=..."
                var parts = xSignature.Split(',');
                string? ts = null, v1 = null;
                foreach (var part in parts)
                {
                    var kv = part.Trim().Split('=', 2);
                    if (kv.Length == 2)
                    {
                        if (kv[0] == "ts") ts = kv[1];
                        if (kv[0] == "v1") v1 = kv[1];
                    }
                }

                if (ts == null || v1 == null) return false;

                var manifest = $"id:{dataId};request-id:{xRequestId};ts:{ts};";
                var secretBytes = Encoding.UTF8.GetBytes(WebhookSecret);
                var manifestBytes = Encoding.UTF8.GetBytes(manifest);

                using var hmac = new HMACSHA256(secretBytes);
                var hash = hmac.ComputeHash(manifestBytes);
                var computed = Convert.ToHexString(hash).ToLower();

                return computed == v1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar firma del webhook de MercadoPago");
                return false;
            }
        }

        public string GetCheckoutUrl(MpPreferenceResponse preference) =>
            IsSandbox ? preference.SandboxInitPoint : preference.InitPoint;

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient("MercadoPago");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AccessToken);
            return client;
        }
    }
}
