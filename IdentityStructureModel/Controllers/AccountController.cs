using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityStructureModel.IdentityModels;
using IdentityStructureModel.ViewModels;
using Microsoft.AspNetCore.Identity;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace IdentityStructureModel.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult Login(string ReturnUrl)
        {
            TempData["ReturnUrl"] = ReturnUrl;
            return View(new LoginViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            string failedExceptionDesc = "Kullanıcı adı veya şifre hatalı";
            bool lockoutUserWhenFailed = false;
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", failedExceptionDesc);
                return View(model);
            }

            SignInResult loginResult =
                await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutUserWhenFailed);

            if (!loginResult.Succeeded)
            {
                ModelState.AddModelError("", failedExceptionDesc);
                return View(model);
            }

            if (TempData["ReturnUrl"] != null && string.IsNullOrEmpty(TempData["ReturnUrl"].ToString()))
            {
                return Redirect(TempData["ReturnUrl"].ToString());
            }

            return RedirectToAction("Index","Home");
        }

        public IActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignUp(UserSignUpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = model.CreateUser();
            IdentityResult result = await _userManager.CreateAsync(user,model.Password);
            if (!result.Succeeded)
            {
                result.Errors.ToList().ForEach(i =>
                {
                    ModelState.AddModelError("",i.Description);
                });
                return View();
            }

            return RedirectToAction("Login");
        }
    }
}
