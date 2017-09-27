using System.Threading.Tasks;
using TexasTRInventory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace TexasTRInventory.Authorization
{
    public class AdministratorAuthorizationHandler
                    : AuthorizationHandler<OperationAuthorizationRequirement> 
    {
        protected override Task HandleRequirementAsync(
                                              AuthorizationHandlerContext context,
                                    OperationAuthorizationRequirement requirement)
        {
            if (context.User == null)
            {
                return Task.FromResult(0);
            }

            // Administrators can do anything.
            //Get in the user record that some users (i.e. me) are top fucking dogs
            if (context.User.IsInRole("some string that I will determine means administrator"))
            {
                context.Succeed(requirement);
            }

            return Task.FromResult(0);
        }
    }
}