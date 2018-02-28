using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public static bool CanUserEditProduct(ClaimsPrincipal user, Product product)
        {
            return Utils.CanUserEditProduct(user, product);
        }

        public static bool IsAdmin(ClaimsPrincipal user)
        {
            return Utils.IsAdmin(user);
        }

        public static bool IsInternalUser(ClaimsPrincipal user)
        {
            return Utils.IsInternalUser(user);
        }
    }
    public static class General
    {
        //https://stackoverflow.com/questions/3813934/change-single-url-query-string-value
        public static IDictionary<string,string> AugmentedQueryString(IQueryCollection query, string param=null, string val=null)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            if(!string.IsNullOrEmpty(param))
            {
                ret.Add(param, val);  
            }

            foreach (var q in query)
            {
                if (!string.Equals(q.Key, param, StringComparison.OrdinalIgnoreCase))
                {
                    ret.Add(q.Key, q.Value);
                }
            }

            return ret;
        }

        public static IDictionary<string, string> PrunedQueryString(this IDictionary<string, string> q, params string[] toRemove)
        {
            if(toRemove != null)
            {
                foreach(string str in toRemove)
                {
                    q.Remove(str);
                }
            }
            return q;
        }

        public static IHtmlContent QuotedString(this IHtmlHelper htmlHelper, string arg)
        {
            return htmlHelper.Raw("\"" + arg + "\"");
        }
    }
}
