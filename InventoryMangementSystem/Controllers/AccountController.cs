using InventoryMangementSystem.Models;
using InventoryMangementSystemEntities.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InventoryMangementSystem.Controllers
{
   
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Register()
        {
            ViewBag.Roles = TempData["Roles"];
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newuse = new AppUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    PhoneNumberConfirmed = true,
                    EmailConfirmed = true,
                    
                };
                var result = await _userManager.CreateAsync(newuse, model.Password);
                if (result.Succeeded)
                {
                    if (model.Roles != null)
                    {
                        var roles = await _userManager.GetRolesAsync(newuse);
                        await _userManager.RemoveFromRolesAsync(newuse, roles);
                        await _userManager.AddToRolesAsync(newuse, model.Roles);
                    }
                    return RedirectToAction("GetAllUsers", "Administration");
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, item.Description);
                    }
                }

            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? ReturnUrl)
        {
            ViewData["ReturnUrl"] = ReturnUrl;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? ReturnUrl)
        {
            if (ModelState.IsValid)
            { 
                var user =await _userManager.FindByNameAsync(model.UserName);
                if (user == null)
                {
                    user = await _userManager.FindByEmailAsync(model.UserName);
                    if(user == null)
                    {
                        ModelState.AddModelError(string.Empty, "User data incorrect");
                        return View(model);
                    }
                    
                }
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    if (string.IsNullOrEmpty(ReturnUrl))
                    {
                        
                        return RedirectToAction("Index", "DashBoard");
                    }
                   return Redirect(ReturnUrl);
                }
               ModelState.AddModelError(string.Empty, "User data incorrect");
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
        public IActionResult AccessDeniedNew()
        {
            return View();
        }
    }
}
