using System.ComponentModel.DataAnnotations;

namespace MutluSporSalonu.Models
{
    public class SporSalonu
    {
        [Key]
        public int SalonID { get; set; }

        [Required(ErrorMessage = "Salon adı zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Salon Adı")]
        public string SalonAdi { get; set; }

        [Required(ErrorMessage = "Adres zorunludur.")]
        [StringLength(250)]
        [Display(Name = "Adres")]
        public string SalonAdres { get; set; }

        [Required]
        [Display(Name = "Açılış Saati")]
        public TimeSpan SalonAcilisSaati { get; set; }

        [Required]
        [Display(Name = "Kapanış Saati")]
        public TimeSpan SalonKapanisSaati { get; set; }

        [StringLength(500)]
        [Display(Name = "Açıklama")]
        public string SalonAciklama { get; set; }

        // İlişkiler
        [Display(Name = "Hizmetler")]
        public ICollection<Hizmet>? Hizmetler { get; set; }

        [Display(Name = "Antrenörler")]
        public ICollection<Antrenor>? Antrenorler { get; set; }
    }
}