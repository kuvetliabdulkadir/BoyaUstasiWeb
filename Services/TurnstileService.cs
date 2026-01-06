using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;

namespace boya_usta_web.Services
{
    // Cloudflare Turnstile CAPTCHA doğrulama ve Rate Limiting servisi
    public class TurnstileService : ITurnstileService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TurnstileService> _logger;
        
        // Cloudflare Turnstile doğrulama endpoint'i
        private const string VerifyUrl = "https://challenges.cloudflare.com/turnstile/v0/siteverify";



        public bool IsEnabled => _configuration.GetValue<bool>("Recaptcha:Enabled");
        
        public string SiteKey => _configuration["Recaptcha:SiteKey"] ?? "";
        
        public int ThresholdCount => _configuration.GetValue<int>("Recaptcha:ThresholdCount", 2);
        
        public int ThresholdMinutes => _configuration.GetValue<int>("Recaptcha:ThresholdMinutes", 10);

        // Token doğrulama işlemi
        public async Task<bool> ValidateTokenAsync(string token, string? remoteIp = null)
        {
            // CAPTCHA devre dışıysa her zaman başarılı say
            if (!IsEnabled)
            {
                _logger.LogDebug("Turnstile devre dışı, doğrulama atlandı.");
                return true;
            }

            // Token boşsa başarısız
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Turnstile token boş.");
                return false;
            }

            var secretKey = _configuration["Recaptcha:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                _logger.LogWarning("Turnstile SecretKey yapılandırılmamış.");
                return false;
            }

            try
            {
                var formData = new Dictionary<string, string>
                {
                    { "secret", secretKey },
                    { "response", token }
                };

                if (!string.IsNullOrEmpty(remoteIp))
                {
                    formData.Add("remoteip", remoteIp);
                }

                var content = new FormUrlEncodedContent(formData);
                var response = await _httpClient.PostAsync(VerifyUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Turnstile API yanıt vermedi. StatusCode: {StatusCode}", response.StatusCode);
                    return false;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TurnstileResponse>(responseContent);

                if (result == null)
                {
                    _logger.LogError("Turnstile API yanıtı parse edilemedi.");
                    return false;
                }

                if (!result.Success)
                {
                    _logger.LogWarning("Turnstile doğrulama başarısız. Hatalar: {Errors}", 
                        string.Join(", ", result.ErrorCodes ?? Array.Empty<string>()));
                }

                return result.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Turnstile doğrulama sırasında hata oluştu.");
                return false;
            }
        }

        // Model
        private class TurnstileResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("error-codes")]
            public string[]? ErrorCodes { get; set; }

            [JsonPropertyName("challenge_ts")]
            public string? ChallengeTimestamp { get; set; }

            [JsonPropertyName("hostname")]
            public string? Hostname { get; set; }
        }

        // ==========================================
        // Rate Limiting Mantığı
        // ==========================================
        private readonly Microsoft.Extensions.Caching.Distributed.IDistributedCache _cache;
        private const string CacheKeyPrefix = "ContactFormSubmissions_";

        // Constructor'a IDistributedCache eklendi
        public TurnstileService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<TurnstileService> logger,
            Microsoft.Extensions.Caching.Distributed.IDistributedCache cache)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _cache = cache;
        }

        public bool ShouldShowCaptcha(string ipAddress)
        {
            try
            {
                if (!IsEnabled) return false;
                var recentCount = GetRecentSubmissionCount(ipAddress);
                return recentCount >= ThresholdCount;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ShouldShowCaptcha hatası - false döndürülüyor.");
                return false;
            }
        }

        public int GetRecentSubmissionCount(string ipAddress)
        {
            if (_cache == null || string.IsNullOrEmpty(ipAddress)) return 0;

            try
            {
                var cacheKey = $"{CacheKeyPrefix}{ipAddress}";
                var submissionTimesJson = _cache.GetString(cacheKey);
                if (string.IsNullOrEmpty(submissionTimesJson)) return 0;

                var submissionTimes = JsonSerializer.Deserialize<List<long>>(submissionTimesJson) ?? new List<long>();
                var cutoffTime = DateTimeOffset.UtcNow.AddMinutes(-ThresholdMinutes).ToUnixTimeSeconds();

                return submissionTimes.Count(t => t > cutoffTime);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GetRecentSubmissionCount hatası.");
                return 0;
            }
        }

        public void RecordFormSubmission(string ipAddress)
        {
            if (_cache == null || string.IsNullOrEmpty(ipAddress)) return;

            try
            {
                var cacheKey = $"{CacheKeyPrefix}{ipAddress}";
                var submissionTimesJson = _cache.GetString(cacheKey);
                var submissionTimes = string.IsNullOrEmpty(submissionTimesJson)
                    ? new List<long>()
                    : JsonSerializer.Deserialize<List<long>>(submissionTimesJson) ?? new List<long>();

                var cutoffTime = DateTimeOffset.UtcNow.AddMinutes(-ThresholdMinutes).ToUnixTimeSeconds();
                
                // Eskileri temizle ve yeniyi ekle
                submissionTimes = submissionTimes.Where(t => t > cutoffTime).ToList();
                submissionTimes.Add(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                var cacheOptions = new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(ThresholdMinutes + 1)
                };

                _cache.SetString(cacheKey, JsonSerializer.Serialize(submissionTimes), cacheOptions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RecordFormSubmission hatası.");
            }
        }
    }
}

