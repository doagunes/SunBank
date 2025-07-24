using System.Text;
using System.Text.Json;

namespace Backend.Services
{
    /*
Bu kod, ASP.NET Core uygulamasında Google Gemini AI API'sini kullanmak için yazılmış bir servis sınıfıdır. 
Dependency Injection pattern'i kullanarak temiz bir mimari sunar.
    */
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeminiService> _logger;
        private readonly string _apiKey;

        public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            //API anahtarı configuration["Gemini:ApiKey"] ile appsettings.json'dan okunur.
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentException("Gemini API key is not configured"); 

        }

        public async Task<string> GetGeminiResponse(string prompt)
        {
            try
            {
                _logger.LogInformation("Sending request to Gemini API");
                
                var requestBody = new
                {
                    contents = new[] //Gemini API'sine gönderilecek içerik
                    {
                        new
                        {
                            parts = new[] //Metin parçaları (burada sadece kullanıcı prompt'u)
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new //AI yanıt ayarları
                    {
                        temperature = 0.7, //Yaratıcılık seviyesi (0-1 arası)
                        maxOutputTokens = 1000 // Maksimum yanıt uzunluğu
                    }
                };

                var json = JsonSerializer.Serialize(requestBody); //request body json formatına çevrilir
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}"; //Gemini API endpoint'ini API anahtarıyla birleştirir
                
                _logger.LogInformation("Making request to: {Url}", url);
                
                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Response status: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Response content: {Content}", responseContent);
                
                if (!response.IsSuccessStatusCode)
                {
                    return $"API Error: {response.StatusCode} - {responseContent}";
                }

                //Yanıt İşleme Kısmı
                var geminiResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                if (geminiResponse.TryGetProperty("candidates", out var candidates) &&
                    candidates.GetArrayLength() > 0)
                {
                    var candidate = candidates[0];
                    if (candidate.TryGetProperty("content", out var contentProp) &&
                        contentProp.TryGetProperty("parts", out var parts) &&
                        parts.GetArrayLength() > 0)
                    {
                        var part = parts[0];
                        if (part.TryGetProperty("text", out var text))
                        {
                            return text.GetString() ?? "No text received";
                        }
                    }
                }

                return "No valid response received from Gemini";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API");
                return $"Error: {ex.Message}";
            }
        }
    }
}