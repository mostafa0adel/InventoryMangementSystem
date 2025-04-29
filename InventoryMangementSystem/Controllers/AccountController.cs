using InventoryMangementSystem.Models;
using InventoryMangementSystemEntities.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InventoryMangementSystem.Controllers
{
    /// <summary>
    /// Controller for managing user account-related actions like registration, login, and logout.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="userManager">The user manager service.</param>
        /// <param name="signInManager">The sign-in manager service.</param>
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Displays the registration page.
        /// </summary>
        /// <returns>The registration view.</returns>
        public IActionResult Register()
        {
            ViewBag.Roles = TempData["Roles"];  //from adminstration controller
            return View();
        }

        /// <summary>
        /// Handles user registration.
        /// </summary>
        /// <param name="model">The registration view model.</param>
        /// <returns>A redirection to the list of all users if successful, otherwise the registration view with errors.</returns>
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

        /// <summary>
        /// Displays the login page.
        /// </summary>
        /// <param name="ReturnUrl">Optional URL to return to after login.</param>
        /// <returns>The login view.</returns>
        [HttpGet]
        public IActionResult Login(string? ReturnUrl)
        {
            ViewData["ReturnUrl"] = ReturnUrl;
            return View();
        }

        /// <summary>
        /// Handles user login.
        /// </summary>
        /// <param name="model">The login view model.</param>
        /// <param name="ReturnUrl">Optional URL to return to after login.</param>
        /// <returns>A redirection to the specified URL or the dashboard if login is successful, otherwise the login view with errors.</returns>
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? ReturnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                if (user == null)
                {
                    user = await _userManager.FindByEmailAsync(model.UserName);
                    if (user == null)
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

        /// <summary>
        /// Logs the user out.
        /// </summary>
        /// <returns>A redirection to the login page.</returns>
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// Displays the access denied page.
        /// </summary>
        /// <returns>The access denied view.</returns>
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Displays the new access denied page.
        /// </summary>
        /// <returns>The new access denied view.</returns>
        public IActionResult AccessDeniedNew()
        {
            return View();
        }
    }
}
