using TexasTRInventory.Models;
using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity.Owin;

namespace TexasTRInventory.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(InventoryContext context)
        {
            context.Database.EnsureCreated();
            if (!context.Companies.Where(c => c.IsInternal).Any())
            {
                context.Companies.Add(new Company {Name = Constants.TexasTRCompanyName, IsInternal=true });
                context.SaveChanges();
            }
        
            if (!context.Users.Any())
            {
                //In theory, things could go wrong, because the other users will have a userManager/userStore from DI
                //But trying to get the DI one in here is not worth the risk and difficulty
                using (var userStore = new UserStore<ApplicationUser>(context))
                {
                    using (var userManager = new UserManager<ApplicationUser>(userStore,null,new PasswordHasher<ApplicationUser>(),null,null,new UpperInvariantLookupNormalizer(),null,null,null))
                    {
                        string adminIdentifier = GlobalCache.GetAdminUsername();
                        ApplicationUser expUser = new ApplicationUser()
                        {
                            UserName = adminIdentifier,
                            Email = adminIdentifier,
                            EmailConfirmed = true,
                            EmployerID = GlobalCache.GetTexasTRCompanyID(context)
                        };

                        string pwd = await GlobalCache.GetSecret(Constants.SecretNames.AdminInitializer);

                        var chkUser = await userManager.CreateAsync(expUser, pwd);
                    }
                }
            }
        }
    
        //EXP 9.12.17. Making its own method
        public static void TrashInitialize(InventoryContext context)
        {

            //if (context.Products.Any()) return;
            var suppliers = new Company[]
            {
                    new Company{/*ID=98,*/ Name="First Supplier"},//Comenting out the IDs, to see if that'll cause it to stop throwing errors
                    new Company{/*ID=2,*/ Name="Second Supplier"},
                    new Company{/*ID=3, */Name="Third Supplier"},
                    new Company{/*ID=4,*/ Name="Fourth Supplier"},
            };
            foreach (Company m in suppliers)
            {
                context.Companies.Add(m);
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