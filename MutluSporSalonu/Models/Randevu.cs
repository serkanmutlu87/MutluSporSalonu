/**
* @file        Randevu.cs
* @description Mutlu Spor Salonu uygulamasında randevu kayıtlarını temsil eden model sınıfı.
*              Entity Framework Core ile veritabanı tablolaşmasını sağlar.
*
*              Sağlanan özellikler:
*              - Üye, antrenör, hizmet ve spor salonu ilişkilerini tanımlar.
*              - Randevu tarih ve saat aralığını (başlangıç / bitiş) tutar.
*              - Randevuya ait ücret bilgisini ve açıklamayı saklar.
*              - Admin onay mekanizması için durum alanı sağlar.
*              - Randevunun oluşturulma zamanını otomatik olarak kaydeder.
*              - DataAnnotations ile doğrulama ve ekran etiketleri belirler.
*
* @course      BSM 311 Web Programlama
* @assignment  Dönem Projesi – MutluSporSalonu
* @date        20.12.2025
* @author      D255012008 - Serkan Mutlu
*/

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MutluSporSalonu.Models
{
    public class Randevu
    {
        // ------------------------------------------------------------
        // Birincil anahtar
        // ------------------------------------------------------------
        [Key]
        public int RandevuID { get; set; }

        // ============================================================
        // İLİŞKİLER
        // ============================================================

        [Display(Name = "Üye")]
        [Required]
        public int UyeID { get; set; }
        public Uye? Uye { get; set; }

        [Display(Name = "Antrenör")]
        [Required]
        public int AntrenorID { get; set; }
        public Antrenor? Antrenor { get; set; }

        [Display(Name = "Hizmet Türü")]
        [Required]
        public int HizmetID { get; set; }
        public Hizmet? Hizmet { get; set; }

        [Display(Name = "Spor Salonu")]
        [Required]
        public int SalonID { get; set; }

        [ForeignKey(nameof(SalonID))]
        public SporSalonu? SporSalonu { get; set; }

        // ============================================================
        // RANDEVU DETAYLARI
        // ============================================================

        [Required(ErrorMessage = "Randevu tarihi zorunludur.")]
        [Display(Name = "Randevu Tarihi")]
        public DateTime RandevuTarihi { get; set; }

        [Required]
        [Display(Name = "Başlangıç Saati")]
        public TimeSpan RandevuBaslangicSaati { get; set; }

        [Required]
        [Display(Name = "Bitiş Saati")]
        public TimeSpan RandevuBitisSaati { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Toplam Ücret (₺)")]
        public decimal RandevuUcret { get; set; }

        // ============================================================
        // ONAY & DURUM
        // ============================================================

        [Display(Name = "Onay Durumu")]
        public bool RandevuOnaylandiMi { get; set; } = false; // Admin onay mekanizması

        [StringLength(500)]
        [Display(Name = "Not / Açıklama")]
        public string? RandevuAciklama { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime RandevuOlusturulmaTarihi { get; set; } = DateTime.Now;
    }
}
