using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TexasTRInventory.Models;

namespace TexasTRInventory.Authorization
{
    public class TooBig4YourBritchesAuthorizationHandler : AuthorizationHandler<TooBig4YourBritchesAuthorizationHandler, ApplicationUser>, IAuthorizationRequirement
    {
        protected override Task 
            HandleRequirementAsync(AuthorizationHandlerContext context,
                                    TooBig4YourBritchesAuthorizationHandler requirement,
                                    ApplicationUser resource)
        {
            if (context.User == null || resource == null)
            {
                return Task.FromResult(0);
            }

            if (Utils.CanUserEditUser(context.User,resource))
            {
                context.Succeed(requirement);
            }

            return Task.FromResult(0);
        }

    }
}
