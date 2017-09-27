using TexasTRInventory.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace TexasTRInventory.Data
{
    public class InventoryContext : IdentityDbContext<ApplicationUser>
    {
        public InventoryContext(DbContextOptions<InventoryContext> options) : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<FilePath> FilePaths { get; set; }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);//EXP 8.9.17 Got this from SA. please god let it work.
            modelBuilder.Entity<Company>().ToTable(nameof(Company)).Property(t => t.Name).HasMaxLength(250);
                modelBuilder.Entity<Company>().HasIndex(c => c.Name).IsUnique(); //EXP 9.25.17. Putting in an index
            modelBuilder.Entity<Product>().ToTable(nameof(Product));
            modelBuilder.Entity<FilePath>().ToTable(nameof(FilePath));

        }
    }
}