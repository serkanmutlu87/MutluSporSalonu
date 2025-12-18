using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MutluSporSalonu.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MutluSporSalonu.Controllers
{
    public class HizmetController : Controller
    {
        private readonly DBContext _context;

        public HizmetController(DBContext context)
        {
            _context = context;
        }

        // GET: Hizmet
        public async Task<IActionResult> Index()
        {
            var dBContext = _context.Hizmetler.Include(h => h.SporSalonu);
            return View(await dBContext.ToListAsync());
        }

        // GET: Hizmet/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hizmet = await _context.Hizmetler
                .Include(h => h.SporSalonu)
                .FirstOrDefaultAsync(m => m.HizmetID == id);
            if (hizmet == null)
            {
                return NotFound();
            }

            return View(hizmet);
        }

        // GET: Hizmet/Create
        public IActionResult Create()
        {
            ViewBag.SalonID = new SelectList(_context.Salonlar.ToList(), "SalonID", "SalonAdi");
            return View();
        }

        // POST: Hizmet/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
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

        // GET: Hizmet/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet == null)
            {
                return NotFound();
            }
            ViewBag.SalonID = new SelectList(_context.Salonlar, "SalonID", "SalonAdi", hizmet.SalonID);
            return View(hizmet);
        }

        // POST: Hizmet/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HizmetID,HizmetAdi,HizmetSureDakika,HizmetUcret,HizmetAciklama,SalonID")] Hizmet hizmet)
        {
            if (id != hizmet.HizmetID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hizmet);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HizmetExists(hizmet.HizmetID))
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
            ViewBag.SalonID = new SelectList(_context.Salonlar, "SalonID", "SalonAdi", hizmet.SalonID);
            return View(hizmet);
        }

        // GET: Hizmet/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hizmet = await _context.Hizmetler
                .Include(h => h.SporSalonu)
                .FirstOrDefaultAsync(m => m.HizmetID == id);
            if (hizmet == null)
            {
                return NotFound();
            }

            return View(hizmet);
        }

        // POST: Hizmet/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet != null)
            {
                _context.Hizmetler.Remove(hizmet);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HizmetExists(int id)
        {
            return _context.Hizmetler.Any(e => e.HizmetID == id);
        }
    }
}
