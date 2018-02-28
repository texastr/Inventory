using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using TexasTRInventory.Models;

namespace TexasTRInventory.Authorization
{
    /*EXP 2.27.18 modeling after TooBig4YourBritchesAuthorizationHandler*/
    public class LockedProductAuthorizationHandler : AuthorizationHandler<LockedProductAuthorizationHandler, Product> , IAuthorizationRequirement
    {
        protected override Task
            HandleRequirementAsync(AuthorizationHandlerContext context,
                                    LockedProductAuthorizationHandler requirement,
                                    Product resource)
        {
            if(context.User == null || resource == null)
            {
                return Task.FromResult(0);
            }

            if(Utils.CanUserEditProduct(context.User, resource))
            {
                context.Succeed(requirement);
            }

            return Task.FromResult(0);
        }
    }
}
