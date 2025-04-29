using InventoryMangementSystem.Intefaces;
using InventoryMangementSystemEntities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Core.Types;

namespace InventoryMangementSystem.Controllers
{
    [Authorize]
    public class SuppliersController : Controller
    {
        private IGenericRepository<Supplier> _SupplierRepository;
        private IGenericRepository<Product> _ProductRepository;
        private IGenericRepository<Category> _CategoryRepository;

        public SuppliersController(IGenericRepository<Supplier> SupplierRepository, IGenericRepository<Product> ProductRepository, IGenericRepository<Category> CategoryRepository)
        {
            _SupplierRepository = SupplierRepository;
            _ProductRepository = ProductRepository;
            _CategoryRepository = CategoryRepository;
        }

        /// <summary>
        /// Generates a report of suppliers and their products.
        /// Only accessible by users with the Administrator role.
        /// </summary>
        /// <returns>A view containing the supplier report.</returns>
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Report()
        {
            try
            {
                // Get all products and categories
                var products = await _ProductRepository.GetAllAsync();
                var categories = await _CategoryRepository.GetAllAsync();
                var suppliers = await _SupplierRepository.GetAllAsync();

                var result = new List<dynamic>(); // Using dynamic type for flexibility

                foreach (var supplier in suppliers)
                {
                    // Get products for the current supplier
                    var supplierProducts = products.Where(p => p.supplierId == supplier.Id).ToList();

                    // Create a list for product information
                    var productInfoList = supplierProducts.Select(p => new
                    {
                        ProductId = p.Id,
                        ProductName = p.ProductName,
                        ProductPrice = p.Price,
                        ProductImage = p.ProductImage,
                        QTY = p.StockQuantity,
                        CategoryName = categories.FirstOrDefault(c => c.Id == p.categoryId)?.CategoryName // Assuming CategoryId is in Product
                    }).ToList();

                    // Add supplier information with product info
                    result.Add(new
                    {
                        SupplierName = supplier.SupplierName,
                        Products = productInfoList
                    });
                }
                Console.WriteLine(result);
                // Now, you can return the supplier report view with the result
                return View("SupplierReport", result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View();
            }
        }

        /// <summary>
        /// Displays the list of suppliers.
        /// </summary>
        /// <returns>A view containing the list of suppliers.</returns>
        public async Task<ActionResult> Index()
        {
            var suppliers = await _SupplierRepository.GetAllAsync();
            return View("SuppliersList", suppliers);
        }

        /// <summary>
        /// Displays the details of a specific supplier.
        /// </summary>
        /// <param name="id">The ID of the supplier.</param>
        /// <returns>A view containing the supplier's details.</returns>
        public async Task<ActionResult> Details(int id)
        {
            var supplier = await _SupplierRepository.GetByIdAsync(id);
            return View(supplier);
        }

        /// <summary>
        /// Displays the create supplier form.
        /// Only accessible by users with the Administrator role.
        /// </summary>
        /// <returns>A view for creating a new supplier.</returns>
        [Authorize(Roles = "Administrator")]
        public ActionResult Create()
        {
            return View("AddNewSupplier");
        }

        /// <summary>
        /// Creates a new supplier.
        /// </summary>
        /// <param name="item">The supplier information.</param>
        /// <returns>A redirect to the index action if successful, otherwise displays the create form again.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Supplier item)
        {
            try
            {
                var Isexists = _SupplierRepository.GetAllAsync().Result.Any(s => s.SupplierName == item.SupplierName);
                if (Isexists)
                {
                    ViewBag.ExistsError = "Supplier Already exists";
                    return View("AddNewSupplier");
                }
                await _SupplierRepository.AddAsync(item);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View("AddNewSupplier");
            }
        }

        /// <summary>
        /// Displays the edit form for a specific supplier.
        /// </summary>
        /// <param name="id">The ID of the supplier.</param>
        /// <returns>A view for editing the supplier.</returns>
        public async Task<ActionResult> Edit(int id)
        {
            var supplier = await _SupplierRepository.GetByIdAsync(id);
            return View(supplier);
        }

        /// <summary>
        /// Updates a specific supplier's information.
        /// </summary>
        /// <param name="id">The ID of the supplier.</param>
        /// <param name="item">The updated supplier information.</param>
        /// <returns>A redirect to the index action if successful, otherwise displays the edit form again.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, Supplier item)
        {
            try
            {
                await _SupplierRepository.UpdateAsync(item);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        /// <summary>
        /// Displays the delete confirmation for a specific supplier.
        /// </summary>
        /// <param name="id">The ID of the supplier.</param>
        /// <returns>A view for confirming deletion of the supplier.</returns>
        public async Task<ActionResult> Delete(int id)
        {
            var supplier = await _SupplierRepository.GetByIdAsync(id);
            return View(supplier);
        }

        /// <summary>
        /// Deletes a specific supplier.
        /// </summary>
        /// <param name="id">The ID of the supplier.</param>
        /// <param name="collection">Form collection for additional parameters.</param>
        /// <returns>A redirect to the index action if successful, otherwise displays the delete confirmation again.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            try
            {
                await _SupplierRepository.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
