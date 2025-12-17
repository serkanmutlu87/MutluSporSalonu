using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MutluSporSalonu.Models
{
    public class Randevu
    {
        [Key]
        public int RandevuID { get; set; }

        // ===========================
        //  İLİŞKİLER
        // ===========================

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
        public SporSalonu? SporSalonu { get; set; }

        // ===========================
        //  RANDEVU DETAYLARI
        // ===========================

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

        // ===========================
        //  ONAY & DURUM
        // ===========================

        [Display(Name = "Onay Durumu")]
        public bool RandevuOnaylandiMi { get; set; } = false; // Admin onay mekanizması

        [StringLength(500)]
        [Display(Name = "Not / Açıklama")]
        public string? RandevuAciklama { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime RandevuOlusturulmaTarihi { get; set; } = DateTime.Now;
    }
}