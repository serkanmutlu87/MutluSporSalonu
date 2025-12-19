/**
* @file        HesapController.cs
* @description Mutlu Spor Salonu uygulamasında kullanıcı kimlik doğrulama işlemlerini yöneten MVC controller.
*              Cookie tabanlı authentication kullanır.
*
*              Sağlanan işlevler:
*              - Kullanıcı giriş (login) işlemlerini gerçekleştirir.
*              - Kullanıcı kayıt (register) işlemlerini gerçekleştirir.
*              - Rol bilgisine göre (Admin / Üye) yönlendirme yapar.
*              - CookieAuthentication ile oturum açma ve kapatma işlemlerini yönetir.
*              - Yetkisiz erişimler için Authorize / AllowAnonymous kurallarını uygular.
*
* @course      BSM 311 Web Programlama
* @assignment  Dönem Projesi – MutluSporSalonu
* @date        20.12.2025
* @author      D255012008 - Serkan Mutlu
*/

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
        // GİRİŞ (LOGIN)
        // --------------------------------------------------------------------

        [HttpGet]
        [AllowAnonymous] // Giriş ekranına yetkilendirme olmadan erişim sağlar
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

            // Login ekranında bulunmayan alanları model doğrulamasından çıkarır
            ModelState.Remove(nameof(Uye.UyeAdSoyad));
            ModelState.Remove(nameof(Uye.UyeTelefon));
            ModelState.Remove(nameof(Uye.KayitTarihi));
            ModelState.Remove(nameof(Uye.Rol));

            if (!ModelState.IsValid)
                return View(model);

            var email = (model.UyeEposta ?? "").Trim();
            var sifre = model.UyeSifre ?? "";

            // E-posta ve şifre bilgisine göre kullanıcıyı veritabanında arar
            var uye = await _veriTabani.Uyeler.FirstOrDefaultAsync(u =>
                u.UyeEposta.Trim().ToLower() == email.ToLower() &&
                u.UyeSifre == sifre);

            if (uye == null)
            {
                ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
                return View(model);
            }

            // Rol bilgisini normalize eder (Admin / Uye)
            var rolDb = (uye.Rol ?? "Uye").Trim();
            var rol = rolDb.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Uye";

            // Kullanıcıya ait claim listesini oluşturur
            var haklar = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, uye.UyeID.ToString()),
                new Claim(ClaimTypes.Name, uye.UyeAdSoyad),
                new Claim(ClaimTypes.Email, uye.UyeEposta),
                new Claim(ClaimTypes.Role, rol)
            };

            // Cookie tabanlı kimlik nesnesi oluşturur
            var kimlik = new ClaimsIdentity(
                haklar,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var kullanici = new ClaimsPrincipal(kimlik);

            // Kullanıcıyı sisteme giriş yapmış olarak işaretler
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                kullanici);

            // Rol bilgisine göre ana sayfa yönlendirmesi yapar
            if (string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("AdminHome", "Home");

            return RedirectToAction("UyeHome", "Home");
        }

        // --------------------------------------------------------------------
        // KAYIT (REGISTER)
        // --------------------------------------------------------------------

        [HttpGet]
        [AllowAnonymous] // Kayıt ekranına giriş yapmadan erişim sağlar
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

            // Aynı e-posta adresiyle kayıtlı kullanıcı olup olmadığını kontrol eder
            bool epostaVarMi = await _veriTabani.Uyeler
                .AnyAsync(u => u.UyeEposta == model.UyeEposta);

            if (epostaVarMi)
            {
                ModelState.AddModelError("Eposta", "Bu e-posta ile kayıtlı bir kullanıcı zaten mevcut.");
                return View(model);
            }

            // Yeni kullanıcı nesnesi oluşturur
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
        // ÇIKIŞ (LOGOUT)
        // --------------------------------------------------------------------

        [HttpPost]
        [Authorize] // Sadece giriş yapmış kullanıcıların çıkış yapmasını sağlar
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cikis()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Giris");
        }
    }
}
