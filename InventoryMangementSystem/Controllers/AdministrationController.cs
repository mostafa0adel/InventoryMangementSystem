using InventoryMangementSystem.Models;
using InventoryMangementSystemEntities.ViewModels.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryMangementSystem.Controllers
{
    /// <summary>
    /// Controller for managing roles and users in the inventory management system.
    /// </summary>
    [Authorize(Roles = "Administrator")]
    public class AdministrationController : Controller
    {
        private RoleManager<IdentityRole> _roleManager;
        private UserManager<AppUser> _userManager;
        private IHttpContextAccessor _contextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdministrationController"/> class.
        /// </summary>
        /// <param name="roleManager">Role manager for managing roles.</param>
        /// <param name="userManager">User manager for managing users.</param>
        /// <param name="contextAccessor">HTTP context accessor for accessing the current user context.</param>
        public AdministrationController(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager, IHttpContextAccessor contextAccessor)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _contextAccessor = contextAccessor;
        }

        #region Roles
        /// <summary>
        /// Retrieves all roles and returns a view to display them.(map roles in _roleManager.Roles to RolesViewModel)
        /// </summary>
        /// <returns>A view that displays all roles.</returns>
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleManager.Roles.Select(r => new RolesViewModel()
            {
                RoleName = r.Name,
                Id = r.Id,
            }).ToListAsync();

            return View(roles);
        }

        /// <summary>
        /// Returns a view to create a new role.
        /// </summary>
        /// <returns>A view for creating a new role.</returns>
        public IActionResult NewRole()
        {
            var role = new RolesViewModel() { Id = Guid.NewGuid().ToString() };
            return View(role);
        }

        /// <summary>
        /// Creates a new role in the system.
        /// </summary>
        /// <param name="model">The role view model.</param>
        /// <returns>A redirection to the list of all roles, or the same view if the creation fails.</returns>
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
        /// <summary>
        /// Retrieves all users and their roles, and returns a view to display them.(map to user view model)
        /// </summary>
        /// <returns>A view that displays all users and their roles.</returns>
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

        /// <summary>
        /// Redirects to the user registration page.
        /// </summary>
        /// <returns>A redirection to the user registration page.</returns>
        public async Task<IActionResult> NewUser()
        {
            TempData["Roles"] = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return RedirectToAction("Register", "Account");
        }

        /// <summary>
        /// Returns a view for managing the roles of a specific user.
        /// </summary>
        /// <param name="userID">The ID of the user.</param>
        /// <returns>A view for managing the roles of the specified user.</returns>
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

        /// <summary>
        /// Updates the roles of a user.by deleting all user roles and reassign selected roles to the user
        /// </summary>
        /// <param name="model">The user roles view model.</param>
        /// <returns>A redirection to the list of all users.</returns>
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

        /// <summary>
        /// Deletes a user from the system.
        /// </summary>
        /// <param name="userID">The ID of the user to delete.</param>
        /// <returns>A redirection to the list of all users.</returns>
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

        /// <summary>
        /// Returns a view for changing the current user's password.
        /// </summary>
        /// <returns>A view for changing the current user's password.</returns>
        [AllowAnonymous]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// Changes the current user's password.
        /// </summary>
        /// <param name="model">The change password view model.</param>
        /// <returns>A redirection to the dashboard or the same view if the change fails.</returns>
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
