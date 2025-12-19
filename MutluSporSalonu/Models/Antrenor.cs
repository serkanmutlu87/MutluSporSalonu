using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MutluSporSalonu.Models
{
    public class Antrenor
    {
        [Key]
        public int AntrenorID { get; set; }

        [Required(ErrorMessage = "Ad soyad bilgisi zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Ad Soyad")]
        public string AntrenorAdSoyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
        [StringLength(200)]
        [Display(Name = "Uzmanlık Alanları")]
        public string AntrenorUzmanlikAlanlari { get; set; } = string.Empty;
        // Örn: "Kas Geliştirme, Yoga, Kilo Verme"

        [Display(Name = "Telefon")]
        [StringLength(20)]
        public string? AntrenorTelefon { get; set; }

        [Display(Name = "E-posta")]
        [EmailAddress]
        public string? AntrenorEposta { get; set; }

        // Müsaitlik Saatleri
        [Required]
        [Display(Name = "Müsaitlik Başlangıç Saati")]
        public TimeSpan AntrenorMusaitlikBaslangic { get; set; }

        [Required]
        [Display(Name = "Müsaitlik Bitiş Saati")]
        public TimeSpan AntrenorMusaitlikBitis { get; set; }

        // İlişkiler
        [Display(Name = "Bağlı Olduğu Spor Salonu")]
        public int SalonID { get; set; }
        [ForeignKey(nameof(SalonID))]
        public SporSalonu? SporSalonu { get; set; }

        [Display(Name = "Verebildiği Hizmetler")]
        public ICollection<Hizmet>? Hizmetler { get; set; }

        [Display(Name = "Randevular")]
        public ICollection<Randevu>? Randevular { get; set; }
    }
}