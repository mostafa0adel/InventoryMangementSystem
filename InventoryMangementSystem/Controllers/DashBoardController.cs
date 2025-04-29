using InventoryMangementSystem.Intefaces;
using InventoryMangementSystemEntities.Models;
using InventoryMangementSystemEntities.ViewModels.DashBoard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryMangementSystem.Controllers
{
    /// <summary>
    /// Controller responsible for displaying the dashboard with product and supplier statistics.
    /// </summary>
    [Authorize]
    public class DashBoardController : Controller
    {
        private readonly IGenericRepository<Product> _ProductRepository;
        private readonly IGenericRepository<Category> _CategoryRepository;
        private readonly IGenericRepository<Supplier> _SupplierRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashBoardController"/> class.
        /// </summary>
        /// <param name="supplierRepository">The repository for managing supplier data.</param>
        /// <param name="ProductRepository">The repository for managing product data.</param>
        /// <param name="CategoryRepository">The repository for managing category data.</param>
        public DashBoardController(IGenericRepository<Supplier> supplierRepository, IGenericRepository<Product> ProductRepository, IGenericRepository<Category> CategoryRepository)
        {
            _SupplierRepository = supplierRepository;
            _ProductRepository = ProductRepository;
            _CategoryRepository = CategoryRepository;
        }

        /// <summary>
        /// Displays the dashboard with various product and supplier statistics.
        /// </summary>
        /// <returns>The dashboard view populated with statistics.</returns>
        public async Task<ActionResult> Index()
        {
            // Fetch products with low stock levels.
            var lowStockProducts = await _ProductRepository.GetAllAsync(p => p.StockQuantity <= p.LowStockThreshold && p.StockQuantity > 0);

            // Fetch all products including their categories.
            var totalProducts = await _ProductRepository.GetAllAsync(inculdes: new[] { "category" });

            // Fetch all suppliers.
            var totalSuppliers = await _SupplierRepository.GetAllAsync();

            // Fetch all products that are out of stock.
            var outOfStockProducts = await _ProductRepository.GetAllAsync(p => p.StockQuantity == 0);

            // Fetch the most recently added products, including their categories, and limit to the 4 most recent ones.
            var recentlyAdded = await _ProductRepository.GetAllAsync(inculdes: new[] { "category" });
            recentlyAdded = recentlyAdded.OrderByDescending(p => p.CreatedDate).Take(4);

            // Create a dashboard view model to hold the statistics.
            var dashboardViewModel = new DashboardViewModel
            {
                ProductsStockLevels = totalProducts,
                LowStockProducts = lowStockProducts.Count(),
                TotalSuppliers = totalSuppliers.Count(),
                outOfStockProducts = outOfStockProducts.Count(),
                RecentlyAddedProducts = recentlyAdded
            };

            // Return the dashboard view with the view model.
            return View("DashBoard", dashboardViewModel);
        }
    }
}
