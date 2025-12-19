using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MutluSporSalonu.Models;
using System.Diagnostics;

namespace MutluSporSalonu.Controllers
{
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public IActionResult Index()
        {
            // Giriþ yapmýþsa, role göre yönlendir
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("AdminHome", "Home");
                }

                // Normal üye
                return RedirectToAction("UyeHome", "Home");
            }

            // Giriþ yapmamýþsa: Landing (Layout = null olan sayfa)
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminHome()
        {
            return View();
        }

        [Authorize(Roles = "Uye")]
        public IActionResult UyeHome()
        {
            return View();
        }
    }
}
