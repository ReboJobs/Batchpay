using Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using XeroApp.Models.BusinessModels;

namespace XeroApp.Controllers
{
	[Authorize]
	public class AccountController : Controller
	{
		private readonly UserManager<User> _userManager;
		private readonly SignInManager<User> _signInManager;

		public AccountController(
			UserManager<User> userManager,
			SignInManager<User> signInManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
		}

		[AllowAnonymous]
		public IActionResult Register()
		{
			return View();
		}

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(AuthViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userModel = new User
                {
                    UserName = model.Email,
                    Email = model.Email
                };

                var userInfo = await _userManager.FindByNameAsync(userModel.UserName);

                if (userInfo != null)
				{
                    ModelState.AddModelError("", "User already exists.");
                    return View(model);
                }
                else
				{
                    var result = await _userManager.CreateAsync(userModel, model.Password);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(userModel, false);
                        return RedirectToAction("Index", "Xero");
                    }
                }
            }
            ModelState.AddModelError("", "Invalid Register.");
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(AuthViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usr = await _userManager.FindByNameAsync(model.Email);
                
                if (usr == null)
				{
                    ModelState.AddModelError("", "Account does not exist.");
                    return View(model);
                }
                else
				{
                    await _signInManager.SignInAsync(usr, false);
                    return RedirectToAction("Index", "Xero");
                }
            }
            ModelState.AddModelError("", "Invalid Login.");
            return View(model);
        }

        public async Task<IActionResult> Logout()
		{
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
		}
    }
}