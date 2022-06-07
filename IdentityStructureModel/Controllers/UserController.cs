using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using IdentityStructureModel.IdentityModels;
using IdentityStructureModel.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IdentityStructureModel.Controllers
{
    [Authorize]
    public class UserController : BaseController
    {
        public UserController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            RoleManager<AppRole> roleManager) : base(signInManager, userManager, roleManager)
        {

        }

        public IActionResult Index()
        {
            // Controller kıstasında authorize attribude kullanıldığından current user null gelmez ondan dolayı if kontrolü yapılmamıştır
            AppUser user = currentUser;
            UserViewModel viewModel = user.CreateUserViewModel();
            return View(viewModel);
        }

        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) View(model);

            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var result = await _userManager.ChangePasswordAsync(user, model.PasswordOld, model.PasswordNew);
            if (!result.Succeeded)
            {
                result.Errors.ToList().ForEach(i => { ModelState.AddModelError("", i.Description); });
                model.ResetModel();
                return View(model);
            }

            await _userManager.UpdateSecurityStampAsync(user);
            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user, true);
            ViewBag.success = "true";
            return View(model);
        }

        public async Task<IActionResult> UserEdit()
        {
            var user = currentUser.CreateUserViewModel();
            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)), user.Gender);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> UserEdit(UserViewModel model, IFormFile userPicture)
        {
            if (ModelState.IsValid) View(model);
            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)), model.Gender);

            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            model.UpdateUser(user);
            var result = await _userManager.UpdateAsync(user);
            if (userPicture != null && userPicture.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(userPicture.FileName);

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/UserPicture", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await userPicture.CopyToAsync(stream);
                    user.Picture = "/UserPicture/" + fileName;
                }
            }
            if (!result.Succeeded)
            {
                result.Errors.ToList().ForEach(i =>
                {
                    ModelState.AddModelError("", i.Description);
                });
                return View(model);
            }
            await _userManager.UpdateSecurityStampAsync(user);
            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user, true);
            ViewBag.success = "true";
            return View(model);
        }
    }
}
