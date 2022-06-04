using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityStructureModel.IdentityModels;

namespace IdentityStructureModel.CustomValidations
{
    public class CustomPasswordValidator : IPasswordValidator<AppUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string password)
        {
            List<IdentityError> errors = new List<IdentityError>();

            if (password.ToLower().Contains(user.UserName.ToLower()))
            {
                if (!user.Email.Contains(user.UserName))
                {
                    errors.Add(new IdentityError() { Code = "UserNameError", Description = "Şifre içerisinde kullanıcı adı bulunamaz." });
                }
            }

            //you can add more validator right here


            if (errors.Count == 0)
            {
                return Task.FromResult(IdentityResult.Success);
            }
            else
            {
                return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
            }
        }
    }
}