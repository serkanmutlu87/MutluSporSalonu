/**
* @file        GeminiYapayZekaServisi.cs
* @description Mutlu Spor Salonu uygulamasında yapay zekâ servis katmanını temsil eden sınıf.
*              Google Gemini API ile haberleşerek metin tabanlı öneri üretimini sağlar.
*
*              Sağlanan işlevler:
*              - Yapay zekâ servisleri için ortak bir arayüz (IYapayZekaServisi) tanımlar.
*              - Sistem mesajı ve kullanıcı mesajını birleştirerek yapay zekâya gönderir.
*              - Gemini API üzerinden HTTP POST isteği ile içerik üretimi talep eder.
*              - API anahtarı ve model bilgilerini IConfiguration üzerinden okur.
*              - API yanıtını JSON olarak ayrıştırır ve üretilen metni string olarak döndürür.
*              - Hata ve istisna durumlarında anlamlı hata mesajı üretir.
*
* @course      BSM 311 Web Programlama
* @assignment  Dönem Projesi – MutluSporSalonu
* @date        20.12.2025
* @author      D255012008 - Serkan Mutlu
*/

using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace MutluSporSalonu.Services
{
    // ------------------------------------------------------------
    // Yapay zekâ servisleri için ortak arayüz
    // ------------------------------------------------------------
    public interface IYapayZekaServisi
    {
        // Sistem ve kullanıcı mesajlarını alır, üretilen metni döndürür
        Task<string> MetinOlustur(string sistemMesaji, string kullaniciMesaji);
    }

    // ------------------------------------------------------------
    // Google Gemini API tabanlı yapay zekâ servisi
    // ------------------------------------------------------------
    public class GeminiYapayZekaServisi : IYapayZekaServisi
    {
        private readonly string _apiKey;
        private readonly string _model;
        private readonly HttpClient _http;

        public GeminiYapayZekaServisi(IConfiguration config)
        {
            // API anahtarı ve model bilgisi uygulama ayarlarından okunur
            _apiKey = config["Gemini:ApiKey"] ?? "";
            _model = config["Gemini:Model"] ?? "gemini-2.0-flash";

            // HTTP istekleri için HttpClient oluşturulur
            _http = new HttpClient();
        }

        // ------------------------------------------------------------
        // Yapay zekâdan metin üretir
        // ------------------------------------------------------------
        public async Task<string> MetinOlustur(string sistemMesaji, string kullaniciMesaji)
        {
            // API anahtarı yoksa servis çağrısı yapılmaz
            if (string.IsNullOrWhiteSpace(_apiKey))
                return "Gemini API key bulunamadı.";

            // Gemini API v1 endpoint adresi
            var url =
                $"https://generativelanguage.googleapis.com/v1/models/{_model}:generateContent?key={_apiKey}";

            // Gemini API beklenen istek formatı
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
                // HTTP POST isteği gönderilir
                var response = await _http.PostAsJsonAsync(url, payload);
                var rawJson = await response.Content.ReadAsStringAsync();

                // HTTP hata durumları kontrol edilir
                if (!response.IsSuccessStatusCode)
                {
                    return $"Gemini API hata: {response.StatusCode}\n\n{rawJson}";
                }

                // JSON yanıt ayrıştırılır
                var json = JsonDocument.Parse(rawJson);

                string text =
                    json.RootElement
                       .GetProperty("candidates")[0]
                       .GetProperty("content")
                       .GetProperty("parts")[0]
                       .GetProperty("text")
                       .GetString();

                // Üretilen metin döndürülür
                return text ?? "Boş yanıt.";
            }
            catch (Exception ex)
            {
                // Ağ, JSON veya API kaynaklı hatalar yakalanır
                return "Gemini API Exception: " + ex.Message;
            }
        }
    }
}
