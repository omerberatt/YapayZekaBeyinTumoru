using Microsoft.EntityFrameworkCore;
using SIFAIBackend.Entities;

namespace SIFAIBackend.DataAccess
{
    public class SifaiContext : DbContext
    {
        public SifaiContext(DbContextOptions<SifaiContext> options) : base(options)
        {
        }

        // User tablosu
        public DbSet<User> Users { get; set; }

        // TumorDetectionHistory tablosu
        public DbSet<TumorDetectionHistory> TumorDetectionHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User tablosu için kurallar
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(u => u.Password)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.Property(u => u.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            // TumorDetectionHistory tablosu için kurallar
            modelBuilder.Entity<TumorDetectionHistory>(entity =>
            {
                entity.ToTable("TumorDetectionHistory");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.ImageUrl)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.Property(t => t.TumorType)
                    .HasMaxLength(100);
                entity.Property(t => t.DetectionDate)
                    .IsRequired();
                entity.HasOne<User>() // User ile ilişki tanımı
                    .WithMany()
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
