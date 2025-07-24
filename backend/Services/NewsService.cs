using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class NewsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public NewsService(IConfiguration config)
        {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");

        _apiKey = config["NewsApi:ApiKey"]!;
        _baseUrl = config["NewsApi:BaseUrl"]!;
        }

        public async Task<string> GetBusinessNewsAsync()
{
    string query = "economy+finance+banking";
    string url = $"{_baseUrl}/everything?q={query}&language=en&sortBy=publishedAt&apiKey={_apiKey}";    var response = await _httpClient.GetAsync(url);
    return await response.Content.ReadAsStringAsync();
}

    }
}
