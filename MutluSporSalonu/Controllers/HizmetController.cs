/**
* @file        HizmetController.cs
* @description Mutlu Spor Salonu uygulamasında hizmet (paket/servis) yönetimini yapan MVC controller.
*              View döndürür, JSON değil.
*
*              Sağlanan işlevler:
*              - Hizmet listesini salon bilgisiyle birlikte görüntüler.
*              - Hizmet detayını görüntüler.
*              - Admin rolü için hizmet ekleme, güncelleme ve silme işlemlerine izin verir.
*              - Üye rolü için listeleme ve detay görüntülemeye izin verir.
*              - [Authorize] ve rol bazlı yetkilendirme ile erişim kontrolü uygular.
*
* @course      BSM 311 Web Programlama
* @assignment  Dönem Projesi – MutluSporSalonu
* @date        20.12.2025
* @author      D255012008 - Serkan Mutlu
*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MutluSporSalonu.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MutluSporSalonu.Controllers
{
    [Authorize] // Kimlik doğrulaması olmayan erişimleri engeller
    public class HizmetController : Controller
    {
        private readonly DBContext _context;

        public HizmetController(DBContext context)
        {
            _context = context;
        }

        // ------------------------------------------------------------
        // GET: Hizmet
        // Hizmet listesini salon bilgisiyle birlikte görüntüler
        // Admin ve Uye rollerine açıktır
        // ------------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var dBContext = _context.Hizmetler.Include(h => h.SporSalonu);
            return View(await dBContext.ToListAsync());
        }

        // ------------------------------------------------------------
        // GET: Hizmet/Details/5
        // Seçilen hizmetin detay bilgilerini görüntüler
        // Admin ve Uye rollerine açıktır
        // ------------------------------------------------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var hizmet = await _context.Hizmetler
                .Include(h => h.SporSalonu)
                .FirstOrDefaultAsync(m => m.HizmetID == id);

            if (hizmet == null) return NotFound();

            return View(hizmet);
        }

        // ------------------------------------------------------------
        // GET: Hizmet/Create
        // Yeni hizmet ekleme ekranını açar
        // Sadece Admin rolüne izin verir
        // ------------------------------------------------------------
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.SalonID = new SelectList(_context.Salonlar.ToList(), "SalonID", "SalonAdi");
            return View();
        }

        // ------------------------------------------------------------
        // POST: Hizmet/Create
        // Yeni hizmet kaydını veritabanına ekler
        // Model doğrulaması başarılıysa kayıt işlemini tamamlar
        // Sadece Admin rolüne izin verir
        // ------------------------------------------------------------
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HizmetID,HizmetAdi,HizmetSureDakika,HizmetUcret,HizmetAciklama,SalonID")] Hizmet hizmet)
        {
            if (ModelState.IsValid)
            {
                _context.Add(hizmet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.SalonID = new SelectList(_context.Salonlar, "SalonID", "SalonAdi", hizmet.SalonID);
            return View(hizmet);
        }

        // ------------------------------------------------------------
        // GET: Hizmet/Edit/5
        // Seçilen hizmetin düzenleme ekranını açar
        // Sadece Admin rolüne izin verir
        // ------------------------------------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet == null) return NotFound();

            ViewBag.SalonID = new SelectList(_context.Salonlar, "SalonID", "SalonAdi", hizmet.SalonID);
            return View(hizmet);
        }

        // ------------------------------------------------------------
        // POST: Hizmet/Edit/5
        // Hizmet bilgilerini günceller
        // Eş zamanlı güncelleme hatalarını kontrol eder
        // Sadece Admin rolüne izin verir
        // ------------------------------------------------------------
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HizmetID,HizmetAdi,HizmetSureDakika,HizmetUcret,HizmetAciklama,SalonID")] Hizmet hizmet)
        {
            if (id != hizmet.HizmetID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hizmet);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HizmetExists(hizmet.HizmetID))
                        return NotFound();
                    throw;
                }
            }

            ViewBag.SalonID = new SelectList(_context.Salonlar, "SalonID", "SalonAdi", hizmet.SalonID);
            return View(hizmet);
        }

        // ------------------------------------------------------------
        // GET: Hizmet/Delete/5
        // Hizmet silme onay ekranını açar
        // Sadece Admin rolüne izin verir
        // ------------------------------------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var hizmet = await _context.Hizmetler
                .Include(h => h.SporSalonu)
                .FirstOrDefaultAsync(m => m.HizmetID == id);

            if (hizmet == null) return NotFound();

            return View(hizmet);
        }

        // ------------------------------------------------------------
        // POST: Hizmet/Delete/5
        // Seçilen hizmet kaydını veritabanından siler
        // Sadece Admin rolüne izin verir
        // ------------------------------------------------------------
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet != null)
                _context.Hizmetler.Remove(hizmet);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Hizmet kaydının veritabanında var olup olmadığını kontrol eder
        private bool HizmetExists(int id)
        {
            return _context.Hizmetler.Any(e => e.HizmetID == id);
        }
    }
}
