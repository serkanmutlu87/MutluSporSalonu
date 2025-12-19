/**
* @file        DBContext.cs
* @description Mutlu Spor Salonu uygulamasında Entity Framework Core veritabanı bağlamını (DbContext)
*              temsil eden sınıf.
*
*              Sağlanan işlevler:
*              - Veritabanı ile uygulama arasındaki bağlantıyı kurar.
*              - Uygulamada kullanılan tüm entity setlerini (DbSet) tanımlar.
*              - Antrenör, hizmet, randevu, spor salonu ve üye tablolarının
*                EF Core üzerinden yönetilmesini sağlar.
*              - SQL Server LocalDB bağlantı ayarını merkezi olarak tanımlar.
*
* @course      BSM 311 Web Programlama
* @assignment  Dönem Projesi – MutluSporSalonu
* @date        20.12.2025
* @author      D255012008 - Serkan Mutlu
*/

using Microsoft.EntityFrameworkCore;

namespace MutluSporSalonu.Models
{
    public class DBContext : DbContext
    {
        // DbContext yapılandırmasını dışarıdan (Program.cs) alır
        public DBContext(DbContextOptions<DBContext> options) : base(options) { }

        // ------------------------------------------------------------
        // DbSet tanımları
        // Her DbSet, veritabanında bir tabloyu temsil eder
        // ------------------------------------------------------------
        public DbSet<Antrenor> Antrenorler { get; set; }
        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<Randevu> Randevular { get; set; }
        public DbSet<SporSalonu> Salonlar { get; set; }
        public DbSet<Uye> Uyeler { get; set; }

        // ------------------------------------------------------------
        // Veritabanı bağlantı ayarı
        // ------------------------------------------------------------
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\mssqllocaldb;
                  Database=MutluSporSalonu;
                  Trusted_Connection=True;");
        }
    }
}
