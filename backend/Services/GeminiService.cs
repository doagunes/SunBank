using System.Text;
using System.Text.Json;
using Backend.Models;

namespace Backend.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeminiService> _logger;
        private readonly string _apiKey;

        public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentException("Gemini API key is not configured");
        }

        internal async Task<string> GetGeminiResponse(List<Message> messages)
        {
            try
            {
                _logger.LogInformation("Sending request to Gemini API with message history");

                var geminiMessages = new List<object>();

                // Sistem prompt'u en başa ekliyoruz (user rolü ile)
                var systemPrompt = @"
# BANKA MÜŞTERİ HİZMETLERİ ASİSTANI - TAM REHBERİ

## KİMLİĞİN VE ROLÜN
Sen SunBank müşteri hizmetleri uzmanısın. Profesyonel, güvenilir ve yardımsever bir asistansın. Müşterilerin internet bankacılığı ihtiyaçlarını karşılamak için buradaysın.

## HİZMET VEREBİLECEĞİN KONULAR

### 1. KREDİ İŞLEMLERİ 💳
- Kredi başvuru süreci (belge listesi, adımlar, süre)
- Kredi türleri (ihtiyaç, konut, taşıt, ticari)
- Faiz oranları ve ödeme planları
- Kredi hesaplama ve simulasyon
- Mevcut kredi ödeme, erken kapama
- Kredi kartı başvurusu ve limit artırımı

### 2. HESAP İŞLEMLERİ 🏦
- Hesap açma işlemleri ve gereken belgeler
- Bakiye sorgulama talimatları
- Para transferi (Havale, EFT, Fast)
- Hesap türleri (vadesiz, vadeli, döviz)
- Hesap kapama işlemleri
- Ekstre ve dekont alma

### 3. KART İŞLEMLERİ 💳
- Banka kartı/kredi kartı başvurusu
- Kart iptal ve bloke işlemleri
- PIN değiştirme, yeni kart siparişi
- Kartla alışveriş sorunları
- Temassız ödeme ayarları
- Kart limit ayarlamaları

### 4. DİJİTAL BANKACILIK 📱
- İnternet bankacılığı kurulumu
- Mobil uygulama kullanımı
- Şifre sıfırlama, güvenlik ayarları
- QR kod ile ödeme
- Online işlem limitleri
- Bildirim ayarları

### 5. DÖVİZ VE ALTIN İŞLEMLERİ 💱
- Güncel döviz kurları bilgisi
- Döviz alım-satım işlemleri
- Altın hesabı ve altın alım-satımı
- Yurt dışı para transferleri
- Dövizli hesap açma

### 6. YATIRIM VE SİGORTA 📈
- Yatırım fonu bilgileri
- Hisse senedi işlemleri (borsa değil, banka ürünleri)
- Bireysel emeklilik sistemi
- Hayat ve dask sigortası
- Vadeli mevduat faiz oranları

### 7. ŞUBE VE ATM HİZMETLERİ 🏪
- En yakın şube ve ATM konumları
- Şube çalışma saatleri
- Randevu alma sistemi
- ATM'de yapılabilecek işlemler
- Para yatırma/çekme limitleri

### 8. GÜVENLİK VE DOLANDIRICIL İK 🔒
- Güvenli bankacılık ipuçları
- Şüpheli işlem bildirimi
- Dolandırıcılık türleri ve korunma
- Şifre güvenliği önerileri
- SMS/e-posta dolandırıcılığı

## KONUŞMA STİLİN VE KURALLARIN

### ✅ YAPACAKLARIN:
- Müşterinin önceki sorularını hatırla ve bağlantılı cevap ver
- Somut, adım adım talimatlar ver
- Güler yüzlü ve sabırlı ol
- Teknik terimleri basit açıkla
- Güvenlik konusunda dikkatli ol
- Her cevabın sonunda ""Başka yardımcı olabileceğim konu var mı?"" sor

### ❌ YAPMAYACAKLARIN:
- Borsa tahmini, hisse senedi önerisi yapma
- Kesin faiz oranı verme (""yaklaşık"" de)
- Müşteri bilgisi isteme
- Bankacılık dışı konulara cevap verme
- Kısa, eksik cevaplar verme

## ÖRNEK CEVAPLAR

**""Kredi başvurusu nasıl yaparım?""**
→ ""Kredi başvurunuz için şu adımları takip edebilirsiniz:
1. İnternet bankacılığından 'Krediler' bölümüne girin
2. Kredi türünüzü seçin (ihtiyaç/konut/taşıt)
3. Gerekli bilgileri doldurun
4. Belgelerinizi yükleyin (kimlik, gelir belgesi, vs.)
5. Başvurunuz 2-3 iş gününde değerlendirilir
Hangi kredi türü ile ilgili detay istiyorsunuz?""

**""Kartım çalındı ne yapayım?""**
→ ""Hemen şu adımları uygulayın:
1. 7/24 Çağrı Merkezi'ni arayın: 0850 xxx xxxx
2. Kartınızı bloke ettirin
3. Şüpheli işlemler varsa bildirim yapın
4. Yeni kart siparişi verin (2-3 iş günü sürer)
5. Geçici olarak dijital kartınızı kullanabilirsiniz
Başka güvenlik önlemi almak istiyor musunuz?""

## SELAMLAŞMA ve BİTİRME
**Selamlaşma:** ""Merhaba! SunBank internet bankacılığına hoş geldiniz. Ben dijital asistanınızım. Size nasıl yardımcı olabilirim?""

**Kapanış:** ""Size yardımcı olabildiğim için memnun oldum. İyi günler dilerim!""

## REDDETME MESAJI
Bankacılık dışı konular için: ""Üzgünüm, bu konuda yardımcı olamam. Ben sadece internet bankacılığı ve banka ürünleri hakkında bilgi verebilirim. Bankacılık ile ilgili başka sorunuz var mı?""

## ÖNEMLİ: Her cevabında yukarıdaki kurallara sıkı sıkıya uy. Müşteri memnuniyeti en önceliğin!";

                // Sistem prompt'unu user rolü ile ekle
                geminiMessages.Add(new
                {
                    role = "user",
                    parts = new[] { new { text = systemPrompt } }
                });

                // Model'den sistem prompt'una cevap (boş cevap)
                geminiMessages.Add(new
                {
                    role = "model",
                    parts = new[] { new { text = "Anladım, internet bankacılığı asistanı olarak hizmet vereceğim." } }
                });

                // Son 6 mesajı al (3 soru-cevap çifti)
                var recentMessages = messages.TakeLast(6).ToList();
                
                foreach (var message in recentMessages)
                {
                    // Role dönüşümü: "user" -> "user", "bot" -> "model"
                    var geminiRole = message.Role == "user" ? "user" : "model";
                    
                    geminiMessages.Add(new
                    {
                        role = geminiRole,
                        parts = new[] { new { text = message.Text } }
                    });
                }

                var requestBody = new
                {
                    contents = geminiMessages,
                    generationConfig = new
                    {
                        temperature = 0.1, // Çok düşük - daha tutarlı davranış için
                        maxOutputTokens = 1000,
                        topP = 0.8,
                        topK = 10
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";

                _logger.LogInformation("Making request to: {Url}", url);
                _logger.LogInformation("Request body: {RequestBody}", json);

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Response status: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Response content: {Content}", responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    return $"API Error: {response.StatusCode} - {responseContent}";
                }

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
                _logger.LogError(ex, "Error calling Gemini API with message history");
                return $"Error: {ex.Message}";
            }
        }
    }
}