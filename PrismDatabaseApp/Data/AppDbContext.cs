using Microsoft.EntityFrameworkCore;
using PrismDatabaseApp.Models;
using PrismDatabaseApp.ViewModels;
using PrismDatabaseApp.Views;

namespace PrismDatabaseApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Alarm> Alarms { get; set; }
        public DbSet<Alarm> NotificationInquiry { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Alarm>(entity =>
            {
                entity.ToTable("Alarms");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Message).IsRequired();
                entity.Property(e => e.Timestamp).IsRequired();
            });
        }
    }
}
