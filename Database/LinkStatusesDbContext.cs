
using Database.Configurations;
using LinkStatusDb.Models;
using Microsoft.EntityFrameworkCore;

namespace Database
{
    public class LinkStatusesDbContext(DbContextOptions<LinkStatusesDbContext> options) : DbContext(options)
    {
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<LinkEntity> Links { get; set; }
        public DbSet<LinkStatusEntity> LinkStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new LinkConfiguration());
            modelBuilder.ApplyConfiguration(new LinkStatusConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("LinkStatusesDb");
        }
    }
}