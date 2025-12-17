using System.ComponentModel.DataAnnotations;

namespace MutluSporSalonu.Models
{
    public class Hizmet
    {
        [Key]
        public int HizmetID { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Hizmet Adı")]
        public string HizmetAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Süre bilgisi zorunludur.")]
        [Display(Name = "Hizmet Süresi (Dakika)")]
        public int HizmetSureDakika { get; set; }

        [Required(ErrorMessage = "Ücret zorunludur.")]
        [Display(Name = "Hizmet Ücreti (₺)")]
        public decimal HizmetUcret { get; set; }

        [StringLength(500)]
        [Display(Name = "Açıklama")]
        public string? HizmetAciklama { get; set; }

        // İlişkiler
        [Display(Name = "Spor Salonları")]
        public int SporSalonuID { get; set; }
        public SporSalonu? SporSalonu { get; set; }

        [Display(Name = "Bu Hizmeti Verebilen Antrenörler")]
        public ICollection<Antrenor>? Antrenorler { get; set; }
    }
}