﻿using TexasTRInventory.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace TexasTRInventory.Data
{
    public class InventoryContext : IdentityDbContext<ApplicationUser>
    {
        public InventoryContext(DbContextOptions<InventoryContext> options) : base(options)
        {
        }

        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<FilePath> FilePaths { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);//EXP 8.9.17 Got this from SA. please god let it work.
            modelBuilder.Entity<Manufacturer>().ToTable("Manufacturer");
            modelBuilder.Entity<Product>().ToTable("Product");
            modelBuilder.Entity<FilePath>().ToTable("FilePath");

        }
    }
}