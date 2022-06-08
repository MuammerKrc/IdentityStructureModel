using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityStructureModel.IdentityModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityStructureModel.Controllers
{
    public class _BaseController:Controller
    {
        protected readonly UserManager<AppUser> _userManager;
        protected readonly SignInManager<AppUser> _signInManager;
        protected readonly RoleManager<AppRole> _roleManager;

        public _BaseController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        protected AppUser currentUser =>User.Identity.Name!=null? _userManager.FindByNameAsync(User.Identity.Name).Result:null;

    }
}
