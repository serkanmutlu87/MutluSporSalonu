using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MutluSporSalonu.Models;

namespace MutluSporSalonu.Controllers
{
    public class HesapController : Controller
    {
        private readonly DBContext _veriTabani;

        public HesapController(DBContext veriTabani)
        {
            _veriTabani = veriTabani;
        }

        // --------------------------------------------------------------------
        // GİRİŞ
        // --------------------------------------------------------------------

        [HttpGet]
        [AllowAnonymous] // Giriş ekranına herkes erişebilsin
        public IActionResult Giris(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new Uye());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Giris(Uye model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // Login ekranında bu alanlar yok, o yüzden validasyondan çıkar
            ModelState.Remove(nameof(Uye.UyeAdSoyad));
            ModelState.Remove(nameof(Uye.UyeTelefon));
            ModelState.Remove(nameof(Uye.KayitTarihi));
            ModelState.Remove(nameof(Uye.Rol));

            if (!ModelState.IsValid)
                return View(model);

            var email = (model.UyeEposta ?? "").Trim();
            var sifre = model.UyeSifre ?? "";

            var uye = await _veriTabani.Uyeler.FirstOrDefaultAsync(u =>
                u.UyeEposta.Trim().ToLower() == email.ToLower() &&
                u.UyeSifre == sifre);

            if (uye == null)
            {
                ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
                return View(model);
            }

            var rolDb = (uye.Rol ?? "Uye").Trim();
            var rol = rolDb.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Uye";
            new Claim(ClaimTypes.Role, rol);

            var haklar = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, uye.UyeID.ToString()),
                new Claim(ClaimTypes.Name, uye.UyeAdSoyad),
                new Claim(ClaimTypes.Email, uye.UyeEposta),
                new Claim(ClaimTypes.Role, rol)
            };

            var kimlik = new ClaimsIdentity(
                haklar,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var kullanici = new ClaimsPrincipal(kimlik);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                kullanici);

            if (string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("AdminHome", "Home");

            return RedirectToAction("UyeHome", "Home"); ;
        }

        // --------------------------------------------------------------------
        // KAYIT
        // --------------------------------------------------------------------

        [HttpGet]
        [AllowAnonymous] // Kayıt ekranına da giriş yapmadan erişilebilsin
        public IActionResult Kayit()
        {
            return View(new Uye());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Kayit(Uye model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Bu e-posta ile daha önce kayıt yapılmış mı?
            bool epostaVarMi = await _veriTabani.Uyeler
                .AnyAsync(u => u.UyeEposta == model.UyeEposta);

            if (epostaVarMi)
            {
                ModelState.AddModelError("Eposta", "Bu e-posta ile kayıtlı bir kullanıcı zaten mevcut.");
                return View(model);
            }

            // Yeni üye nesnesi oluştur
            var yeniUye = new Uye
            {
                UyeAdSoyad = model.UyeAdSoyad,
                UyeEposta = model.UyeEposta,
                UyeSifre = model.UyeSifre,
                UyeTelefon = model.UyeTelefon,
                KayitTarihi = DateTime.Now,
                Rol = "Uye"
            };

            _veriTabani.Uyeler.Add(yeniUye);
            await _veriTabani.SaveChangesAsync();

            return RedirectToAction("Giris");
        }

        // --------------------------------------------------------------------
        // ÇIKIŞ
        // --------------------------------------------------------------------

        [HttpPost]
        [Authorize] // Çıkış işlemini sadece giriş yapmış kullanıcı çağırabilsin
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cikis()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Giris");
        }
    }
}
