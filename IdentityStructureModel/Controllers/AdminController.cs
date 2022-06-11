using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityStructureModel.IdentityModels;
using IdentityStructureModel.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace IdentityStructureModel.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : _BaseController
    {
        public AdminController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager) : base(signInManager, userManager, roleManager)
        {
        }
        public IActionResult Index()
        {
            return View();
        }
        [Authorize(policy: "CityPolicy")]
        public IActionResult Claims()
        {
            return View(User.Claims.ToList());
        }

        public IActionResult Users()
        {
            return View(_userManager.Users.ToList());
        }
        public IActionResult Roles(string err)
        {
            if (string.IsNullOrEmpty(err))
                ViewBag.err = err;
            return View(_roleManager.Roles.ToList());
        }
        public IActionResult RoleCreate()
        {
            return View(new RoleViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> RoleCreate(RoleViewModel model)
        {
            if (!ModelState.IsValid) View(model);
            var result = await _roleManager.CreateAsync(model.CreateRole());

            if (!result.Succeeded)
            {
                result.Errors.ToList().ForEach(i =>
                {
                    ModelState.AddModelError("", i.Description);

                });
                return View(model);
            }

            return RedirectToAction("Roles");
        }



        public async Task<IActionResult> RoleDelete(string id)
        {
            string roleNotFoundErr = "Böyle bir role bulunamadı";
            AppRole role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return RedirectToAction("Roles", new
                {
                    err = roleNotFoundErr
                });
            }
            IdentityResult result = _roleManager.DeleteAsync(role).Result;
            return RedirectToAction("Roles", new
            {
                err = result.Errors.ToList().FirstOrDefault()
            });
        }

        public async Task<IActionResult> RoleUpdate(string id)
        {
            string roleNotFoundErr = "Böyle bir role bulunamadı";
            AppRole role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                RedirectToAction("Roles", new
                {
                    err = roleNotFoundErr
                });

            return View(role.GetRoleViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> RoleUpdate(RoleViewModel model)
        {
            string roleNotFoundErr = "Böyle bir role bulunamadı";
            if (ModelState.IsValid) View(model);
            AppRole role = await _roleManager.FindByIdAsync(model.Id.ToString());
            if (role == null)
            {
                ModelState.AddModelError("", roleNotFoundErr);
                return View(model);
            }

            model.UpdateRole(role);
            IdentityResult result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                result.Errors.ToList().ForEach(i =>
                {
                    ModelState.AddModelError("", i.Description);
                });
                return View(model);
            }

            return RedirectToAction("Roles");
        }

        public async Task<IActionResult> RoleAssignToUser(string id)
        {
            string userNotFoundErr = "Böyle bir kullanıcı";
            TempData["userId"] = id;
            AppUser user = await _userManager.FindByIdAsync(id);

            List<AppRole> roles = _roleManager.Roles.ToList();
            List<string> userRoles = await _userManager.GetRolesAsync(user) as List<string>;

            List<RoleAssignViewModel> roleAssignViewModels = new List<RoleAssignViewModel>();
            foreach (var item in roles)
            {
                RoleAssignViewModel model = new RoleAssignViewModel();
                model.RoleId = item.Id;
                model.RoleName = item.Name;
                if (userRoles.Contains(item.Name))
                {
                    model.Exist = true;
                }
                else
                {
                    model.Exist = false;
                }
                roleAssignViewModels.Add(model);
            }
            return View(roleAssignViewModels);
        }
        [HttpPost]
        public async Task<IActionResult> RoleAssignToUser(List<RoleAssignViewModel> roleAssignViewModels)
        {
            AppUser user = _userManager.FindByIdAsync(TempData["userId"].ToString()).Result;

            foreach (var item in roleAssignViewModels)
            {
                if (item.Exist)

                {
                    await _userManager.AddToRoleAsync(user, item.RoleName);
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(user, item.RoleName);
                }
            }

            return RedirectToAction("Users");
        }

        public async Task<IActionResult> FreeDayDefine()
        {
            if (User != null && User.Identity.IsAuthenticated)
            {
                if (!User.HasClaim(i => i.Type == "freeDay"))
                {
                    
                    Claim claim = new Claim("freeDay", DateTime.Now.AddMinutes(2).ToLongTimeString(),
                        ClaimValueTypes.String, "custom");
                    var result=await _userManager.AddClaimAsync(currentUser, claim);
                    await _signInManager.SignOutAsync();
                    await _signInManager.SignInAsync(currentUser, true);
                }
            }

            return RedirectToAction("RestrictedPage");
        }

        [Authorize(policy:"FreeDayPolicy")]
        public async Task<IActionResult> RestrictedPage()
        {
            return View();
        }

    }
}
