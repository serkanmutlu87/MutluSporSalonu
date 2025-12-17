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
    public class UyeController : Controller
    {
        private readonly DBContext _context;

        public UyeController(DBContext context)
        {
            _context = context;
        }

        // GET: Uye
        public async Task<IActionResult> Index()
        {
            return View(await _context.Uyeler.ToListAsync());
        }

        // GET: Uye/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uye = await _context.Uyeler
                .Include(m => m.Randevular)
                .FirstOrDefaultAsync(m => m.UyeID == id);
            if (uye == null)
            {
                return NotFound();
            }

            return View(uye);
        }

        // GET: Uye/Create
        public IActionResult Create()
        {
            var model = new Uye
            { 
                KayitTarihi = DateTime.Now,
                Rol = "Uye"
            };
            return View(model);
        }

        // POST: Uye/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UyeID,UyeAdSoyad,UyeEposta,UyeSifre,UyeTelefon,KayitTarihi,Rol")] Uye uye)
        {
            if (await _context.Uyeler.AnyAsync(u => u.UyeEposta == uye.UyeEposta))
            {
                ModelState.AddModelError("Eposta", "Bu e-posta adresi ile zaten bir hesap bulunmaktadır.");
            }

            if (!ModelState.IsValid)
            {
                return View(uye);
            }

            // Varsayılan rol atanmadıysa Uye yap
            if (string.IsNullOrWhiteSpace(uye.Rol))
                uye.Rol = "Uye";

            _context.Uyeler.Add(uye);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(KayitBasarili));
        }
        public IActionResult KayitBasarili()
        {
            return View();
        }

        // GET: Uye/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uye = await _context.Uyeler.FindAsync(id);
            if (uye == null)
            {
                return NotFound();
            }
            return View(uye);
        }

        // POST: Uye/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UyeID,UyeAdSoyad,UyeEposta,UyeSifre,UyeTelefon,KayitTarihi,Rol")] Uye uye)
        {
            if (id != uye.UyeID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(uye);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UyeExists(uye.UyeID))
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
            return View(uye);
        }

        // GET: Uye/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uye = await _context.Uyeler
                .FirstOrDefaultAsync(m => m.UyeID == id);
            if (uye == null)
            {
                return NotFound();
            }

            return View(uye);
        }

        // POST: Uye/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var uye = await _context.Uyeler.FindAsync(id);
            if (uye != null)
            {
                _context.Uyeler.Remove(uye);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UyeExists(int id)
        {
            return _context.Uyeler.Any(e => e.UyeID == id);
        }
    }
}
