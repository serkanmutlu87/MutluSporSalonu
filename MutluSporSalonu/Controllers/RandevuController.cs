/**
* @file        RandevuController.cs
* @description Mutlu Spor Salonu uygulamasında randevu kayıtları için CRUD işlemlerini,
*              rol bazlı yetkilendirmeyi (Admin/Uye), dependent dropdown endpointlerini
*              ve randevu onaylama/onay kaldırma iş kurallarını yöneten controller sınıfı.
*
*              Notlar (kısa):
*              - Üye kullanıcı için UyeID server-side set edilir (claim’den alınır); formdan gelen UyeID dikkate alınmaz.
*              - Üye kullanıcı randevuyu "onaylı" oluşturamaz / güncelleyemez (onay sadece Admin’de).
*              - Dependent dropdown: Antrenör seçilince salon + o salondaki hizmetler; Salon seçilince antrenörler + hizmetler döner.
*              - Çakışma kontrolü: Aynı antrenörün aynı günde saat aralığı çakışan randevusu varsa engellenir.
*
* @course      WEB PROGRAMLAMA (ASP.NET Core MVC) – 2025-2026 Güz
* @assignment  Dönem Projesi – MutluSporSalonu
* @date        19.12.2025
* @authors     Serkan Mutlu & Öğrencileri
*/

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MutluSporSalonu.Models;

namespace MutluSporSalonu.Controllers
{
    [Authorize] // Admin + Uye giriş yapmadan randevu işlemi yapılamasın
    public class RandevuController : Controller
    {
        private readonly DBContext _context;

        public RandevuController(DBContext context)
        {
            _context = context;
        }

        // ------------------------------------------------------------
        // Yardımcılar
        // ------------------------------------------------------------
        private bool IsAdmin() => User.IsInRole("Admin");

        // Login sırasında set edilen ClaimTypes.NameIdentifier üzerinden aktif üye ID çekilir
        private int? GetCurrentUyeId()
        {
            var s = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(s, out var id) ? id : (int?)null;
        }

        // Details/Delete gibi ekranlarda ilişkili tablolarla birlikte randevuyu getirir
        private async Task<Randevu?> GetRandevuWithIncludesAsync(int id)
        {
            return await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.Uye)
                .Include(r => r.SporSalonu)
                .FirstOrDefaultAsync(r => r.RandevuID == id);
        }

        // Üye rolünde: sadece kendi randevularını görsün/düzenlesin
        private bool IsOwner(Randevu randevu)
        {
            var uyeId = GetCurrentUyeId();
            return uyeId != null && randevu.UyeID == uyeId.Value;
        }

        // ------------------------------------------------------------
        // Dependent Dropdown Endpoint'leri
        // NOT: Hizmetler salon bazlı filtreleniyor (Hizmet.SalonID varsayımı)
        // ------------------------------------------------------------

        // Antrenör seçilince: antrenörün salonu + o salondaki hizmetler
        [HttpGet]
        public async Task<IActionResult> GetOptionsByAntrenor(int antrenorId)
        {
            var antrenor = await _context.Antrenorler
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AntrenorID == antrenorId);

            if (antrenor == null)
            {
                return Json(new
                {
                    salons = Array.Empty<object>(),
                    services = Array.Empty<object>(),
                    antrenorler = Array.Empty<object>()
                });
            }

            var salonId = antrenor.SalonID;

            var salons = await _context.Salonlar
                .AsNoTracking()
                .Where(s => s.SalonID == salonId)
                .Select(s => new { id = s.SalonID, name = s.SalonAdi })
                .ToListAsync();

            var services = await _context.Hizmetler
                .AsNoTracking()
                .Where(h => h.SalonID == salonId)
                .OrderBy(h => h.HizmetAdi)
                .Select(h => new { id = h.HizmetID, name = h.HizmetAdi })
                .ToListAsync();

            var antrenorler = await _context.Antrenorler
                .AsNoTracking()
                .Where(a => a.AntrenorID == antrenorId)
                .Select(a => new { id = a.AntrenorID, name = a.AntrenorAdSoyad })
                .ToListAsync();

