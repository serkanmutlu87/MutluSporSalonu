using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MutluSporSalonu.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MutluSporSalonu.Controllers
{
    [Authorize] // giriş yapmadan SporSalonu sayfalarına girilmesin
    public class SporSalonuController : Controller
    {
        private readonly DBContext _context;

        public SporSalonuController(DBContext context)
        {
            _context = context;
        }

        // GET: SporSalonu (Admin + Uye görebilir)
        public async Task<IActionResult> Index()
        {
            return View(await _context.Salonlar.ToListAsync());
        }

        // GET: SporSalonu/Details/5 (Admin + Uye görebilir)
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

        // GET: SporSalonu/Create (sadece Admin)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: SporSalonu/Create (sadece Admin)
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

        // GET: SporSalonu/Edit/5 (sadece Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var sporSalonu = await _context.Salonlar.FindAsync(id);
            if (sporSalonu == null) return NotFound();

            return View(sporSalonu);
        }

        // POST: SporSalonu/Edit/5 (sadece Admin)
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

        // GET: SporSalonu/Delete/5 (sadece Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var sporSalonu = await _context.Salonlar
                .FirstOrDefaultAsync(m => m.SalonID == id);

            if (sporSalonu == null) return NotFound();

            return View(sporSalonu);
        }

        // POST: SporSalonu/Delete/5 (sadece Admin)
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

        private bool SporSalonuExists(int id)
        {
            return _context.Salonlar.Any(e => e.SalonID == id);
        }
    }
}
