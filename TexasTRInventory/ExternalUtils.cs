using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TexasTRInventory.Models;

namespace TexasTRInventory.ExternalUtils
{
    /**
     * The purpose of this class is write code that will be called from razor pages, or IOW from anywhere where "find all references" won't find it.
     * Tags here should generally be short and just call tags that are defined in a different class, whereever is appropriate for the tag.
     * */
    public static class ApplicationUserUtils
    {
        public static bool CanUserEditUser(ClaimsPrincipal cp, ApplicationUser au)
        {
            return Utils.CanUserEditUser(cp, au);
        }

        public static bool IsAdmin(ClaimsPrincipal user)
        {
            return Utils.IsAdmin(user);
        }
    }
}
