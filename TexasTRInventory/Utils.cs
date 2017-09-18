using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Security.Claims;
using System.Web.Configuration;
using Microsoft.AspNetCore.Identity;
using TexasTRInventory.Models;

namespace TexasTRInventory
{
    public class Utils
    {

        public static bool IsInternalUser(ClaimsPrincipal user)
        {
            return user.IsInRole(Constants.Roles.InternalUser);
        }

        /// <summary>
        /// finds the ID of the supplier associated with the user
        /// </summary>
        /// <param name="user">user whose associated ID we want to check</param>
        /// <returns>the supplier ID, if there is one. Null, if anything goes wrong</returns>
        public static int? SafeFindSupplierID(ClaimsPrincipal user)
        {
            Claim supplierClaim = user.FindFirst(Constants.ClaimNames.Supplier);
            if(supplierClaim == null)
            {
                return null;
            }

            string supplierStr = supplierClaim.Value;

            if (string.IsNullOrWhiteSpace(supplierStr))
            {
                return null;
            }

            int ret;

            if(!int.TryParse(supplierStr, out ret))
            {
                return null;
            }

            return ret;
        }

    }
}

