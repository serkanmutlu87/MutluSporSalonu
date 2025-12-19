using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MutluSporSalonu.Models;
using MutluSporSalonu.Services;
using System.Security.Claims;
using System.Text;

namespace MutluSporSalonu.Controllers
{
    [Authorize]
    public class YapayZekaController : Controller
    {
        private readonly IYapayZekaServisi _yz;
        private readonly DBContext _context;

        public YapayZekaController(IYapayZekaServisi yz, DBContext context)
        {
            _yz = yz;
            _context = context;
        }

        [HttpGet]
        public IActionResult Oneri()
        {
            // Sadece form ekranını açıyoruz (model yok)
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Oneri(string hedef, string? ekNot)
        {
            // Basit doğrulama (model yoksa bunu sen yapacaksın)
            if (string.IsNullOrWhiteSpace(hedef))
            {
                ViewBag.Hata = "Hedef alanı zorunludur.";
                return View();
            }

            // Giriş yapan kullanıcı ID
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // ✅ Üyeyi + randevularını çek (Hizmet ve Antrenor ile birlikte)
            var uye = await _context.Uyeler
                .Include(u => u.Randevular)
                    .ThenInclude(r => r.Hizmet)
                .Include(u => u.Randevular)
                    .ThenInclude(r => r.Antrenor)
                .FirstOrDefaultAsync(u => u.UyeID == userId);

            if (uye == null)
                return NotFound();

            // Prompt için geçmiş antrenman/hizmet geçmişi özeti
            var sb = new StringBuilder();

            sb.AppendLine($"Üye Ad Soyad: {uye.UyeAdSoyad}");
            sb.AppendLine($"Üye E-posta: {uye.UyeEposta}");
            sb.AppendLine($"Üye Telefon: {(string.IsNullOrWhiteSpace(uye.UyeTelefon) ? "Belirtilmedi" : uye.UyeTelefon)}");
            sb.AppendLine($"Kayıt Tarihi: {uye.KayitTarihi:dd.MM.yyyy}");
            sb.AppendLine();

            sb.AppendLine("Üyenin Randevu Geçmişi (varsa):");
            if (uye.Randevular == null || uye.Randevular.Count == 0)
            {
                sb.AppendLine("- Randevu kaydı yok.");
            }
            else
            {
                // En son 10 randevu yeterli (prompt şişmesin)
                var sonRandevular = uye.Randevular
                    .OrderByDescending(r => r.RandevuTarihi)
                    .Take(10)
                    .ToList();

                foreach (var r in sonRandevular)
                {
                    var hizmetAdi = r.Hizmet?.HizmetAdi ?? "Hizmet belirtilmemiş";
                    var antAdi = r.Antrenor?.AntrenorAdSoyad ?? "Antrenör belirtilmemiş";

                    sb.AppendLine($"- {r.RandevuTarihi:dd.MM.yyyy} | {hizmetAdi} | Antrenör: {antAdi}");
                }
            }

            sb.AppendLine();
            sb.AppendLine($"Hedef: {hedef}");
            sb.AppendLine($"Ek Not: {(string.IsNullOrWhiteSpace(ekNot) ? "Yok" : ekNot)}");

            string sistem =
                "Sen profesyonel bir spor koçu ve diyetisyensin. " +
                "Üyenin hedefi, notları ve randevu geçmişine göre kişiselleştirilmiş öneri oluştur. " +
                "Çıktıyı Türkçe ver. " +
                "Format: (1) Kısa özet (2) Haftalık egzersiz planı (gün gün) " +
                "(3) Örnek 1 günlük beslenme planı (4) Genel kurallar (5) Güvenlik uyarıları. " +
                "Tıbbi teşhis koyma; riskli durumlarda doktora danış uyarısı ekle.";

            string kullanici = sb.ToString();

            ViewBag.OneriMetni = await _yz.MetinOlustur(sistem, kullanici);

            // Form değerlerini geri basmak istersen:
            ViewBag.Hedef = hedef;
            ViewBag.EkNot = ekNot;

            return View();
        }
    }
}
