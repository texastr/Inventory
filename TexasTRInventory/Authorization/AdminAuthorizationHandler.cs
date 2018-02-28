using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TexasTRInventory.Authorization
{
    /*
     * EXP 2.27.18. Checks if the user is an admin. copied from InternalUserAuthorizationHandler
     */
    public class AdminAuthorizationHandler: AuthorizationHandler<AdminAuthorizationHandler>, IAuthorizationRequirement
    {
        protected override Task
            HandleRequirementAsync(AuthorizationHandlerContext context,
                                    AdminAuthorizationHandler requirement)
        {
            if (context.User == null)
            {
                return Task.FromResult(0);
            }

            if (Utils.IsAdmin(context.User))
            {
                context.Succeed(requirement);
            }

            return Task.FromResult(0);
        }
    }
}