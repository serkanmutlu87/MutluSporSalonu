/**
* @file        HomeController.cs
* @description Mutlu Spor Salonu uygulamasýnda ana sayfa ve rol bazlý yönlendirmeleri yöneten MVC controller.
*              Kullanýcýnýn giriþ ve rol durumuna göre doðru ana sayfaya yönlendirme saðlar.
*
*              Saðlanan iþlevler:
*              - Giriþ yapýlmamýþ kullanýcýlar için tanýtým (landing) sayfasýný gösterir.
*              - Giriþ yapýlmýþ kullanýcýlarý rollerine göre ayýrýr.
*              - Admin rolü için yönetim ana sayfasýný açar.
*              - Üye rolü için üye ana sayfasýný açar.
*              - Yetkilendirme kurallarý ile eriþim kontrolü uygular.
*
* @course      BSM 311 Web Programlama
* @assignment  Dönem Projesi – MutluSporSalonu
* @date        20.12.2025
* @author      D255012008 - Serkan Mutlu
*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MutluSporSalonu.Models;
using System.Diagnostics;

namespace MutluSporSalonu.Controllers
{
    public class HomeController : Controller
    {
        // ------------------------------------------------------------
        // GET: /
        // Giriþ durumuna göre kullanýcýyý uygun ana sayfaya yönlendirir
        // Giriþ yapýlmamýþsa tanýtým sayfasýný gösterir
        // ------------------------------------------------------------
        [AllowAnonymous]
        public IActionResult Index()
        {
            // Kullanýcý giriþ yapmýþsa rol bilgisine göre yönlendirme yapýlýr
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("AdminHome", "Home");
                }

                // Admin olmayan kullanýcýlar üye ana sayfasýna yönlendirilir
                return RedirectToAction("UyeHome", "Home");
            }

            // Giriþ yapýlmamýþ kullanýcýlar için landing sayfasý gösterilir
            // Bu sayfa Layout = null olarak çalýþýr
            return View();
        }

        // ------------------------------------------------------------
        // GET: /Home/AdminHome
        // Yönetici ana sayfasýný görüntüler
        // Sadece Admin rolüne izin verir
        // ------------------------------------------------------------
        [Authorize(Roles = "Admin")]
        public IActionResult AdminHome()
        {
            return View();
        }

        // ------------------------------------------------------------
        // GET: /Home/UyeHome
        // Üye ana sayfasýný görüntüler
        // Sadece Uye rolüne izin verir
        // ------------------------------------------------------------
        [Authorize(Roles = "Uye")]
        public IActionResult UyeHome()
        {
            return View();
        }
    }
}