            return Json(new { salons, services, antrenorler });
        }

        // Salon seçilince: o salondaki antrenörler + o salondaki hizmetler
        [HttpGet]
        public async Task<IActionResult> GetOptionsBySalon(int salonId)
        {
            var salons = await _context.Salonlar
                .AsNoTracking()
                .Where(s => s.SalonID == salonId)
                .Select(s => new { id = s.SalonID, name = s.SalonAdi })
                .ToListAsync();

            var antrenorler = await _context.Antrenorler
                .AsNoTracking()
                .Where(a => a.SalonID == salonId)
                .OrderBy(a => a.AntrenorAdSoyad)
                .Select(a => new { id = a.AntrenorID, name = a.AntrenorAdSoyad })
                .ToListAsync();

            var services = await _context.Hizmetler
                .AsNoTracking()
                .Where(h => h.SalonID == salonId)
                .OrderBy(h => h.HizmetAdi)
                .Select(h => new { id = h.HizmetID, name = h.HizmetAdi })
                .ToListAsync();

            return Json(new { salons, antrenorler, services });
        }

        // ============================================================
        // GET: Randevu
        // Admin: tüm randevular
        // Uye: sadece kendi randevuları
        // ============================================================
        public async Task<IActionResult> Index()
        {
            var query = _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.Uye)
                .Include(r => r.SporSalonu)
                .AsQueryable();

            if (!IsAdmin())
            {
                var uyeId = GetCurrentUyeId();
                if (uyeId == null) return Forbid();
                query = query.Where(r => r.UyeID == uyeId.Value);
            }

            var liste = await query
                .OrderByDescending(r => r.RandevuTarihi)
                .ThenBy(r => r.RandevuBaslangicSaati)
                .ToListAsync();

            return View(liste);
        }

        // Admin için sadece bekleyen randevular
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Bekleyenler()
        {
            var liste = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.Uye)
                .Include(r => r.SporSalonu)
                .Where(r => r.RandevuOnaylandiMi == false)
                .OrderBy(r => r.RandevuTarihi)
                .ThenBy(r => r.RandevuBaslangicSaati)
                .ToListAsync();

            return View("Index", liste);
        }

        // ============================================================
        // GET: Randevu/Details/5
        // ============================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await GetRandevuWithIncludesAsync(id.Value);
            if (randevu == null) return NotFound();

            if (!IsAdmin() && !IsOwner(randevu))
                return Forbid();

            return View(randevu);
        }

        // ============================================================
        // GET: Randevu/Create
        // ============================================================
        public IActionResult Create()
        {
            HazirlaDropDownListelerForCurrentUser();
            return View();
        }

        // ============================================================
        // POST: Randevu/Create
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RandevuID,UyeID,AntrenorID,HizmetID,SalonID,RandevuTarihi,RandevuBaslangicSaati,RandevuBitisSaati,RandevuUcret,RandevuOnaylandiMi,RandevuAciklama,RandevuOlusturulmaTarihi")] Randevu randevu)
        {
            // ÜYE ise: UyeID'yi server set eder + ModelState temizlenir
            if (!IsAdmin())
            {
                var uyeId = GetCurrentUyeId();
                if (uyeId == null) return Forbid();

                randevu.UyeID = uyeId.Value;
                ModelState.Remove("UyeID");
                ModelState.Remove(nameof(Randevu.UyeID));

                // Üye asla "onaylı" oluşturamasın
                randevu.RandevuOnaylandiMi = false;
                ModelState.Remove("RandevuOnaylandiMi");
                ModelState.Remove(nameof(Randevu.RandevuOnaylandiMi));
            }

            HazirlaDropDownListelerForCurrentUser(randevu);

            // Saat kontrolü: başlangıç < bitiş olmalı
            if (randevu.RandevuBaslangicSaati >= randevu.RandevuBitisSaati)
                ModelState.AddModelError(string.Empty, "Başlangıç saati, bitiş saatinden önce olmalıdır.");

            var hizmet = await _context.Hizmetler.FirstOrDefaultAsync(h => h.HizmetID == randevu.HizmetID);
            var antrenor = await _context.Antrenorler.FirstOrDefaultAsync(a => a.AntrenorID == randevu.AntrenorID);

            if (hizmet == null) ModelState.AddModelError("HizmetID", "Seçilen hizmet bulunamadı.");
            if (antrenor == null) ModelState.AddModelError("AntrenorID", "Seçilen antrenör bulunamadı.");

            // Dropdown ile filtrelense bile server tarafında salon-antrenör/hizmet uyumu kontrol edilir
            if (antrenor != null && randevu.SalonID > 0 && antrenor.SalonID != randevu.SalonID)
                ModelState.AddModelError(string.Empty, "Seçilen antrenör ile seçilen salon uyuşmuyor.");

            if (hizmet != null && randevu.SalonID > 0 && hizmet.SalonID != randevu.SalonID)
                ModelState.AddModelError(string.Empty, "Seçilen hizmet bu salonda verilmiyor.");

            // Antrenör müsaitlik aralığı kontrolü
            if (antrenor != null)
            {
                if (randevu.RandevuBaslangicSaati < antrenor.AntrenorMusaitlikBaslangic ||
                    randevu.RandevuBitisSaati > antrenor.AntrenorMusaitlikBitis)
                {
                    ModelState.AddModelError(string.Empty,
                        "Seçilen saat aralığı, antrenörün müsaitlik saatleri dışında kalıyor.");
                }
            }

            // Çakışma kontrolü: aynı antrenör + aynı gün + saat aralığı overlap
            if (antrenor != null)
            {
                var ayniGundeRandevular = await _context.Randevular
                    .Where(r =>
                        r.AntrenorID == randevu.AntrenorID &&
                        r.RandevuTarihi.Date == randevu.RandevuTarihi.Date)
                    .ToListAsync();

                bool cakismaVar = ayniGundeRandevular.Any(r =>
                    randevu.RandevuBaslangicSaati < r.RandevuBitisSaati &&
                    randevu.RandevuBitisSaati > r.RandevuBaslangicSaati);

                if (cakismaVar)
                    ModelState.AddModelError(string.Empty,
                        "Seçilen antrenör bu saat aralığında başka bir randevuya sahiptir.");
            }

            if (!ModelState.IsValid)
                return View(randevu);

            // Ücret otomatik
            if (hizmet != null)
                randevu.RandevuUcret = hizmet.HizmetUcret;

            randevu.RandevuOlusturulmaTarihi = DateTime.Now;

            // Varsayılan: beklemede
            if (!IsAdmin())
                randevu.RandevuOnaylandiMi = false;

            _context.Randevular.Add(randevu);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // GET: Randevu/Edit/5
        // ============================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular.FindAsync(id.Value);
            if (randevu == null) return NotFound();

            if (!IsAdmin())
            {
                var uyeId = GetCurrentUyeId();
                if (uyeId == null || randevu.UyeID != uyeId.Value) return Forbid();
            }

            HazirlaDropDownListelerForCurrentUser(randevu);
            return View(randevu);
        }

        // ============================================================
        // POST: Randevu/Edit/5
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RandevuID,UyeID,AntrenorID,HizmetID,SalonID,RandevuTarihi,RandevuBaslangicSaati,RandevuBitisSaati,RandevuUcret,RandevuOnaylandiMi,RandevuAciklama,RandevuOlusturulmaTarihi")] Randevu randevu)
        {
            if (id != randevu.RandevuID) return NotFound();

            // Üye: sadece kendi randevusu + onay alanını değiştiremez
            if (!IsAdmin())
            {
                var uyeId = GetCurrentUyeId();
                if (uyeId == null) return Forbid();

                var mevcut = await _context.Randevular.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.RandevuID == id);

                if (mevcut == null) return NotFound();
                if (mevcut.UyeID != uyeId.Value) return Forbid();

                randevu.UyeID = uyeId.Value;
                ModelState.Remove("UyeID");
                ModelState.Remove(nameof(Randevu.UyeID));

                // Üye onayı değiştiremesin: DB'deki değeri koru
                randevu.RandevuOnaylandiMi = mevcut.RandevuOnaylandiMi;
                ModelState.Remove("RandevuOnaylandiMi");
                ModelState.Remove(nameof(Randevu.RandevuOnaylandiMi));
            }

            HazirlaDropDownListelerForCurrentUser(randevu);

            // Saat kontrolü: başlangıç < bitiş olmalı
            if (randevu.RandevuBaslangicSaati >= randevu.RandevuBitisSaati)
                ModelState.AddModelError(string.Empty, "Başlangıç saati, bitiş saatinden önce olmalıdır.");

            var hizmet = await _context.Hizmetler.FirstOrDefaultAsync(h => h.HizmetID == randevu.HizmetID);
            var antrenor = await _context.Antrenorler.FirstOrDefaultAsync(a => a.AntrenorID == randevu.AntrenorID);

            if (antrenor != null && randevu.SalonID > 0 && antrenor.SalonID != randevu.SalonID)
                ModelState.AddModelError(string.Empty, "Seçilen antrenör ile seçilen salon uyuşmuyor.");

            if (hizmet != null && randevu.SalonID > 0 && hizmet.SalonID != randevu.SalonID)
                ModelState.AddModelError(string.Empty, "Seçilen hizmet bu salonda verilmiyor.");

            if (antrenor != null)
            {
                // Antrenör müsaitlik aralığı kontrolü
                if (randevu.RandevuBaslangicSaati < antrenor.AntrenorMusaitlikBaslangic ||
                    randevu.RandevuBitisSaati > antrenor.AntrenorMusaitlikBitis)
                {
                    ModelState.AddModelError(string.Empty,
                        "Seçilen saat aralığı, antrenörün müsaitlik saatleri dışında kalıyor.");
                }

                // Çakışma kontrolü (edit): kendi kaydını hariç tut
                var ayniGundeRandevular = await _context.Randevular
                    .Where(x =>
                        x.AntrenorID == randevu.AntrenorID &&
                        x.RandevuTarihi.Date == randevu.RandevuTarihi.Date &&
                        x.RandevuID != randevu.RandevuID)
                    .ToListAsync();

                bool cakismaVar = ayniGundeRandevular.Any(x =>
                    randevu.RandevuBaslangicSaati < x.RandevuBitisSaati &&
                    randevu.RandevuBitisSaati > x.RandevuBaslangicSaati);

                if (cakismaVar)
                    ModelState.AddModelError(string.Empty,
                        "Seçilen antrenör bu saat aralığında başka bir randevuya sahiptir.");
            }

            if (!ModelState.IsValid)
                return View(randevu);

            if (hizmet != null)
                randevu.RandevuUcret = hizmet.HizmetUcret;

            try
            {
                _context.Update(randevu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Randevular.AnyAsync(x => x.RandevuID == randevu.RandevuID))
                    return NotFound();
                throw;
            }
        }

        // ============================================================
        // ONAYLA / ONAY KALDIR  (SADECE ADMIN)
        // ============================================================

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Onayla(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            randevu.RandevuOnaylandiMi = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnayKaldir(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            randevu.RandevuOnaylandiMi = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // GET: Randevu/Delete/5
        // ============================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await GetRandevuWithIncludesAsync(id.Value);
            if (randevu == null) return NotFound();

            if (!IsAdmin() && !IsOwner(randevu))
                return Forbid();

            return View(randevu);
        }

        // ============================================================
        // POST: Randevu/Delete/5
        // ============================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            if (!IsAdmin())
            {
                var uyeId = GetCurrentUyeId();
                if (uyeId == null || randevu.UyeID != uyeId.Value) return Forbid();
            }

            _context.Randevular.Remove(randevu);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // DropDown listeler
        // ============================================================
        private void HazirlaDropDownListelerForCurrentUser(Randevu? randevu = null)
        {
            if (IsAdmin())
            {
                HazirlaDropDownListeler(randevu);
                return;
            }

            var uyeId = GetCurrentUyeId();
            if (uyeId == null) return;

            // Üye için: sadece kendisini seçebilir (formda başka üye görünmez)
            ViewBag.Uyeler = new SelectList(
                _context.Uyeler.Where(u => u.UyeID == uyeId.Value).ToList(),
                "UyeID", "UyeAdSoyad", uyeId.Value);

            ViewBag.Antrenorler = new SelectList(
                _context.Antrenorler.OrderBy(a => a.AntrenorAdSoyad).ToList(),
                "AntrenorID", "AntrenorAdSoyad", randevu?.AntrenorID);

            ViewBag.Hizmetler = new SelectList(
                _context.Hizmetler.OrderBy(h => h.HizmetAdi).ToList(),
                "HizmetID", "HizmetAdi", randevu?.HizmetID);

            ViewBag.SporSalonlari = new SelectList(
                _context.Salonlar.OrderBy(s => s.SalonAdi).ToList(),
                "SalonID", "SalonAdi", randevu?.SalonID);
        }

        private void HazirlaDropDownListeler(Randevu? randevu = null)
        {
            ViewBag.Uyeler = new SelectList(
                _context.Uyeler.OrderBy(u => u.UyeAdSoyad).ToList(),
                "UyeID", "UyeAdSoyad", randevu?.UyeID);

            ViewBag.Antrenorler = new SelectList(
                _context.Antrenorler.OrderBy(a => a.AntrenorAdSoyad).ToList(),
                "AntrenorID", "AntrenorAdSoyad", randevu?.AntrenorID);

            ViewBag.Hizmetler = new SelectList(
                _context.Hizmetler.OrderBy(h => h.HizmetAdi).ToList(),
                "HizmetID", "HizmetAdi", randevu?.HizmetID);

            ViewBag.SporSalonlari = new SelectList(
                _context.Salonlar.OrderBy(s => s.SalonAdi).ToList(),
                "SalonID", "SalonAdi", randevu?.SalonID);
        }
    }
}
