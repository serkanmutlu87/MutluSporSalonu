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
    public class AntrenorController : Controller
    {
        private readonly DBContext _context;

        public AntrenorController(DBContext context)
        {
            _context = context;
        }

        // GET: Antrenor
        public async Task<IActionResult> Index()
        {
            var antrenorler = await _context.Antrenorler
                .Include(a => a.SporSalonu)
                .ToListAsync();

            return View(antrenorler);
        }

        // GET: Antrenor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var antrenor = await _context.Antrenorler
                .Include(a => a.SporSalonu)
                .FirstOrDefaultAsync(m => m.AntrenorID == id);
            if (antrenor == null)
            {
                return NotFound();
            }

            return View(antrenor);
        }

        // GET: Antrenor/Create
        public IActionResult Create()
        {
            ViewData["SalonID"] = new SelectList(_context.Salonlar, "SalonID", "SalonAdi");
            ViewData["HizmetID"] = new SelectList(_context.Hizmetler, "HizmetID", "HizmetAdi");
            return View();
        }

        // POST: Antrenor/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // GET: Antrenor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor == null)
            {
                return NotFound();
            }
            ViewData["SalonID"] = new SelectList(_context.Salonlar, "SalonID", "SalonAdi", antrenor.SalonID);
            ViewData["HizmetID"] = new SelectList(_context.Hizmetler, "HizmetID", "HizmetAdi");
            return View(antrenor);
        }

        // POST: Antrenor/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AntrenorID,AntrenorAdSoyad,AntrenorUzmanlikAlanlari,AntrenorTelefon,AntrenorEposta,AntrenorMusaitlikBaslangic,AntrenorMusaitlikBitis,SalonID")] Antrenor antrenor)
        {
            if (id != antrenor.AntrenorID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(antrenor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AntrenorExists(antrenor.AntrenorID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["SalonID"] = new SelectList(_context.Salonlar, "SalonID", "SalonAdi", antrenor.SalonID);
            ViewData["HizmetID"] = new SelectList(_context.Hizmetler, "HizmetID", "HizmetAdi");
            return View(antrenor);
        }

        // GET: Antrenor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var antrenor = await _context.Antrenorler
                .Include(a => a.SporSalonu)
                .Include(a => a.Hizmetler)
                .FirstOrDefaultAsync(m => m.AntrenorID == id);
            if (antrenor == null)
            {
                return NotFound();
            }

            return View(antrenor);
        }

        // POST: Antrenor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor != null)
            {
                _context.Antrenorler.Remove(antrenor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AntrenorExists(int id)
        {
            return _context.Antrenorler.Any(e => e.AntrenorID == id);
        }
    }
}
