/**
* @file        YapayZekaController.cs
* @description Mutlu Spor Salonu uygulamasında yapay zekâ destekli öneri ekranını yöneten MVC controller.
*              View döndürür.
*
*              Sağlanan işlevler:
*              - Giriş yapmış kullanıcılar için öneri form ekranını açar.
*              - Hedef alanı boş bırakıldığında doğrulama hatası üretir ve formu tekrar gösterir.
*              - Giriş yapan kullanıcının ID bilgisini claim üzerinden alır.
*              - Üye bilgilerini ve randevu geçmişini (Hizmet + Antrenör dahil) veritabanından çeker.
*              - Randevu geçmişini özetleyip yapay zekâ servisine prompt olarak hazırlar.
*              - Sistem mesajı + kullanıcı mesajı formatında yapay zekâ servisini çağırır.
*              - Üretilen öneri metnini ViewBag ile ekrana taşır.
*              - Form alanlarını (hedef / ekNot) geri basarak kullanıcı deneyimini korur.
*
* @course      BSM 311 Web Programlama
* @assignment  Dönem Projesi – MutluSporSalonu
* @date        20.12.2025
* @author      D255012008 - Serkan Mutlu
*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MutluSporSalonu.Models;
using MutluSporSalonu.Services;
using System.Security.Claims;
using System.Text;

namespace MutluSporSalonu.Controllers
{
    [Authorize] // Giriş yapılmadan öneri ekranına erişimi engeller
    public class YapayZekaController : Controller
    {
        private readonly IYapayZekaServisi _yz;
        private readonly DBContext _context;

        public YapayZekaController(IYapayZekaServisi yz, DBContext context)
        {
            _yz = yz;
            _context = context;
        }

        // ------------------------------------------------------------
        // GET: YapayZeka/Oneri
        // Öneri form ekranını açar (model kullanılmaz)
        // ------------------------------------------------------------
        [HttpGet]
        public IActionResult Oneri()
        {
            // Form ekranını görüntüler
            return View();
        }

        // ------------------------------------------------------------
        // POST: YapayZeka/Oneri
        // Kullanıcının hedef ve not bilgisine göre kişiselleştirilmiş öneri metni üretir
        // ------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Oneri(string hedef, string? ekNot)
        {
            // Hedef alanı boşsa hata mesajı üretir ve aynı ekranı tekrar gösterir
            if (string.IsNullOrWhiteSpace(hedef))
            {
                ViewBag.Hata = "Hedef alanı zorunludur.";
                return View();
            }

            // Giriş yapan kullanıcı ID bilgisini claim üzerinden okur
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Üyeyi ve randevularını Hizmet + Antrenör bilgileriyle birlikte getirir
            var uye = await _context.Uyeler
                .Include(u => u.Randevular)
                    .ThenInclude(r => r.Hizmet)
                .Include(u => u.Randevular)
                    .ThenInclude(r => r.Antrenor)
                .FirstOrDefaultAsync(u => u.UyeID == userId);

            if (uye == null)
                return NotFound();

            // Yapay zekâya gidecek metin için özet prompt oluşturur
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
                // Prompt metni gereksiz büyümesin diye en son 10 randevu ile sınırlar
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

            // Sistem mesajı: çıktının kapsamını, formatını ve güvenlik sınırlarını belirler
            string sistem =
                "Sen profesyonel bir spor koçu ve diyetisyensin. " +
                "Üyenin hedefi, notları ve randevu geçmişine göre kişiselleştirilmiş öneri oluştur. " +
                "Çıktıyı Türkçe ver. " +
                "Format: (1) Kısa özet (2) Haftalık egzersiz planı (gün gün) " +
                "(3) Örnek 1 günlük beslenme planı (4) Genel kurallar (5) Güvenlik uyarıları. " +
                "Tıbbi teşhis koyma; riskli durumlarda doktora danış uyarısı ekle.";

            // Kullanıcı mesajı: üyeye ait özet bilgiler + hedef + notlar içerir
            string kullanici = sb.ToString();

            // Yapay zekâ servisini çağırır ve dönen metni ekrana taşır
            ViewBag.OneriMetni = await _yz.MetinOlustur(sistem, kullanici);

            // Form değerlerini tekrar ekrana basar
            ViewBag.Hedef = hedef;
            ViewBag.EkNot = ekNot;

            return View();
        }
    }
}
