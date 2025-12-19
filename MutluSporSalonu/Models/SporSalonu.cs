/**
* @file        SporSalonu.cs
* @description Mutlu Spor Salonu uygulamasında spor salonu varlığını temsil eden model sınıfı.
*              Entity Framework Core ile veritabanı tablolaşmasını sağlar.
*
*              Sağlanan özellikler:
*              - Spor salonuna ait temel tanımlayıcı bilgileri tutar.
*              - Salonun adres ve çalışma saatlerini (açılış / kapanış) tanımlar.
*              - Salon açıklaması için isteğe bağlı alan sunar.
*              - Hizmetler ve antrenörler ile olan ilişkileri navigation property üzerinden kurar.
*              - DataAnnotations kullanılarak doğrulama kuralları ve ekran etiketleri belirlenir.
*
* @course      BSM 311 Web Programlama
* @assignment  Dönem Projesi – MutluSporSalonu
* @date        20.12.2025
* @author      D255012008 - Serkan Mutlu
*/

using System.ComponentModel.DataAnnotations;

namespace MutluSporSalonu.Models
{
    public class SporSalonu
    {
        // ------------------------------------------------------------
        // Birincil anahtar
        // ------------------------------------------------------------
        [Key]
        public int SalonID { get; set; }

        // ------------------------------------------------------------
        // Salon bilgileri
        // ------------------------------------------------------------
        [Required(ErrorMessage = "Salon adı zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Salon Adı")]
        public string SalonAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adres zorunludur.")]
        [StringLength(250)]
        [Display(Name = "Adres")]
        public string SalonAdres { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Açılış Saati")]
        public TimeSpan SalonAcilisSaati { get; set; }

        [Required]
        [Display(Name = "Kapanış Saati")]
        public TimeSpan SalonKapanisSaati { get; set; }

        [StringLength(500)]
        [Display(Name = "Açıklama")]
        public string? SalonAciklama { get; set; }

        // ------------------------------------------------------------
        // İlişkiler
        // ------------------------------------------------------------

        // Salonda verilen hizmetleri temsil eder
        [Display(Name = "Hizmetler")]
        public ICollection<Hizmet>? Hizmetler { get; set; }

        // Salona bağlı antrenörleri temsil eder
        [Display(Name = "Antrenörler")]
        public ICollection<Antrenor>? Antrenorler { get; set; }
    }
}
