using Microsoft.EntityFrameworkCore;
using ProjectAI.Data.Entities;

namespace ProjectAI.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Request> Requests { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Material> Materials { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<Item>()
                .HasOne(i => i.Request)
                .WithMany(r => r.Items)
                .HasForeignKey(i => i.RequestId);

            modelBuilder.Entity<Material>()
                .HasOne(m => m.Item)
                .WithMany(i => i.Materials)
                .HasForeignKey(m => m.ItemId);
        }
    }
}
