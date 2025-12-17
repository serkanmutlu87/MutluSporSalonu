using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MutluSporSalonu.Models;

namespace MutluSporSalonu.Controllers
{
    public class RandevuController : Controller
    {
        private readonly DBContext _context;

        public RandevuController(DBContext context)
        {
            _context = context;
        }

        // GET: Randevu
        public async Task<IActionResult> Index()
        {
            var dBContext = _context.Randevular.Include(r => r.Antrenor).Include(r => r.Hizmet).Include(r => r.Uye).Include(r => r.SporSalonu).OrderByDescending(r => r.RandevuTarihi).ThenBy(r => r.RandevuBaslangicSaati);
            return View(await dBContext.ToListAsync());
        }

        // GET: Randevu/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var randevu = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.Uye)
                .Include(r => r.SporSalonu)
                .FirstOrDefaultAsync(m => m.RandevuID == id);
            if (randevu == null)
            {
                return NotFound();
            }

            return View(randevu);
        }

        // GET: Randevu/Create
        public IActionResult Create()
        {
            HazirlaDropDownListeler();
            return View();
        }

        // POST: Randevu/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RandevuID,UyeID,AntrenorID,HizmetID,SalonID,RandevuTarihi,RandevuBaslangicSaati,RandevuBitisSaati,RandevuUcret,RandevuOnaylandiMi,RandevuAciklama,RandevuOlusturulmaTarihi")] Randevu randevu)
        {

            // DropDown’lar tekrar doldurulsun (hata durumunda da lazım olacak)
            HazirlaDropDownListeler(randevu);

            // Tarih / saat temel validasyon
            if (randevu.RandevuBaslangicSaati >= randevu.RandevuBitisSaati)
            {
                ModelState.AddModelError(string.Empty, "Başlangıç saati, bitiş saatinden önce olmalıdır.");
            }

            // Hizmet ve antrenör bilgilerini çek
            var hizmet = await _context.Hizmetler.FirstOrDefaultAsync(h => h.HizmetID == randevu.HizmetID);
            var antrenor = await _context.Antrenorler.FirstOrDefaultAsync(a => a.AntrenorID == randevu.AntrenorID);

            if (hizmet == null)
            {
                ModelState.AddModelError("HizmetID", "Seçilen hizmet bulunamadı.");
            }

            if (antrenor == null)
            {
                ModelState.AddModelError("AntrenorID", "Seçilen antrenör bulunamadı.");
            }

            // Antrenör müsaitlik kontrolü
            if (antrenor != null)
            {
                if (randevu.RandevuBaslangicSaati < antrenor.AntrenorMusaitlikBaslangic ||
                    randevu.RandevuBitisSaati > antrenor.AntrenorMusaitlikBitis)
                {
                    ModelState.AddModelError(string.Empty,
                        "Seçilen saat aralığı, antrenörün müsaitlik saatleri dışında kalıyor.");
                }
            }

            // Aynı antrenör için çakışan randevu kontrolü
            if (antrenor != null)
            {
                var ayniGundeRandevular = await _context.Randevular
                    .Where(r =>
                        r.AntrenorID == randevu.AntrenorID &&
                        r.RandevuTarihi.Date == randevu.RandevuTarihi.Date)
                    .ToListAsync();

                bool cakismaVar = ayniGundeRandevular.Any(r =>
                    randevu.RandevuBaslangicSaati < r.RandevuBitisSaati &&
                    randevu.RandevuBitisSaati > r.RandevuBaslangicSaati
                );

                if (cakismaVar)
                {
                    ModelState.AddModelError(string.Empty,
                        "Seçilen antrenör bu saat aralığında başka bir randevuya sahiptir.");
                }
            }

            if (!ModelState.IsValid)
            {
                // Hata varsa formu tekrar göster
                return View(randevu);
            }

            // Hizmet ücreti randevuya yazılsın
            if (hizmet != null)
            {
                randevu.RandevuUcret = hizmet.HizmetUcret;
            }

            randevu.RandevuOlusturulmaTarihi = DateTime.Now;
            randevu.RandevuOnaylandiMi = false; // Varsayılan: beklemede

            _context.Randevular.Add(randevu);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Randevu/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null)
            {
                return NotFound();
            }
            HazirlaDropDownListeler(randevu);
            return View(randevu);
        }

        // POST: Randevu/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RandevuID,UyeID,AntrenorID,HizmetID,SalonID,RandevuTarihi,RandevuBaslangicSaati,RandevuBitisSaati,RandevuUcret,RandevuOnaylandiMi,RandevuAciklama,RandevuOlusturulmaTarihi")] Randevu randevu)
        {
            if (id != randevu.RandevuID)
                return NotFound();

            HazirlaDropDownListeler(randevu);

            if (randevu.RandevuBaslangicSaati >= randevu.RandevuBitisSaati)
            {
                ModelState.AddModelError(string.Empty, "Başlangıç saati, bitiş saatinden önce olmalıdır.");
            }

            var hizmet = await _context.Hizmetler.FirstOrDefaultAsync(h => h.HizmetID == randevu.HizmetID);
            var antrenor = await _context.Antrenorler.FirstOrDefaultAsync(a => a.AntrenorID == randevu.AntrenorID);

            if (antrenor != null)
            {
                if (randevu.RandevuBaslangicSaati < antrenor.AntrenorMusaitlikBaslangic ||
                    randevu.RandevuBitisSaati > antrenor.AntrenorMusaitlikBitis)
                {
                    ModelState.AddModelError(string.Empty,
                        "Seçilen saat aralığı, antrenörün müsaitlik saatleri dışında kalıyor.");
                }

                var ayniGundeRandevular = await _context.Randevular
                    .Where(r =>
                        r.AntrenorID == randevu.AntrenorID &&
                        r.RandevuTarihi.Date == randevu.RandevuTarihi.Date &&
                        r.RandevuID != randevu.RandevuID) // kendisi hariç
                    .ToListAsync();

                bool cakismaVar = ayniGundeRandevular.Any(r =>
                    randevu.RandevuBaslangicSaati < r.RandevuBitisSaati &&
                    randevu.RandevuBitisSaati > r.RandevuBaslangicSaati
                );

                if (cakismaVar)
                {
                    ModelState.AddModelError(string.Empty,
                        "Seçilen antrenör bu saat aralığında başka bir randevuya sahiptir.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(randevu);
            }

            if (hizmet != null)
            {
                randevu.RandevuUcret = hizmet.HizmetUcret;
            }

            try
            {
                _context.Update(randevu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Randevular.AnyAsync(r => r.RandevuID == randevu.RandevuID))
                    return NotFound();
                else
                    throw;
            }
        }

        // GET: Randevu/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var randevu = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.Uye)
                .Include(r => r.SporSalonu)
                .FirstOrDefaultAsync(m => m.RandevuID == id);
            if (randevu == null)
            {
                return NotFound();
            }

            return View(randevu);
        }

        // POST: Randevu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                _context.Randevular.Remove(randevu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RandevuExists(int id)
        {
            return _context.Randevular.Any(e => e.RandevuID == id);
        }

        // ===========================================
        //  Yardımcı: DropDown listeleri hazırla
        // ===========================================
        private void HazirlaDropDownListeler(Randevu? randevu = null)
        {
            ViewBag.Uyeler = new SelectList(
                _context.Uyeler.OrderBy(u => u.UyeAdSoyad).ToList(),
                "UyeID",
                "UyeAdSoyad",
                randevu?.UyeID
            );

            ViewBag.Antrenorler = new SelectList(
                _context.Antrenorler
                    .Include(a => a.SporSalonu)
                    .OrderBy(a => a.AntrenorAdSoyad)
                    .ToList(),
                "AntrenorID",
                "AntrenorAdSoyad",
                randevu?.AntrenorID
            );

            ViewBag.Hizmetler = new SelectList(
                _context.Hizmetler.OrderBy(h => h.HizmetAdi).ToList(),
                "HizmetID",
                "HizmetAdi",
                randevu?.HizmetID
            );

            ViewBag.SporSalonlari = new SelectList(
                _context.Salonlar.OrderBy(s => s.SalonAdi).ToList(),
                "SalonID",
                "SalonAdi",
                randevu?.SalonID
            );
        }
    }
}
