using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityStructureModel.IdentityModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace IdentityStructureModel.CustomTagHelper
{
    [HtmlTargetElement("td",Attributes = "user-roles")]
    public class UserRolesNames:TagHelper
    {
        private  UserManager<AppUser> _userManager { get; set; }

        public UserRolesNames(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        [HtmlAttributeName("user-roles")]
        public string userId { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            AppUser user = await _userManager.FindByIdAsync(userId);
            var roles= await _userManager.GetRolesAsync(user);
            string html = "";
            if (roles != null)
            {
                roles.ToList().ForEach(i =>
                {
                    html += $"<span class='badge badge-info'>{i}</span>";
                });
            }
            output.Content.SetHtmlContent(html);
        }
    }
}
