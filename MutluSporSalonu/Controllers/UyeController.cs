using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MutluSporSalonu.Models;

// ✅ eklediklerimiz
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace MutluSporSalonu.Controllers
{
    public class UyeController : Controller
    {
        private readonly DBContext _context;

        public UyeController(DBContext context)
        {
            _context = context;
        }

        // ✅ Üye listesini normal üye görmesin (SADECE ADMIN)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Uyeler.ToListAsync());
        }

        // ✅ Detay: Admin her üyeyi görür, Üye sadece kendini görür
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                int currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (id.Value != currentUserId) return Forbid();
            }

            var uye = await _context.Uyeler
                .Include(u => u.Randevular)
                    .ThenInclude(r => r.Hizmet)
                .Include(u => u.Randevular)
                    .ThenInclude(r => r.Antrenor)
                .FirstOrDefaultAsync(u => u.UyeID == id);

            if (uye == null) return NotFound();

            return View(uye);
        }


        // ✅ Kayıt: herkes sisteme kayıt olabilir
        [AllowAnonymous]
        public IActionResult Create()
        {
            var model = new Uye
            {
                KayitTarihi = DateTime.Now,
                Rol = "Uye"
            };
            return View(model);
        }

        // ✅ Kayıt: herkes sisteme kayıt olabilir
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UyeID,UyeAdSoyad,UyeEposta,UyeSifre,UyeTelefon")] Uye uye)
        {
            // eposta kontrol
            if (await _context.Uyeler.AnyAsync(u => u.UyeEposta == uye.UyeEposta))
            {
                ModelState.AddModelError("UyeEposta", "Bu e-posta adresi ile zaten bir hesap bulunmaktadır.");
            }

            if (!ModelState.IsValid)
            {
                return View(uye);
            }

            // ✅ güvenlik: rol ve kayıt tarihi server tarafında setlensin
            uye.Rol = "Uye";
            uye.KayitTarihi = DateTime.Now;

            _context.Uyeler.Add(uye);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(KayitBasarili));
        }

        [AllowAnonymous]
        public IActionResult KayitBasarili()
        {
            return View();
        }

        // ✅ Edit: Admin her üyeyi, Üye sadece kendini
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                int currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (id.Value != currentUserId) return Forbid();
            }

            var uye = await _context.Uyeler.FindAsync(id);
            if (uye == null) return NotFound();

            return View(uye);
        }

        // ✅ Edit POST: Admin her üyeyi, Üye sadece kendini
        // ✅ ayrıca Rol/KayitTarihi güvenli şekilde korunuyor
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UyeID,UyeAdSoyad,UyeEposta,UyeSifre,UyeTelefon")] Uye formUye)
        {
            if (id != formUye.UyeID) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                int currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (id != currentUserId) return Forbid();
            }

            if (!ModelState.IsValid)
                return View(formUye);

            // ✅ DB’den çek, sadece izinli alanları güncelle (rolü asla formdan alma)
            var uyeDb = await _context.Uyeler.FindAsync(id);
            if (uyeDb == null) return NotFound();

            uyeDb.UyeAdSoyad = formUye.UyeAdSoyad;
            uyeDb.UyeEposta = formUye.UyeEposta;
            uyeDb.UyeSifre = formUye.UyeSifre;
            uyeDb.UyeTelefon = formUye.UyeTelefon;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UyeExists(id)) return NotFound();
                throw;
            }

            // Admin değilse kendi profil ekranına, admin ise listeye
            if (!User.IsInRole("Admin"))
                return RedirectToAction(nameof(Details), new { id });

            return RedirectToAction(nameof(Index));
        }

        // ✅ Delete: Admin her üyeyi, Üye sadece kendini
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                int currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (id.Value != currentUserId) return Forbid();
            }

            var uye = await _context.Uyeler.FirstOrDefaultAsync(m => m.UyeID == id);
            if (uye == null) return NotFound();

            return View(uye);
        }

        // ✅ Delete POST: Admin her üyeyi, Üye sadece kendini
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!User.IsInRole("Admin"))
            {
                int currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (id != currentUserId) return Forbid();
            }

            var uye = await _context.Uyeler.FindAsync(id);
            if (uye != null)
            {
                _context.Uyeler.Remove(uye);
                await _context.SaveChangesAsync();
            }

            // ✅ üye kendi hesabını sildiyse çıkış yaptır
            if (!User.IsInRole("Admin"))
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UyeExists(int id)
        {
            return _context.Uyeler.Any(e => e.UyeID == id);
        }
    }
}
