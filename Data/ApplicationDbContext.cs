using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using yeniWeb.Models;

namespace yeniWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext<UserDetails>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Salon> Salonlar { get; set; } = null!;
        public DbSet<Hizmet> Hizmetler { get; set; } = null!;
        public DbSet<Antrenor> Antrenorler { get; set; } = null!;
        public DbSet<AntrenorHizmet> AntrenorHizmetler { get; set; } = null!;
        public DbSet<AntrenorMusaitlik> AntrenorMusaitlikler { get; set; } = null!;
        public DbSet<Uye> Uyeler { get; set; } = null!;
        public DbSet<Randevu> Randevular { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.Entity<AntrenorHizmet>()
                .HasKey(ah => new { ah.AntrenorId, ah.HizmetId });

            builder.Entity<AntrenorHizmet>()
                .HasOne(ah => ah.Antrenor)
                .WithMany(a => a.AntrenorHizmetler)
                .HasForeignKey(ah => ah.AntrenorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AntrenorHizmet>()
                .HasOne(ah => ah.Hizmet)
                .WithMany(h => h.AntrenorHizmetler)
                .HasForeignKey(ah => ah.HizmetId)
                .OnDelete(DeleteBehavior.NoAction); // multiple cascade'ı engelledik


            builder.Entity<Randevu>()
                .HasOne(r => r.Antrenor)
                .WithMany(a => a.Randevular)
                .HasForeignKey(r => r.AntrenorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Randevu>()
                .HasOne(r => r.Uye)
                .WithMany(u => u.Randevular)
                .HasForeignKey(r => r.UyeId)
                .OnDelete(DeleteBehavior.Cascade); 

            builder.Entity<Randevu>()
                .HasOne(r => r.Hizmet)
                .WithMany(h => h.Randevular)
                .HasForeignKey(r => r.HizmetId)
                .OnDelete(DeleteBehavior.NoAction); // ❗ BURAYI CASCADE YAPMIYORUZ
        }
    }
}
