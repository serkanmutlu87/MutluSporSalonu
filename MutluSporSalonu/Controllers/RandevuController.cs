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
            var dBContext = _context.Randevular.Include(r => r.Antrenor).Include(r => r.Hizmet).Include(r => r.Uye);
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
            ViewData["AntrenorID"] = new SelectList(_context.Antrenorler, "AntrenorID", "AntrenorAdSoyad");
            ViewData["HizmetID"] = new SelectList(_context.Hizmetler, "HizmetID", "HizmetAdi");
            ViewData["UyeID"] = new SelectList(_context.Uyeler, "UyeID", "Rol");
            return View();
        }

        // POST: Randevu/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RandevuID,UyeID,AntrenorID,HizmetID,SalonID,RandevuTarihi,RandevuBaslangicSaati,RandevuBitisSaati,RandevuUcret,RandevuOnaylandiMi,RandevuAciklama,RandevuOlusturulmaTarihi")] Randevu randevu)
        {
            if (ModelState.IsValid)
            {
                _context.Add(randevu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AntrenorID"] = new SelectList(_context.Antrenorler, "AntrenorID", "AntrenorAdSoyad", randevu.AntrenorID);
            ViewData["HizmetID"] = new SelectList(_context.Hizmetler, "HizmetID", "HizmetAdi", randevu.HizmetID);
            ViewData["UyeID"] = new SelectList(_context.Uyeler, "UyeID", "Rol", randevu.UyeID);
            return View(randevu);
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
            ViewData["AntrenorID"] = new SelectList(_context.Antrenorler, "AntrenorID", "AntrenorAdSoyad", randevu.AntrenorID);
            ViewData["HizmetID"] = new SelectList(_context.Hizmetler, "HizmetID", "HizmetAdi", randevu.HizmetID);
            ViewData["UyeID"] = new SelectList(_context.Uyeler, "UyeID", "Rol", randevu.UyeID);
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
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(randevu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RandevuExists(randevu.RandevuID))
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
            ViewData["AntrenorID"] = new SelectList(_context.Antrenorler, "AntrenorID", "AntrenorAdSoyad", randevu.AntrenorID);
            ViewData["HizmetID"] = new SelectList(_context.Hizmetler, "HizmetID", "HizmetAdi", randevu.HizmetID);
            ViewData["UyeID"] = new SelectList(_context.Uyeler, "UyeID", "Rol", randevu.UyeID);
            return View(randevu);
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
    }
}
