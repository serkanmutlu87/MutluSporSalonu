/**
* @file        SporSalonuController.cs
* @description Mutlu Spor Salonu uygulamasında spor salonu (Salonlar) yönetimini yapan MVC controller.
*              View döndürür.
*
*              Sağlanan işlevler:
*              - Salon listesini görüntüler.
*              - Salon detayında salonun hizmet ve antrenör ilişkilerini birlikte gösterir.
*              - Admin rolü için salon ekleme, güncelleme ve silme işlemlerini sağlar.
*              - Üye rolü için salon listeleme ve detay görüntülemeye izin verir.
*              - [Authorize] ve rol bazlı yetkilendirme ile erişim kontrolü uygular.
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
using System.Linq;
using System.Threading.Tasks;

namespace MutluSporSalonu.Controllers
{
    [Authorize] // Kimlik doğrulaması olmayan erişimleri engeller
    public class SporSalonuController : Controller
    {
        private readonly DBContext _context;

        public SporSalonuController(DBContext context)
        {
            _context = context;
        }

        // ------------------------------------------------------------
        // GET: SporSalonu
        // Salon listesini görüntüler
        // Admin ve Uye rollerine açıktır
        // ------------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            return View(await _context.Salonlar.ToListAsync());
        }

        // ------------------------------------------------------------
        // GET: SporSalonu/Details/5
        // Seçilen salonun detay bilgilerini görüntüler
        // Hizmetler ve antrenörler ilişkileriyle birlikte getirilir
        // Admin ve Uye rollerine açıktır
        // ------------------------------------------------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sporSalonu = await _context.Salonlar
                .Include(s => s.Hizmetler)
                .Include(s => s.Antrenorler)
                .FirstOrDefaultAsync(s => s.SalonID == id);

            if (sporSalonu == null) return NotFound();

            return View(sporSalonu);
        }

        // ------------------------------------------------------------
        // GET: SporSalonu/Create
        // Yeni salon ekleme ekranını açar
        // Sadece Admin rolüne izin verir
        // ------------------------------------------------------------
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // ------------------------------------------------------------
        // POST: SporSalonu/Create
        // Yeni salon kaydını veritabanına ekler
        // Model doğrulaması başarılıysa kayıt işlemini tamamlar
        // Sadece Admin rolüne izin verir
        // ------------------------------------------------------------
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SaloNID,SalonAdi,SalonAdres,SalonAcilisSaati,SalonKapanisSaati,SalonAciklama")] SporSalonu sporSalonu)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sporSalonu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sporSalonu);
        }

        // ------------------------------------------------------------
        // GET: SporSalonu/Edit/5
        // Seçilen salonun düzenleme ekranını açar
        // Sadece Admin rolüne izin verir
        // ------------------------------------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var sporSalonu = await _context.Salonlar.FindAsync(id);
            if (sporSalonu == null) return NotFound();

            return View(sporSalonu);
        }

        // ------------------------------------------------------------
        // POST: SporSalonu/Edit/5
        // Salon bilgilerini günceller
        // Eş zamanlı güncelleme hatalarını kontrol eder
        // Sadece Admin rolüne izin verir
        // ------------------------------------------------------------
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SaloNID,SalonAdi,SalonAdres,SalonAcilisSaati,SalonKapanisSaati,SalonAciklama")] SporSalonu sporSalonu)
        {
            if (id != sporSalonu.SalonID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sporSalonu);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SporSalonuExists(sporSalonu.SalonID))
                        return NotFound();
                    throw;
                }
            }

            return View(sporSalonu);
        }

        // ------------------------------------------------------------
        // GET: SporSalonu/Delete/5
        // Salon silme onay ekranını açar
        // Sadece Admin rolüne izin verir
        // ------------------------------------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var sporSalonu = await _context.Salonlar
                .FirstOrDefaultAsync(m => m.SalonID == id);

            if (sporSalonu == null) return NotFound();

            return View(sporSalonu);
        }

        // ------------------------------------------------------------
        // POST: SporSalonu/Delete/5
        // Seçilen salon kaydını veritabanından siler
        // Sadece Admin rolüne izin verir
        // ------------------------------------------------------------
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sporSalonu = await _context.Salonlar.FindAsync(id);
            if (sporSalonu != null)
                _context.Salonlar.Remove(sporSalonu);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Salon kaydının veritabanında var olup olmadığını kontrol eder
        private bool SporSalonuExists(int id)
        {
            return _context.Salonlar.Any(e => e.SalonID == id);
        }
    }
}
