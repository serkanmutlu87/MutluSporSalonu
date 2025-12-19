using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MutluSporSalonu.Models;

namespace MutluSporSalonu.Controllers
{
    [Authorize] // giriş zorunlu olsun (istersen kaldır)
    public class AntrenorController : Controller
    {
        private readonly DBContext _context;

        public AntrenorController(DBContext context)
        {
            _context = context;
        }

        // GET: Antrenor  (Admin + Üye görebilir)
        public async Task<IActionResult> Index()
        {
            var antrenorler = await _context.Antrenorler
                .Include(a => a.SporSalonu)
                .ToListAsync();

            return View(antrenorler);
        }

        // GET: Antrenor/Details/5  (Admin + Üye görebilir)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler
                .Include(a => a.SporSalonu)
                .Include(a => a.Hizmetler)
                .FirstOrDefaultAsync(m => m.AntrenorID == id);

            if (antrenor == null) return NotFound();

            return View(antrenor);
        }

        // GET: Antrenor/Create  (SADECE ADMIN)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["SalonID"] = new SelectList(_context.Salonlar, "SalonID", "SalonAdi");
            ViewData["HizmetID"] = new SelectList(_context.Hizmetler, "HizmetID", "HizmetAdi");
            return View();
        }

        // POST: Antrenor/Create  (SADECE ADMIN)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("AntrenorID,AntrenorAdSoyad,AntrenorUzmanlikAlanlari,AntrenorTelefon,AntrenorEposta,AntrenorMusaitlikBaslangic,AntrenorMusaitlikBitis,SalonID")] Antrenor antrenor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(antrenor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["SalonID"] = new SelectList(_context.Salonlar, "SalonID", "SalonAdi", antrenor.SalonID);
            ViewData["HizmetID"] = new SelectList(_context.Hizmetler, "HizmetID", "HizmetAdi");
            return View(antrenor);
        }

        // GET: Antrenor/Edit/5  (SADECE ADMIN)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor == null) return NotFound();

            ViewData["SalonID"] = new SelectList(_context.Salonlar, "SalonID", "SalonAdi", antrenor.SalonID);
            ViewData["HizmetID"] = new SelectList(_context.Hizmetler, "HizmetID", "HizmetAdi");
            return View(antrenor);
        }

        // POST: Antrenor/Edit/5  (SADECE ADMIN)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("AntrenorID,AntrenorAdSoyad,AntrenorUzmanlikAlanlari,AntrenorTelefon,AntrenorEposta,AntrenorMusaitlikBaslangic,AntrenorMusaitlikBitis,SalonID")] Antrenor antrenor)
        {
            if (id != antrenor.AntrenorID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(antrenor);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AntrenorExists(antrenor.AntrenorID)) return NotFound();
                    throw;
                }
            }

            ViewData["SalonID"] = new SelectList(_context.Salonlar, "SalonID", "SalonAdi", antrenor.SalonID);
            ViewData["HizmetID"] = new SelectList(_context.Hizmetler, "HizmetID", "HizmetAdi");
            return View(antrenor);
        }

        // GET: Antrenor/Delete/5  (SADECE ADMIN)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler
                .Include(a => a.SporSalonu)
                .Include(a => a.Hizmetler)
                .FirstOrDefaultAsync(m => m.AntrenorID == id);

            if (antrenor == null) return NotFound();

            return View(antrenor);
        }

        // POST: Antrenor/Delete/5  (SADECE ADMIN)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor != null)
            {
                _context.Antrenorler.Remove(antrenor);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AntrenorExists(int id)
        {
            return _context.Antrenorler.Any(e => e.AntrenorID == id);
        }
    }
}
