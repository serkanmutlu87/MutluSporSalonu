/**
* @file        AntrenorApiController.cs
* @description Mutlu Spor Salonu uygulamasında antrenör verilerini JSON olarak döndüren REST API.
*              Sağladıkları:
*              - Tüm antrenörleri (salon bilgisiyle birlikte) liste olarak döndürür.
*              - Belirli bir tarih ve saat aralığında uygun (müsait) antrenörleri döndürür.
*              - LINQ sorguları üzerinden filtreleme yapar, API ihtiyacını karşılar.
*
* @course      BSM 311 Web Programlama (ASP.NET Core MVC) – 2025-2026 Güz
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

namespace MutluSporSalonu.Controllers.Api
{
    [ApiController]
    [Route("api/antrenor")]
    public class AntrenorApiController : ControllerBase
    {
        private readonly DBContext _context;

        public AntrenorApiController(DBContext context)
        {
            _context = context;
        }

        // ==========================================================
        // GET: /api/antrenor
        // Tüm antrenörleri salon bilgisi ile birlikte listeler.
        // JSON döndürür, sayfa üretmez.
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> TumAntrenorleriGetir()
        {
            var liste = await _context.Antrenorler
                .Include(a => a.SporSalonu)
                .Select(a => new
                {
                    a.AntrenorID,
                    a.AntrenorAdSoyad,
                    a.AntrenorUzmanlikAlanlari,
                    a.AntrenorTelefon,
                    a.AntrenorEposta,
                    salonID = a.SalonID,
                    salonAdi = a.SporSalonu != null ? a.SporSalonu.SalonAdi : null,
                    musaitlikBaslangic = a.AntrenorMusaitlikBaslangic.ToString(@"hh\:mm"),
                    musaitlikBitis = a.AntrenorMusaitlikBitis.ToString(@"hh\:mm")
                })
                .ToListAsync();

            return Ok(liste);
        }

        // ==========================================================
        // GET: /api/antrenor/uygun?tarih=2025-12-01&baslangic=10:00&bitis=11:00
        // Belirli tarih ve saat aralığında uygun olan antrenörleri döndürür.
        //
        // Çakışma kontrolü şu mantıkla yapılır:
        // baslangic < mevcutBitis  &&  bitis > mevcutBaslangic  => çakışma var demektir
        //
        // Bu endpoint iki kontrol uygular:
        // 1) Antrenörün kendi müsaitlik aralığı bu saat aralığını kapsar mı?
        // 2) Aynı gün ve aynı saat aralığında çakışan randevusu var mı?
        // ==========================================================
        [HttpGet("uygun")]
        public async Task<IActionResult> UygunAntrenorleriGetir(
            [FromQuery] string tarih,
            [FromQuery] string baslangic,
            [FromQuery] string bitis)
        {
            // Tarih formatı kontrol edilir. Hatalıysa istek reddedilir.
            if (!DateTime.TryParse(tarih, out var randevuTarihi))
                return BadRequest("Geçerli bir tarih giriniz. Örn: 2025-12-01");

            // Başlangıç saati kontrol edilir. Hatalıysa istek reddedilir.
            if (!TimeSpan.TryParse(baslangic, out var baslangicSaat))
                return BadRequest("Geçerli bir başlangıç saati giriniz. Örn: 10:00");

            // Bitiş saati kontrol edilir. Hatalıysa istek reddedilir.
            if (!TimeSpan.TryParse(bitis, out var bitisSaat))
                return BadRequest("Geçerli bir bitiş saati giriniz. Örn: 11:00");

            // Mantık kontrolü yapılır: başlangıç bitişten sonra olamaz.
            if (baslangicSaat >= bitisSaat)
                return BadRequest("Başlangıç saati, bitiş saatinden önce olmalıdır.");

            // 1) Müsaitlik aralığı uygun olanlar seçilir
            // 2) Aynı gün aynı saat aralığında çakışan randevusu olanlar elenir
            var uygunlar = await _context.Antrenorler
                .Where(a =>
                    a.AntrenorMusaitlikBaslangic <= baslangicSaat &&
                    a.AntrenorMusaitlikBitis >= bitisSaat)
                .Where(a =>
                    !_context.Randevular.Any(r =>
                        r.AntrenorID == a.AntrenorID &&
                        r.RandevuTarihi.Date == randevuTarihi.Date &&
                        baslangicSaat < r.RandevuBitisSaati &&
                        bitisSaat > r.RandevuBaslangicSaati
                    )
                )
                .Include(a => a.SporSalonu)
                .Select(a => new
                {
                    a.AntrenorID,
                    a.AntrenorAdSoyad,
                    a.AntrenorUzmanlikAlanlari,
                    salonID = a.SalonID,
                    salonAdi = a.SporSalonu != null ? a.SporSalonu.SalonAdi : null,
                    musaitlikBaslangic = a.AntrenorMusaitlikBaslangic.ToString(@"hh\:mm"),
                    musaitlikBitis = a.AntrenorMusaitlikBitis.ToString(@"hh\:mm")
                })
                .ToListAsync();

            return Ok(uygunlar);
        }
    }
}
