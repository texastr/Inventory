﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Security.Claims;
using System.Web.Configuration;
using Microsoft.AspNetCore.Identity;
using TexasTRInventory.Models;
using TexasTRInventory.Data;
using TexasTRInventory.Constants;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Html;
using System.Reflection;

namespace TexasTRInventory
{
    public static class Utils
    {
		public static Dictionary<string,PropertyInfo> GetPropertyInfoByName(Type type)
		{
			Dictionary<string, PropertyInfo> ret = new Dictionary<string, PropertyInfo>();
			foreach (PropertyInfo pi in type.GetProperties())
			{
				ret.Add(pi.Name, pi);
			}

			return ret;
		}

        /*EXP. Now relying on third party to do it.
        public static D ModelMapper<D,S>(D destination, S source, params string[] excludedPropertyNamesAry)
		{
			Dictionary<string,PropertyInfo> sourceProperties = Utils.GetPropertyInfoByName(typeof(S));

            PropertyInfo[] destinationProperties = typeof(D).GetProperties();

            IEnumerable<PropertyInfo> iter = sourceProperties.Values.Intersect(destinationProperties);

            HashSet<string> excludedPropertyNamesSet = new HashSet<string>(excludedPropertyNamesAry);

            foreach (PropertyInfo pi in iter)
			{
				string propName = pi.Name;
				if (excludedPropertyNamesSet.Contains(propName))
				{
					continue;
				}

				var propValue = sourceProperties[propName].GetValue(source);
				pi.SetValue(destination, propValue);
			}


			return destination;
		}
        */

        public static bool IsInternalUser(ClaimsPrincipal user)
        {
            //EXP 9.20.17 no more roles. Everything is supplier.
            return user.HasClaim(ClaimNames.IsInternal, true.ToString());
            //return user.IsInRole(Constants.Roles.InternalUser);
        }
        
        public static bool IsAdmin(ClaimsPrincipal user)
        {
            return user.HasClaim(ClaimNames.IsAdmin, true.ToString());
        }
        
        public static bool CanUserEditUser(ClaimsPrincipal cp, ApplicationUser au)
        {
            if (!IsInternalUser(cp))
            {
                return false;
            }
            if (!au.IsAdmin)
            {
                //Any internal can delete a non-admin
                return true;
            }
            if (!IsAdmin(cp))
            {
                //a non-admin is trying to delete an admin! that's bad!
                return false;
            }
            //If we make it here, the user is internal, an admin, trying to delete an admin. wtf. Let him do it. let them squabble
            return true;
        }

        public static bool CanUserEditProduct(ClaimsPrincipal user, Product product)
        {
            if (!IsInternalUser(user) && product.IsAdminApproved)
            {
                return false;
            }
            return true;
        }

        public static int SupplierID (ClaimsPrincipal user)
        {
            int.TryParse(user.FindFirst(ClaimNames.EmployerID).Value, out int ret);
            return ret;
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
                new Claim(ClaimNames.IsInternal,(user.EmployerID==GlobalCache.GetTexasTRCompanyID(_context)).ToString()),
                new Claim(ClaimNames.EmployerID,user.EmployerID.ToString()),
                new Claim(ClaimNames.IsAdmin,user.IsAdmin.ToString())
            });
            return principal;
        }

    }
}

