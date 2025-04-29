using InventoryMangementSystem.Models;
using InventoryMangementSystemEntities.ViewModels.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryMangementSystem.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdministrationController : Controller
    {
        private RoleManager<IdentityRole> _roleManager;
        private UserManager<AppUser> _userManager;
        private IHttpContextAccessor _contextAccessor;

        public AdministrationController(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager, IHttpContextAccessor contextAccessor)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _contextAccessor = contextAccessor;
        }
        #region Roles
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleManager.Roles.Select(r => new RolesViewModel()
            {
                RoleName = r.Name,
                Id = r.Id,
            }).ToListAsync();
            
            return View(roles);
        }
        public IActionResult NewRole()
        {
            var role = new RolesViewModel() { Id = Guid.NewGuid().ToString() };
            return View(role);
        }
        [HttpPost]
        public async Task<IActionResult> NewRole(RolesViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = new IdentityRole()
                {
                    Id = model.Id,
                    Name = model.RoleName
                };
                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("GetAllRoles");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }


        #endregion
        #region Users
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.Select(u => new UserViewModel()
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                UserRoles = _userManager.GetRolesAsync(u).Result
            }).ToListAsync();
            return View(users);
        }
        
        public async Task<IActionResult> NewUser()
        {
           
            TempData["Roles"] = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return RedirectToAction("Register", "Account");
        }
        public async Task<IActionResult> ManageRoles(string userID)
        {
            var user = await _userManager.FindByIdAsync(userID);
            if (user == null)
            {
                return RedirectToAction("GetAllUsers");
            }
            var userRoles = await _roleManager.Roles.ToListAsync();
            var model = new UserRolesViewModel()
            {
                UserId = user.Id,
                UserName = user.UserName,
                UserEmail = user.Email,
				UserPhoneNumber = user.PhoneNumber,
				UserRoles = userRoles.Select(r => new SelectedRoles()
                {
                    RoleName = r.Name,
                    IsSelected = _userManager.IsInRoleAsync(user, r.Name).Result
                }).ToList()
            };

            return View(model);
        }
		[HttpPost]
        public async Task<IActionResult> UpdateRole(UserRolesViewModel model)
		{
			var user = await _userManager.FindByIdAsync(model.UserId);
			if (user == null)
			{
				return RedirectToAction("GetAllUsers");
			}
			var userRoles = await _userManager.GetRolesAsync(user);
			await _userManager.RemoveFromRolesAsync(user, userRoles);
			await _userManager.AddToRolesAsync(user, model.UserRoles.Where(r => r.IsSelected == true).Select(r => r.RoleName));
			return RedirectToAction("GetAllUsers");
		}
		//delete user
		public async Task<IActionResult> DeleteUser(string userID)
		{
			var user = await _userManager.FindByIdAsync(userID);
			if (user == null)
			{
				return RedirectToAction("GetAllUsers");
			}
			var result = await _userManager.DeleteAsync(user);
			if (result.Succeeded)
			{
				return RedirectToAction("GetAllUsers");
			}
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError("", error.Description);
			}
			return RedirectToAction("GetAllUsers");
		}
        [AllowAnonymous]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [AllowAnonymous]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                string userName = _contextAccessor.HttpContext.User.Identity.Name;
                var user = await _userManager.FindByNameAsync(userName);
                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "DashBoard");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }




        #endregion
    }
}
