using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace MutluSporSalonu.Services
{
    public interface IYapayZekaServisi
    {
        Task<string> MetinOlustur(string sistemMesaji, string kullaniciMesaji);
    }

    public class GeminiYapayZekaServisi : IYapayZekaServisi
    {
        private readonly string _apiKey;
        private readonly string _model;
        private readonly HttpClient _http;

        public GeminiYapayZekaServisi(IConfiguration config)
        {
            _apiKey = config["Gemini:ApiKey"] ?? "";
            _model = config["Gemini:Model"] ?? "gemini-2.0-flash";
            _http = new HttpClient();
        }

        public async Task<string> MetinOlustur(string sistemMesaji, string kullaniciMesaji)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                return "Gemini API key bulunamadı.";

            // Doğru endpoint: v1
            var url =
                $"https://generativelanguage.googleapis.com/v1/models/{_model}:generateContent?key={_apiKey}";

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = sistemMesaji + "\n\n" + kullaniciMesaji }
                        }
                    }
                }
            };

            try
            {
                var response = await _http.PostAsJsonAsync(url, payload);
                var rawJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return $"Gemini API hata: {response.StatusCode}\n\n{rawJson}";
                }

                var json = JsonDocument.Parse(rawJson);

                string text =
                    json.RootElement
                       .GetProperty("candidates")[0]
                       .GetProperty("content")
                       .GetProperty("parts")[0]
                       .GetProperty("text")
                       .GetString();

                return text ?? "Boş yanıt.";
            }
            catch (Exception ex)
            {
                return "Gemini API Exception: " + ex.Message;
            }
        }
    }
}
