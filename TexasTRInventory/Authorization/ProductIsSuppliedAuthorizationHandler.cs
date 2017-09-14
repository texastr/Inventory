using System.Threading.Tasks;
using TexasTRInventory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace TexasTRInventory.Authorization
{
    public class ProductIsSuppliedAuthorizationHandler
                : AuthorizationHandler<OperationAuthorizationRequirement, Product>
    {
        UserManager<ApplicationUser> _userManager;

        public ProductIsSuppliedAuthorizationHandler(UserManager<ApplicationUser>
            userManager)
        {
            _userManager = userManager;
        }

        protected override Task
            HandleRequirementAsync(AuthorizationHandlerContext context,
                                   OperationAuthorizationRequirement requirement,
                                   Product resource)
        {
            if (context.User == null || resource == null)
            {
                return Task.FromResult(0);
            }

            // If we're not asking for CRUD permission, return.
            /*EXP 9.11.17 I think I can comment this out. If you're not authorized, then you're not authorized at all
            if (requirement.Name != Constants.CreateOperationName &&
                requirement.Name != Constants.ReadOperationName &&
                requirement.Name != Constants.UpdateOperationName &&
                requirement.Name != Constants.DeleteOperationName)
            {
                return Task.FromResult(0);
            }
            */
            //Immediate TODO. Update the users so that they have a supplier field
            if (context.User.HasClaim("string which signifies supplier field",resource.SupplierID.ToString()))
            {
                context.Succeed(requirement);
            }

            return Task.FromResult(0);
        }
    }
}