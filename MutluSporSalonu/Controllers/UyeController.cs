/**
* @file        UyeController.cs
* @description Mutlu Spor Salonu uygulamasında üye yönetimini yapan MVC controller.
*              View döndürür.
*
*              Sağlanan işlevler:
*              - Üye listesini sadece Admin rolüne açar.
*              - Üye detay ekranında erişim kontrolü uygular:
*                   Admin tüm üyeleri görüntüler, Üye sadece kendi kaydını görüntüler.
*              - Üyelik kayıt (Create) işlemini giriş gerektirmeden çalıştırır.
*              - Rol ve kayıt tarihi gibi kritik alanların istemci tarafından değiştirilmesini engeller,
*                bu alanları sunucu tarafında belirler.
*              - Üye düzenleme ve silme işlemlerinde rol bazlı kısıt uygular:
*                   Admin tüm üyelerde işlem yapar, Üye sadece kendi kaydında işlem yapar.
*              - Üye kendi hesabını sildiğinde oturumu sonlandırır (SignOut) ve ana sayfaya yönlendirir.
*
* @course      BSM 311 Web Programlama
* @assignment  Dönem Projesi – MutluSporSalonu
* @date        20.12.2025
* @author      D255012008 - Serkan Mutlu
*/

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MutluSporSalonu.Models;

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

        // ------------------------------------------------------------
        // GET: Uye
        // Üye listesini görüntüler
        // Sadece Admin rolüne izin verir
        // ------------------------------------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Uyeler.ToListAsync());
        }

        // ------------------------------------------------------------
        // GET: Uye/Details/5
        // Admin tüm üyeleri görüntüler
        // Üye sadece kendi kaydını görüntüler, aksi durumda erişim engellenir
        // ------------------------------------------------------------
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

        // ------------------------------------------------------------
        // GET: Uye/Create
        // Üyelik kayıt ekranını açar
        // Giriş gerektirmez
        // Varsayılan rol ve kayıt tarihi değerini ekrana taşır
        // ------------------------------------------------------------
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

        // ------------------------------------------------------------
        // POST: Uye/Create
        // Yeni kullanıcı kaydını veritabanına ekler
        // Aynı e-posta ile kayıt olup olmadığını kontrol eder
        // Rol ve kayıt tarihi alanlarını sunucu tarafında set eder
        // ------------------------------------------------------------
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UyeID,UyeAdSoyad,UyeEposta,UyeSifre,UyeTelefon")] Uye uye)
        {
            if (await _context.Uyeler.AnyAsync(u => u.UyeEposta == uye.UyeEposta))
            {
                ModelState.AddModelError("UyeEposta", "Bu e-posta adresi ile zaten bir hesap bulunmaktadır.");
            }

            if (!ModelState.IsValid)
            {
                return View(uye);
            }

            // Kritik alanlar istemciden alınmaz, sunucuda belirlenir
            uye.Rol = "Uye";
            uye.KayitTarihi = DateTime.Now;

            _context.Uyeler.Add(uye);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(KayitBasarili));
        }

        // ------------------------------------------------------------
        // GET: Uye/KayitBasarili
        // Kayıt tamamlandığında bilgilendirme ekranı gösterir
        // ------------------------------------------------------------
        [AllowAnonymous]
        public IActionResult KayitBasarili()
        {
            return View();
        }

        // ------------------------------------------------------------
        // GET: Uye/Edit/5
        // Admin tüm üyelerde düzenleme yapabilir
        // Üye sadece kendi kaydını düzenleyebilir
        // ------------------------------------------------------------
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

        // ------------------------------------------------------------
        // POST: Uye/Edit/5
        // Admin tüm üyelerde güncelleme yapabilir
        // Üye sadece kendi kaydını güncelleyebilir
        // Rol/KayitTarihi gibi alanların istemciden değiştirilmesini engeller
        // ------------------------------------------------------------
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

            // Güncelleme sadece izinli alanlarda yapılır (rol vb. alanlar korunur)
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

            // Üye rolünde işlem sonrası profil detayına döndürür, Admin rolünde listeye döndürür
            if (!User.IsInRole("Admin"))
                return RedirectToAction(nameof(Details), new { id });

            return RedirectToAction(nameof(Index));
        }

        // ------------------------------------------------------------
        // GET: Uye/Delete/5
        // Admin tüm üyeleri silebilir
        // Üye sadece kendi kaydını silebilir
        // ------------------------------------------------------------
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

        // ------------------------------------------------------------
        // POST: Uye/Delete/5
        // Admin tüm üyeleri silebilir
        // Üye sadece kendi kaydını silebilir
        // Üye kendi hesabını sildiğinde SignOut ile oturum kapatılır
        // ------------------------------------------------------------
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

            if (!User.IsInRole("Admin"))
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction(nameof(Index));
        }

        // Üye kaydının veritabanında var olup olmadığını kontrol eder
        private bool UyeExists(int id)
        {
            return _context.Uyeler.Any(e => e.UyeID == id);
        }
    }
}
