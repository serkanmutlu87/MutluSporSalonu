using Microsoft.EntityFrameworkCore;

namespace MutluSporSalonu.Models
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options) { }
        public DbSet<Antrenor> Antrenorler { get; set; }
        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<Randevu> Randevular { get; set; }
        public DbSet<SporSalonu> Salonlar { get; set; }
        public DbSet<Uye> Uyeler { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb; 
                Database=MutluSporSalonu;Trusted_Connection=True;");
        }

    }
}
