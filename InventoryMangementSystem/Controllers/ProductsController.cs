using InventoryMangementSystem.Intefaces;
using InventoryMangementSystemEntities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryMangementSystem.Controllers
{
    // Requires the user to be logged in to access this controller
    [Authorize]
    public class ProductsController : Controller
    {
        private IGenericRepository<Product> _ProductRepository;
        private IGenericRepository<Category> _CategoryRepository;
        private IGenericRepository<Supplier> _SupplierRepository;
        private IGenericRepository<StockLevel> _StockLevelRepository;
        private IWebHostEnvironment _environment;
        private IUploudFile _uploadFile;

        public ProductsController(
            IGenericRepository<Product> ProductRepository,
            IGenericRepository<Category> categoryRepository,
            IWebHostEnvironment environment,
            IUploudFile uploadFile,
            IGenericRepository<Supplier> supplierRepository,
            IGenericRepository<StockLevel> stockLevelRepository)
        {
            _ProductRepository = ProductRepository;
            _CategoryRepository = categoryRepository;
            _environment = environment;
            _uploadFile = uploadFile;
            _SupplierRepository = supplierRepository;
            _StockLevelRepository = stockLevelRepository;
        }

        /// <summary>
        /// Generates a report of removed products along with their quantity changes.
        /// Only accessible by users with the Administrator role.
        /// </summary>
        /// <returns>A view containing the purchase report.</returns>
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Report()
        {
            var products = await _ProductRepository.GetAllAsync(inculdes: new[] { "category", "supplier" });
            var stockLevels = await _StockLevelRepository.GetAllAsync();

            var quantityChanges = stockLevels
                .Where(sl => sl.QuantityChange < 0 && sl.ChangeType == "remove")
                .ToDictionary(sl => sl.ProductId, sl => sl.QuantityChange);

            var removedProducts = products
                .Where(p => quantityChanges.ContainsKey(p.Id))
                .Select(p => new
                {
                    Product = p,
                    QuantityChanged = quantityChanges[p.Id]
                }).ToList();

            ViewData["filter"] = "All Products";
            return View("PurchaseReport", removedProducts);
        }

        /// <summary>
        /// Displays the index view of products.
        /// </summary>
        /// <returns>A view containing a list of all products.</returns>
        public async Task<ActionResult> Index()
        {
            var products = await _ProductRepository.GetAllAsync(inculdes: new[] { "category", "supplier" });
            ViewData["filter"] = "All Products";
            return View("ProductsList", products);
        }

        /// <summary>
        /// Filters products based on the specified criteria.
        /// </summary>
        /// <param name="filter">The filter criteria.</param>
        /// <returns>A partial view containing the filtered products.</returns>
        public async Task<ActionResult> Filter(string filter)
        {
            IEnumerable<Product> products;
            if (string.IsNullOrEmpty(filter))
            {
                products = await _ProductRepository.GetAllAsync(inculdes: new[] { "category", "supplier" });
            }
            else
            {
                ViewBag.filter = filter;

                if (filter == "low")
                {
                    products = await _ProductRepository.GetAllAsync(
                        p => p.StockQuantity <= p.LowStockThreshold && p.StockQuantity > 0,
                        inculdes: new[] { "category", "supplier" });
                }
                else if (filter == "out")
                {
                    products = await _ProductRepository.GetAllAsync(
                        p => p.StockQuantity == 0,
                        inculdes: new[] { "category", "supplier" });
                }
                else
                {
                    products = await _ProductRepository.GetAllAsync(inculdes: new[] { "category", "supplier" });
                }
            }
            return PartialView("_ProductsCards", products);
        }

        /// <summary>
        /// Displays the details of a specific product.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <returns>A view containing the product details.</returns>
        public async Task<ActionResult> Details(int id)
        {
            var product = await _ProductRepository.GetByIdAsync(id);
            product.category = await _CategoryRepository.GetByIdAsync(product.categoryId);
            product.supplier = await _SupplierRepository.GetByIdAsync(product.supplierId);
            return View(product);
        }

        /// <summary>
        /// Displays the form for creating a new product.
        /// </summary>
        /// <returns>A view for creating a new product.</returns>
        public async Task<ActionResult> Create()
        {
            ViewBag.categoryList = await _CategoryRepository.GetAllAsync();
            ViewBag.supplierList = await _SupplierRepository.GetAllAsync();
            return View();
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="item">The product data.</param>
        /// <returns>A redirect to the index action if successful, otherwise displays the create form again.</returns>
        [HttpPost]
        public async Task<ActionResult> Create(Product item)
        {
            try
            {
                if (item.ImageFile != null)
                {
                    string FilePath = await _uploadFile.UploadFileAsync("\\img\\product\\", item.ImageFile);
                    item.ProductImage = FilePath;
                }
                item.CreatedBy = User.Identity.Name;
                item.CreatedDate = DateTime.Now;
                await _ProductRepository.AddAsync(item);

                StockLevel stockLevel = new StockLevel
                {
                    ProductId = item.Id,
                    QuantityChange = item.StockQuantity,
                    ChangeDate = DateTime.Now,
                    ChangeType = "Initial Stock"
                };
                await _StockLevelRepository.AddAsync(stockLevel);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        /// <summary>
        /// Displays the form for editing a specific product.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <returns>A view for editing the product.</returns>
        public async Task<ActionResult> Edit(int id)
        {
            var categories = await _CategoryRepository.GetAllAsync();
            var product = await _ProductRepository.GetByIdAsync(id);
            ViewBag.categoryList = categories;
            ViewBag.supplierList = await _SupplierRepository.GetAllAsync();
            return View(product);
        }

        /// <summary>
        /// Updates the information of a specific product.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <param name="item">The updated product data.</param>
        /// <param name="oldQTY">The old quantity of the product.</param>
        /// <returns>A redirect to the index action if successful, otherwise displays the edit form again.</returns>
        [HttpPost]
        public async Task<ActionResult> Edit(int id, Product item, int oldQTY)
        {
            try
            {
                if (item.ImageFile != null)
                {
                    string FilePath = await _uploadFile.UploadFileAsync("\\img\\product\\", item.ImageFile);
                    item.ProductImage = FilePath;
                }
                await _ProductRepository.UpdateAsync(item);

                int stockChange = item.StockQuantity - oldQTY;
                if (stockChange != 0)
                {
                    StockLevel stockLevel = new StockLevel
                    {
                        ProductId = item.Id,
                        QuantityChange = stockChange,
                        ChangeDate = DateTime.Now,
                        ChangeType = stockChange > 0 ? "add" : "remove"
                    };
                    await _StockLevelRepository.AddAsync(stockLevel);
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        /// <summary>
        /// Displays the delete confirmation for a specific product.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <returns>A view for confirming deletion of the product.</returns>
        public async Task<ActionResult> Delete(int id)
        {
            var product = await _ProductRepository.GetByIdAsync(id);
            product.category = await _CategoryRepository.GetByIdAsync(product.categoryId);
            product.supplier = await _SupplierRepository.GetByIdAsync(product.supplierId);
            return View(product);
        }

        /// <summary>
        /// Deletes a specific product.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <param name="item">The product data.</param>
        /// <returns>A redirect to the index action if successful, otherwise displays the delete confirmation again.</returns>
        [HttpPost]
        public async Task<ActionResult> Delete(int id, Product item)
        {
            try
            {
                await _ProductRepository.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        /// <summary>
        /// Displays low stock products.
        /// </summary>
        /// <returns>A view containing a list of low stock products.</returns>
        public async Task<ActionResult> LowStock()
        {
            var products = await _ProductRepository.GetAllAsync(
                p => p.StockQuantity <= p.LowStockThreshold && p.StockQuantity > 0,
                inculdes: new[] { "category", "supplier" });
            ViewData["filter"] = "low stock products";
            return View("ProductsList", products);
        }

        /// <summary>
        /// Displays out-of-stock products.
        /// </summary>
        /// <returns>A view containing a list of out-of-stock products.</returns>
        public async Task<ActionResult> OutOfStock()
        {
            var products = await _ProductRepository.GetAllAsync(
                p => p.StockQuantity == 0,
                inculdes: new[] { "category", "supplier" });
            ViewData["filter"] = "out of stock products";
            return View("ProductsList", products);
        }

        /// <summary>
        /// Displays the stock update form for products.
        /// </summary>
        /// <returns>A view for updating product stock.</returns>
        public async Task<ActionResult> UpdateStock()
        {
            ViewBag.ProductList = await _ProductRepository.GetAllAsync();
            return View();
        }

        /// <summary>
        /// Updates the stock of a specific product.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <param name="quantity">The quantity to add or remove.</param>
        /// <param name="type">The type of stock change (add or remove).</param>
        /// <returns>A redirect to the index action if successful, otherwise displays the stock update form again.</returns>
        [HttpPost]
        public async Task<ActionResult> UpdateStock(int id, int quantity, string type)
        {
            try
            {
                var product = await _ProductRepository.GetByIdAsync(id);
                int newQuantity = type == "add" ? product.StockQuantity + quantity : product.StockQuantity - quantity;

                if (newQuantity < 0)
                {
                    // Handle the case where the stock quantity would fall below zero
                    ModelState.AddModelError("", "Stock quantity cannot be less than zero.");
                    ViewBag.ProductList = await _ProductRepository.GetAllAsync();
                    return View();
                }

                product.StockQuantity = newQuantity;

                StockLevel stockLevel = new StockLevel
                {
                    ProductId = id,
                    QuantityChange = type == "add" ? quantity : -quantity,
                    ChangeDate = DateTime.Now,
                    ChangeType = type
                };

                await _StockLevelRepository.AddAsync(stockLevel);
                await _ProductRepository.UpdateAsync(product);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}


