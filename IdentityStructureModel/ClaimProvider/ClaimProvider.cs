using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityStructureModel.IdentityModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace IdentityStructureModel.ClaimProvider
{
    public class ClaimProvider : IClaimsTransformation
    {
        private readonly UserManager<AppUser> _userManager;

        public ClaimProvider(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal != null && principal.Identity != null && principal.Identity.IsAuthenticated)
            {
                ClaimsIdentity identity = principal.Identity as ClaimsIdentity;

                AppUser user = await _userManager.FindByNameAsync(identity.Name);

                if (user != null)
                {
                    if (!string.IsNullOrEmpty(user.City))
                    {
                        if (!principal.HasClaim(c => c.Type == "city"))
                        {
                            Claim CityClaim = new Claim("city",user.City,ClaimValueTypes.String,"custom");
                            identity.AddClaim(CityClaim);
                        }
                    }
                }
            }
            return principal;
        }
    }
}
