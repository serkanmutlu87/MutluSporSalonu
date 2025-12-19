/**
* @file        RandevuApiController.cs
* @description Mutlu Spor Salonu uygulamasında randevu verilerini yöneten REST API controller.
*              JSON formatında çıktı üretir.
*
*              Sağlanan işlevler:
*              - Admin rolü için tüm randevuları listeler.
*              - Üye rolü için sadece kullanıcıya ait randevuları listeler (/benim).
*              - Tarih parametresi ile randevuların filtrelenmesini sağlar.
*              - LINQ, Include ve Select projeksiyonu kullanarak ilişkili verileri birlikte getirir.
*
* @course      WEB PROGRAMLAMA (ASP.NET Core MVC) – 2025-2026 Güz
* @assignment  Dönem Projesi – MutluSporSalonu
* @date        20.12.2025
* @author      D255012008 - Serkan Mutlu
*/

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MutluSporSalonu.Models;

namespace MutluSporSalonu.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")] // /api/randevuapi
    [Authorize] // Kimlik doğrulaması olmayan erişimleri engeller
    public class RandevuApiController : ControllerBase
    {
        private readonly DBContext _context;

        public RandevuApiController(DBContext context)
        {
            _context = context;
        }

        // Kullanıcının Admin rolünde olup olmadığını kontrol eder
        private bool IsAdmin() => User.IsInRole("Admin");

        // Giriş yapan kullanıcıya ait UyeID bilgisini claim üzerinden okur
        private int? GetCurrentUyeId()
        {
            var s = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(s, out var id) ? id : (int?)null;
        }

        // ============================================================
        // GET: /api/randevuapi/benim
        // Giriş yapan üyenin yalnızca kendisine ait randevularını listeler
        // ============================================================
        [HttpGet("benim")]
        public async Task<IActionResult> BenimRandevularim()
        {
            var uyeId = GetCurrentUyeId();
            if (uyeId == null)
                return Unauthorized("Giriş yapan kullanıcı bulunamadı.");

            var liste = await _context.Randevular
                .AsNoTracking() // Sadece okuma işlemi yapıldığı için change tracking kapatılır
                .Where(r => r.UyeID == uyeId.Value)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.SporSalonu)
                .OrderByDescending(r => r.RandevuTarihi)
                .ThenBy(r => r.RandevuBaslangicSaati)
                .Select(r => new
                {
                    randevuId = r.RandevuID,
                    antrenorAdSoyad = r.Antrenor != null ? r.Antrenor.AntrenorAdSoyad : null,
                    hizmetAdi = r.Hizmet != null ? r.Hizmet.HizmetAdi : null,
                    salonAdi = r.SporSalonu != null ? r.SporSalonu.SalonAdi : null,
                    tarih = r.RandevuTarihi.ToString("yyyy-MM-dd"),
                    baslangic = r.RandevuBaslangicSaati.ToString(@"hh\:mm"),
                    bitis = r.RandevuBitisSaati.ToString(@"hh\:mm"),
                    ucret = r.RandevuUcret,
                    onaylandiMi = r.RandevuOnaylandiMi
                })
                .ToListAsync();

            return Ok(liste);
        }

        // ============================================================
        // GET: /api/randevuapi
        // Admin rolüne sahip kullanıcılar için tüm randevuları listeler
        // ============================================================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TumRandevular()
        {
            var liste = await _context.Randevular
                .AsNoTracking()
                .Include(r => r.Uye)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.SporSalonu)
                .OrderByDescending(r => r.RandevuTarihi)
                .ThenBy(r => r.RandevuBaslangicSaati)
                .Select(r => new
                {
                    randevuId = r.RandevuID,
                    uyeAdSoyad = r.Uye != null ? r.Uye.UyeAdSoyad : null,
                    antrenorAdSoyad = r.Antrenor != null ? r.Antrenor.AntrenorAdSoyad : null,
                    hizmetAdi = r.Hizmet != null ? r.Hizmet.HizmetAdi : null,
                    salonAdi = r.SporSalonu != null ? r.SporSalonu.SalonAdi : null,
                    tarih = r.RandevuTarihi.ToString("yyyy-MM-dd"),
                    baslangic = r.RandevuBaslangicSaati.ToString(@"hh\:mm"),
                    bitis = r.RandevuBitisSaati.ToString(@"hh\:mm"),
                    ucret = r.RandevuUcret,
                    onaylandiMi = r.RandevuOnaylandiMi
                })
                .ToListAsync();

            return Ok(liste);
        }

        // ============================================================
        // GET: /api/randevuapi/tarih?tarih=2025-12-20
        // Belirtilen tarihteki randevuları listeler
        // Admin tümünü, üye sadece kendisine ait olanları görür
        // ============================================================
        [HttpGet("tarih")]
        public async Task<IActionResult> TariheGore([FromQuery] string tarih)
        {
            if (!DateTime.TryParse(tarih, out var hedefTarih))
                return BadRequest("Geçerli bir tarih formatı giriniz. Örn: 2025-12-20");

            var query = _context.Randevular
                .AsNoTracking()
                .Where(r => r.RandevuTarihi.Date == hedefTarih.Date)
                .Include(r => r.Uye)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.SporSalonu)
                .AsQueryable();

            // Admin olmayan kullanıcılar için üye filtresi uygulanır
            if (!IsAdmin())
            {
                var uyeId = GetCurrentUyeId();
                if (uyeId == null)
                    return Unauthorized("Giriş yapan kullanıcı bulunamadı.");

                query = query.Where(r => r.UyeID == uyeId.Value);
            }

            var liste = await query
                .OrderBy(r => r.RandevuBaslangicSaati)
                .Select(r => new
                {
                    randevuId = r.RandevuID,
                    uyeAdSoyad = r.Uye != null ? r.Uye.UyeAdSoyad : null,
                    antrenorAdSoyad = r.Antrenor != null ? r.Antrenor.AntrenorAdSoyad : null,
                    hizmetAdi = r.Hizmet != null ? r.Hizmet.HizmetAdi : null,
                    salonAdi = r.SporSalonu != null ? r.SporSalonu.SalonAdi : null,
                    tarih = r.RandevuTarihi.ToString("yyyy-MM-dd"),
                    baslangic = r.RandevuBaslangicSaati.ToString(@"hh\:mm"),
                    bitis = r.RandevuBitisSaati.ToString(@"hh\:mm"),
                    ucret = r.RandevuUcret,
                    onaylandiMi = r.RandevuOnaylandiMi
                })
                .ToListAsync();

            return Ok(liste);
        }
    }
}
