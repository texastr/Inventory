using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Security.Claims;
using System.Web.Configuration;
using Microsoft.AspNetCore.Identity;
using TexasTRInventory.Models;
using TexasTRInventory.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
//using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TexasTRInventory
{
    public static class Utils
    {

        public static async Task<string> GetImageURL(Product product)
        {
            string ret = "";
            string fileName =  product.ImageFilePath?.FileName;
            if (!String.IsNullOrEmpty(fileName))
            {
                try
                {
                    ret = (await GlobalCache.GetBlob(fileName)).Uri.AbsoluteUri;
                }
                catch
                {
                    //If we can't get the image, just return null. NBD
                }
            }

            return ret;
        }

        public static bool IsInternalUser(ClaimsPrincipal user)
        {
            //EXP 9.20.17 no more roles. Everything is supplier.
            return user.HasClaim(Constants.ClaimTypes.IsInternal, true.ToString());
            //return user.IsInRole(Constants.Roles.InternalUser);
        }

        public static int SupplierID (ClaimsPrincipal user)
        {
            int.TryParse(user.FindFirst(Constants.ClaimTypes.EmployerID).Value, out int ret);
            return ret;
        }

        public static bool IsInternalCompany(Company company)
        {
            return company.IsInternal;
        }

        //EXP 9.19.17 Gonna get fancy with an extension method
        public static async Task<SignInResult> PasswordSignInExcludeDisabled(this SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager, string email, string password, bool rememberMe, bool lockoutOnFailure)
        {
            if ((await userManager.FindByEmailAsync(email)).IsDisabled)
            {
                return SignInResult.NotAllowed;
            }
            return await signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure);
        }

        public static SelectList CompanyList(InventoryContext context, Company selected = null, bool excludeInternal = true)
        {
            IEnumerable<Company> suppliers = context.Companies;

            if (excludeInternal)
            {
                int texasTRID = GlobalCache.GetTexasTRCompanyID(context);
                suppliers = suppliers.Where(s => s.ID != texasTRID);
            }

            SelectList ret = new SelectList(suppliers, nameof(Company.ID), nameof(Company.Name), selected); //Hope that this works when selected is null.

            return ret;

        }

        //EXP 9.25.17. Copied from SO. Will break if we move to a different implementation of SQL
        public static bool IsUniqueKeyViolation(Exception ex)
        {
            SqlException sqlEx = ex as SqlException;
            if (sqlEx is null)
            {
                return false;
            }

            return sqlEx.Errors.Cast<SqlError>().Any(e => e.Class == 14 && (e.Number == 2601 || e.Number == 2627));
        }

        //Probably trash. let me see if I can swing this
        /*public static ViewResult badRequest(ViewDataDictionary viewData)
        {
            viewData[Constants.keyNames.ErrorDetails] = "The URL you request has bad or missing values";
            return View("Error");
        }*/

        public static void AddErrors(IdentityResult result, ModelStateDictionary modelState)
        {
            foreach (var error in result.Errors)
            {
                modelState.AddModelError(string.Empty, error.Description);
            }
        }

    }

    //EXP 9.20.17 I'm feeling allergic and can't think straight. I can't see shit on this small screen. But this is copied from
    //https://adrientorris.github.io/aspnet-core/identity/extend-user-model.html
    public class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        private InventoryContext _context;

        public AppClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager
            , RoleManager<IdentityRole> roleManager
            , IOptions<IdentityOptions> optionsAccessor
            , InventoryContext context)//EXP Can I add the context as a parameter??
        : base(userManager, roleManager, optionsAccessor)
        {
            _context = context;
        }

        public async override Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            var principal = await base.CreateAsync(user);
            ((ClaimsIdentity)principal.Identity).AddClaims(new[] {
                new Claim(Constants.ClaimTypes.IsInternal,(user.EmployerID==GlobalCache.GetTexasTRCompanyID(_context)).ToString()),
                new Claim(Constants.ClaimTypes.EmployerID,user.EmployerID.ToString())
            });
            return principal;
        }

    }
}

