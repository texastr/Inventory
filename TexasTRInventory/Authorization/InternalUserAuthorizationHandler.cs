using System.Threading.Tasks;
using TexasTRInventory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;

/* from https://docs.microsoft.com/en-us/aspnet/core/security/authorization/secure-data
 * This'll be quite different from the tutorial's version.
 * Doesn't care what you're trying to do. Just cares if you're a TexasTREmployee
 */

namespace TexasTRInventory.Authorization
{
    public class InternalUserAuthorizationHandler :
        AuthorizationHandler<OperationAuthorizationRequirement>
    {
        protected override Task
            HandleRequirementAsync(AuthorizationHandlerContext context,
                                   OperationAuthorizationRequirement requirement)
        {
            if (context.User == null)
            {
                return Task.FromResult(0);
            }

            //Immediate TODO. Put in user model that the user is a TexasTR person
            if (context.User.IsInRole("Some string that I determine means the user is a TexasTR person"))
            {
                context.Succeed(requirement);
            }

            return Task.FromResult(0);
        }
    }
}