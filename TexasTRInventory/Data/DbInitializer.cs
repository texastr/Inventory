using TexasTRInventory.Models;
using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Threading.Tasks;

namespace TexasTRInventory.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(InventoryContext context)
        {
            context.Database.EnsureCreated();
            if (!context.Roles.Any())
            {
                using (var roleStore = new RoleStore<IdentityRole>(context))
                {
                    using (var roleManager = new RoleManager<IdentityRole>(roleStore, null, null, null, null, null))
                    {
                        foreach (string role in Constants.Roles.ToArray())
                        {
                            await roleManager.CreateAsync(new IdentityRole(role));
                        }
                    }
                }
            }
            if (!context.Users.Any())
            {
                //In theory, things could go wrong, because the other users will have a userManager/userStore from DI
                //But trying to get the DI one in here is not worth the risk and difficulty
                using (var userStore = new UserStore<ApplicationUser>(context))
                {
                    using (var userManager = new UserManager<ApplicationUser>(userStore,null,new PasswordHasher<ApplicationUser>(),null,null,null,null,null,null))
                    {
                        string adminIdentifier = GlobalCache.Indexer(Constants.ConfigNames.AdminUsername);
                        ApplicationUser expUser = new ApplicationUser()
                        {
                            UserName = adminIdentifier,
                            Email = adminIdentifier,
                            EmailConfirmed = true
                        };

                        string pwd = await GlobalCache.GetSecret(Constants.SecretNames.AdminInitializer);

                        var chkUser = await userManager.CreateAsync(expUser, pwd);
                        if (chkUser.Succeeded)
                        {
                            await userManager.AddToRolesAsync(expUser, new string[] { Constants.Roles.Administrator, Constants.Roles.InternalUser}); 
                        }
                    }
                }
            }
        }
    
        //EXP 9.12.17. Making its own method
        public static void TrashIntialize(InventoryContext context)
        {

            if (context.Products.Any()) return;
            var suppliers = new Supplier[]
            {
                    new Supplier{/*ID=1,*/ Name="First Supplier"},//Comenting out the IDs, to see if that'll cause it to stop throwing errors
                    new Supplier{/*ID=2,*/ Name="Second Supplier"},
                    new Supplier{/*ID=3, */Name="Third Supplier"},
                    new Supplier{/*ID=4,*/ Name="Fourth Supplier"},
            };
            foreach (Supplier m in suppliers)
            {
                context.Suppliers.Add(m);
            }
            context.SaveChanges();

            var products = new Product[]
            {
                    new Product{Name="the first product", SupplierID=1, Info="this product, from producer 1, rocks the fucking house"},
                    new Product{Name="the first product", SupplierID=2, Info="this product, from producer 2, rocks the fucking house"},
                    new Product{Name="the first product", SupplierID=3, Info="this product, from producer 3, rocks the fucking house"},
                    new Product{Name="the first product", SupplierID=4, Info="this product, from producer 4, rocks the fucking house"},
            };
            foreach (Product c in products)
            {
                context.Products.Add(c);
            }
            context.SaveChanges();
        }
    }
}