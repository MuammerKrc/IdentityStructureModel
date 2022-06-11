using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityStructureModel.EmailSender;
using IdentityStructureModel.HelperMethod;
using IdentityStructureModel.IdentityModels;
using IdentityStructureModel.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace IdentityStructureModel.Controllers
{
    public class AccountController : _BaseController
    {
        private readonly IEmailSender _emailSender;
        string userErrorMessage = "Kullanıcı bulunamadı";

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager, IEmailSender emailSender) : base(signInManager, userManager, roleManager)
        {
            _emailSender = emailSender;
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

            if (TempData["ReturnUrl"] != null && !string.IsNullOrEmpty(TempData["ReturnUrl"].ToString()))
            {
                return Redirect(TempData["ReturnUrl"].ToString());
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult FacebookLogin(string ReturnUrl)
        {
            string RedirectUrl = Url.Action("ExternalLogin", "Account", new { returnUrl = ReturnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Facebook", RedirectUrl);
            return new ChallengeResult("Facebook", properties);
        }
        public IActionResult GoogleLogin(string ReturnUrl)
        {
            string RedirectUrl = Url.Action("ExternalLogin", "Account", new { ReturnUrl = ReturnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", RedirectUrl);
            return new ChallengeResult("Google", properties);
        }

        public async Task<IActionResult> ExternalLogin(string returnUrl)
        {
            string facebookLoginErr = "Facebook ile login başarısız";
            ExternalLoginInfo externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync();
            if (externalLoginInfo == null)
            {
                ModelState.AddModelError("", facebookLoginErr);
                return RedirectToAction("Login");
            }
            else
            {
                var result = await _signInManager.ExternalLoginSignInAsync(externalLoginInfo.LoginProvider, externalLoginInfo.ProviderKey,
                    true);
                if (result.Succeeded)
                {
                    if (returnUrl != null) Redirect(returnUrl);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    AppUser user = new AppUser();
                    user.Email= externalLoginInfo.Principal.FindFirst(ClaimTypes.Email).Value;


                    string externalUserId = externalLoginInfo.Principal.FindFirst(ClaimTypes.NameIdentifier).Value;
                    if (externalLoginInfo.Principal.HasClaim(i => i.Type == ClaimTypes.Name) && externalUserId.Length > 5)
                    {
                        string userName = externalLoginInfo.Principal.FindFirst(ClaimTypes.Name).Value;
                        userName = userName.Replace(' ', '-').ToLower() + externalUserId.Substring(0, 5).ToString();
                        user.UserName = userName;
                    }

                    IdentityResult resultCreateUser = await _userManager.CreateAsync(user);
                    if (resultCreateUser.Succeeded)
                    {
                        IdentityResult loginResult = await _userManager.AddLoginAsync(user, externalLoginInfo);
                        if (loginResult.Succeeded)
                        {
                            await _signInManager.ExternalLoginSignInAsync(externalLoginInfo.LoginProvider,
                                externalLoginInfo.ProviderKey, true);
                            if (returnUrl != null) Redirect(returnUrl);
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
                ModelState.AddModelError("", facebookLoginErr);
                return RedirectToAction("Login");
            }

        }

        public IActionResult SignUp()
        {
            return View(new UserSignUpViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> SignUp(UserSignUpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = model.CreateUser();
            IdentityResult result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                result.Errors.ToList().ForEach(i =>
                {
                    ModelState.AddModelError("", i.Description);
                });
                return View(model);
            }

            return RedirectToAction("Login");
        }

        public IActionResult ForgetPassword()
        {
            return View(new ForgetPasswordViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel model)
        {
            if (!ModelState.IsValid) View(model);
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", userErrorMessage);
                return View();
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPasswordConfirm", "Account", new
            {
                user = user.Id,
                token = token

            }, HttpContext.Request.Scheme);
            await _emailSender.SendResetPasswordEmail("muammer.karaca@vantaworks.com", resetLink);
            //HelperClass.SendResetPasswordEmail(model.Email,resetLink);
            model.Success = true;

            return View(model);
        }

        public IActionResult ResetPasswordConfirm(string user, string token)
        {
            var model = new ResetPasswordConfirmModel() { UserId = user, Token = token };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ResetPasswordConfirm(ResetPasswordConfirmModel model)
        {
            if (!ModelState.IsValid) View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                ModelState.AddModelError("", userErrorMessage);
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.PasswordNew);
            if (!result.Succeeded)
            {
                result.Errors.ToList().ForEach(i =>
                {
                    ModelState.AddModelError("", i.Description);
                });
                return View(model);
            }

            await _userManager.UpdateSecurityStampAsync(user);
            model.Success = true;
            return View(model);
        }

        public IActionResult Logout()
        {
            _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
