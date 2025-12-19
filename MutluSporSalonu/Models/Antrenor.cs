/**
* @file        Antrenor.cs
* @description Mutlu Spor Salonu uygulamasında antrenör varlığını (entity) temsil eden model sınıfı.
*              Entity Framework Core ile veritabanı tablolaşmasını sağlar.
*
*              Sağlanan özellikler:
*              - Antrenör kimlik, iletişim ve uzmanlık bilgilerini tutar.
*              - Antrenörün günlük müsaitlik saat aralığını tanımlar.
*              - Spor salonu, hizmetler ve randevular ile olan ilişkileri belirtir.
*              - DataAnnotations kullanılarak doğrulama ve ekran etiketleri tanımlanır.
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
    public class Antrenor
    {
        // ------------------------------------------------------------
        // Birincil anahtar
        // ------------------------------------------------------------
        [Key]
        public int AntrenorID { get; set; }

        // ------------------------------------------------------------
        // Temel bilgiler
        // ------------------------------------------------------------
        [Required(ErrorMessage = "Ad soyad bilgisi zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Ad Soyad")]
        public string AntrenorAdSoyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
        [StringLength(200)]
        [Display(Name = "Uzmanlık Alanları")]
        public string AntrenorUzmanlikAlanlari { get; set; } = string.Empty;
        // Örnek: "Kas Geliştirme, Yoga, Kilo Verme"

        [Display(Name = "Telefon")]
        [StringLength(20)]
        public string? AntrenorTelefon { get; set; }

        [Display(Name = "E-posta")]
        [EmailAddress]
        public string? AntrenorEposta { get; set; }

        // ------------------------------------------------------------
        // Müsaitlik saatleri
        // ------------------------------------------------------------
        [Required]
        [Display(Name = "Müsaitlik Başlangıç Saati")]
        public TimeSpan AntrenorMusaitlikBaslangic { get; set; }

        [Required]
        [Display(Name = "Müsaitlik Bitiş Saati")]
        public TimeSpan AntrenorMusaitlikBitis { get; set; }

        // ------------------------------------------------------------
        // İlişkiler
        // ------------------------------------------------------------

        // Antrenörün bağlı olduğu spor salonunu belirtir
        [Display(Name = "Bağlı Olduğu Spor Salonu")]
        public int SalonID { get; set; }

        [ForeignKey(nameof(SalonID))]
        public SporSalonu? SporSalonu { get; set; }

        // Antrenörün verebildiği hizmetleri temsil eder (çoktan çoğa ilişki)
        [Display(Name = "Verebildiği Hizmetler")]
        public ICollection<Hizmet>? Hizmetler { get; set; }

        // Antrenöre ait randevu kayıtlarını temsil eder
        [Display(Name = "Randevular")]
        public ICollection<Randevu>? Randevular { get; set; }
    }
}
