using System.ComponentModel.DataAnnotations;

namespace MutluSporSalonu.Models
{
    public class Uye
    {
        [Key]
        public int UyeID { get; set; }

        [Required(ErrorMessage = "Ad soyad zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Ad Soyad")]
        public string UyeAdSoyad { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string UyeEposta { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Şifre en az 4 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string UyeSifre { get; set; }

        [Display(Name = "Telefon")]
        [StringLength(20)]
        public string? UyeTelefon { get; set; }

        [Display(Name = "Kayıt Tarihi")]
        public DateTime KayitTarihi { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        [Display(Name = "Rol")]
        public string Rol { get; set; } = "Uye";
        // "Admin" / "Uye"

        // İlişkiler
        [Display(Name = "Randevular")]
        public ICollection<Randevu>? Randevular { get; set; }
    }
}
