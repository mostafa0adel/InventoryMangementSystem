using InventoryMangementSystem.Intefaces;
using InventoryMangementSystemEntities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryMangementSystem.Controllers
{
    /// <summary>
    /// Controller for managing product categories.
    /// </summary>
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly IGenericRepository<Category> _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoriesController"/> class.
        /// </summary>
        /// <param name="repository">The generic repository for categories.</param>
        public CategoriesController(IGenericRepository<Category> repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Displays a list of all categories.
        /// </summary>
        /// <returns>The view with a list of categories.</returns>
        public async Task<ActionResult> Index()
        {
            var categories = await _repository.GetAllAsync();
            return View("CategoriesList", categories);
        }

        /// <summary>
        /// Displays details of a specific category.
        /// </summary>
        /// <param name="id">The ID of the category to display.</param>
        /// <returns>The view with category details.</returns>
        public async Task<ActionResult> Details(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            return View(category);
        }

        /// <summary>
        /// Displays the form to create a new category.
        /// </summary>
        /// <returns>The create category view.</returns>
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Handles the creation of a new category.
        /// </summary>
        /// <param name="item">The category object to be created.</param>
        /// <returns>Redirects to the index view if successful, otherwise returns to the create view with errors.</returns>
        [HttpPost]
        public async Task<ActionResult> Create(Category item)
        {
            try
            {
                var isExists = _repository.GetAllAsync().Result.Any(c => c.CategoryName == item.CategoryName);
                if (isExists)
                {
                    ViewBag.ExistsError = "Category Already exists";
                    return View();
                }
                item.CreatedBy = User.Identity.Name;
                item.CreatedDate = DateTime.Now;
                await _repository.AddAsync(item);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        /// <summary>
        /// Displays the form to edit an existing category.
        /// </summary>
        /// <param name="id">The ID of the category to edit.</param>
        /// <returns>The edit category view.</returns>
        public async Task<ActionResult> Edit(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            return View(category);
        }

        /// <summary>
        /// Handles the update of an existing category.
        /// </summary>
        /// <param name="id">The ID of the category to update.</param>
        /// <param name="item">The updated category object.</param>
        /// <returns>Redirects to the index view if successful, otherwise returns to the edit view with errors.</returns>
        [HttpPost]
        public async Task<ActionResult> Edit(int id, Category item)
        {
            try
            {
                await _repository.UpdateAsync(item);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        /// <summary>
        /// Displays the confirmation view to delete a category.
        /// </summary>
        /// <param name="id">The ID of the category to delete.</param>
        /// <returns>The delete confirmation view.</returns>
        public async Task<ActionResult> Delete(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            return View(category);
        }

        /// <summary>
        /// Handles the deletion of a category.
        /// </summary>
        /// <param name="id">The ID of the category to delete.</param>
        /// <param name="collection">Form collection for potential additional data (not used here).</param>
        /// <returns>Redirects to the index view if successful, otherwise returns to the delete view with errors.</returns>
        [HttpPost]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            try
            {
                await _repository.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
