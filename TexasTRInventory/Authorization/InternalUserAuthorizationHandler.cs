using System.Threading.Tasks;
using TexasTRInventory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;

/* from https://docs.microsoft.com/en-us/aspnet/core/security/authorization/secure-data
 * This'll be quite different from the tutorial's version.
 * Doesn't care what you're trying to do. Just cares if you're a TexasTREmployee
 * Surprise: It's actually from https://stackoverflow.com/questions/31464359/how-do-you-create-a-custom-authorizeattribute-in-asp-net-core
 */

namespace TexasTRInventory.Authorization
{
    public class InternalUserAuthorizationHandler : AuthorizationHandler<InternalUserAuthorizationHandler>, IAuthorizationRequirement
    {
        protected override Task
            HandleRequirementAsync(AuthorizationHandlerContext context,
                                    InternalUserAuthorizationHandler requirement)
        {
            if (context.User == null)
            {
                return Task.FromResult(0);
            }

            if (Utils.IsInternalUser(context.User))
            {
                context.Succeed(requirement);
            }

            return Task.FromResult(0);
        }
    }
}