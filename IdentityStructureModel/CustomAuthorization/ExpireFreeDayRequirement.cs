using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace IdentityStructureModel.CustomAuthorization
{
    public class ExpireFreeDayRequirement : IAuthorizationRequirement
    {

    }

    public class ExpireFreeDayHandler : AuthorizationHandler<ExpireFreeDayRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ExpireFreeDayRequirement requirement)
        {
            if (context.User != null && context.User.Identity != null)
            {
                var claim = context.User.Claims.Where(i => i.Type == "freeDay").FirstOrDefault();
                if (claim != null)
                {
                    if (DateTime.Now < Convert.ToDateTime(claim.Value))
                    {
                        context.Succeed(requirement);
                        return Task.CompletedTask;

                    }
                }
                
            }
            context.Fail();
            return  Task.CompletedTask;
        }
    }
}
