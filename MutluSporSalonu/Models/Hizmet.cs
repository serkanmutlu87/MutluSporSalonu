/**
* @file        Hizmet.cs
* @description Mutlu Spor Salonu uygulamasında verilen hizmetleri (paket/servis) temsil eden model sınıfı.
*              Entity Framework Core ile veritabanı tablolaşmasını sağlar.
*
*              Sağlanan özellikler:
*              - Hizmet adı, süresi ve ücret bilgilerini tanımlar.
*              - Hizmet açıklaması için isteğe bağlı alan sunar.
*              - Hizmetin hangi spor salonunda verildiğini belirtir.
*              - Antrenörler ile olan ilişkileri navigation property üzerinden kurar.
*              - DataAnnotations ile doğrulama kuralları ve ekran etiketleri belirler.
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
    public class Hizmet
    {
        // ------------------------------------------------------------
        // Birincil anahtar
        // ------------------------------------------------------------
        [Key]
        public int HizmetID { get; set; }

        // ------------------------------------------------------------
        // Hizmet bilgileri
        // ------------------------------------------------------------
        [Required(ErrorMessage = "Hizmet adı zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Hizmet Adı")]
        public string HizmetAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Süre bilgisi zorunludur.")]
        [Display(Name = "Hizmet Süresi (Dakika)")]
        public int HizmetSureDakika { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Required(ErrorMessage = "Ücret zorunludur.")]
        [Display(Name = "Hizmet Ücreti (₺)")]
        public decimal HizmetUcret { get; set; }

        [StringLength(500)]
        [Display(Name = "Açıklama")]
        public string? HizmetAciklama { get; set; }

        // ------------------------------------------------------------
        // İlişkiler
        // ------------------------------------------------------------

        // Hizmetin bağlı olduğu spor salonunu belirtir
        [Display(Name = "Spor Salonları")]
        public int SalonID { get; set; }

        [ForeignKey(nameof(SalonID))]
        public SporSalonu? SporSalonu { get; set; }

        // Hizmeti verebilen antrenörleri temsil eder
        [Display(Name = "Bu Hizmeti Verebilen Antrenörler")]
        public ICollection<Antrenor>? Antrenorler { get; set; }
    }
}
