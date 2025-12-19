/**
* @file        Uye.cs
* @description Mutlu Spor Salonu uygulamasında üye (kullanıcı) varlığını temsil eden model sınıfı.
*              Entity Framework Core ile veritabanı tablolaşmasını sağlar.
*
*              Sağlanan özellikler:
*              - Üyeye ait kimlik, iletişim ve giriş bilgilerini tutar.
*              - E-posta ve şifre alanları için doğrulama kurallarını tanımlar.
*              - Kullanıcı rolünü (Admin / Uye) veri katmanında saklar.
*              - Üyeye ait randevu kayıtları ile olan ilişkiyi belirtir.
*              - DataAnnotations kullanılarak doğrulama ve ekran etiketleri belirlenir.
*
* @course      BSM 311 Web Programlama
* @assignment  Dönem Projesi – MutluSporSalonu
* @date        20.12.2025
* @author      D255012008 - Serkan Mutlu
*/

using System.ComponentModel.DataAnnotations;

namespace MutluSporSalonu.Models
{
    public class Uye
    {
        // ------------------------------------------------------------
        // Birincil anahtar
        // ------------------------------------------------------------
        [Key]
        public int UyeID { get; set; }

        // ------------------------------------------------------------
        // Temel üye bilgileri
        // ------------------------------------------------------------
        [Required(ErrorMessage = "Ad soyad zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Ad Soyad")]
        public string UyeAdSoyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string UyeEposta { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Şifre en az 3 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string UyeSifre { get; set; } = string.Empty;

        [Display(Name = "Telefon")]
        [StringLength(20)]
        public string? UyeTelefon { get; set; }

        // ------------------------------------------------------------
        // Sistem alanları
        // ------------------------------------------------------------
        [Display(Name = "Kayıt Tarihi")]
        public DateTime KayitTarihi { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        [Display(Name = "Rol")]
        public string Rol { get; set; } = "Uye";
        // Olası değerler: "Admin", "Uye"

        // ------------------------------------------------------------
        // İlişkiler
        // ------------------------------------------------------------

        // Üyeye ait randevu kayıtlarını temsil eder
        [Display(Name = "Randevular")]
        public ICollection<Randevu>? Randevular { get; set; }
    }
}
