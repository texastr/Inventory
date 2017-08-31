using TexasTRInventory.Models;
using System;
using System.Linq;

namespace TexasTRInventory.Data
{
    public static class DbInitializer
    {
        public static void Initialize(InventoryContext context)
        {
            context.Database.EnsureCreated();

            // Look for any students.
            if (context.Products.Any())
            {
                return;   // DB has been seeded
            }

            var manufacturers = new Manufacturer[]
            {
                new Manufacturer{/*ID=1,*/ Name="First Manufacturer"},//Comenting out the IDs, to see if that'll cause it to stop throwing errors
                new Manufacturer{/*ID=2,*/ Name="Second Manufacturer"},
                new Manufacturer{/*ID=3, */Name="Third Manufacturer"},
                new Manufacturer{/*ID=4,*/ Name="Fourth Manufacturer"},
            };
            foreach (Manufacturer m in manufacturers)
            {
                context.Manufacturers.Add(m);
            }
            context.SaveChanges();

            var products = new Product[]
            {
                new Product{Name="the first product", ManufacturerID=1, Info="this product, from producer 1, rocks the fucking house"},
                new Product{Name="the first product", ManufacturerID=2, Info="this product, from producer 2, rocks the fucking house"},
                new Product{Name="the first product", ManufacturerID=3, Info="this product, from producer 3, rocks the fucking house"},
                new Product{Name="the first product", ManufacturerID=4, Info="this product, from producer 4, rocks the fucking house"},
            };
            foreach (Product c in products)
            {
                context.Products.Add(c);
            }
            context.SaveChanges();
            
        }
    }
}