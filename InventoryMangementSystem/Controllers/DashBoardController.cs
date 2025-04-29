using InventoryMangementSystem.Intefaces;
using InventoryMangementSystemEntities.Models;
using InventoryMangementSystemEntities.ViewModels.DashBoard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryMangementSystem.Controllers
{

    [Authorize]
    public class DashBoardController : Controller
    {

        private IGenericRepository<Product> _ProductRepository;
        private IGenericRepository<Category> _CategoryRepository;
        private IGenericRepository<Supplier> _SupplierRepository;

        public DashBoardController(IGenericRepository<Supplier> supplierRepository, IGenericRepository<Product> ProductRepository, IGenericRepository<Category> CategoryRepository)
        {
            _SupplierRepository = supplierRepository;
            _ProductRepository = ProductRepository;
            _CategoryRepository = CategoryRepository;
        }


        // GET: DashBoardController
        public async Task<ActionResult> Index()
        {

            var lowStockProducts = await _ProductRepository.GetAllAsync(p => p.StockQuantity <= p.LowStockThreshold && p.StockQuantity > 0);
            var totalProducts = await _ProductRepository.GetAllAsync(inculdes: new[] { "category"});
            var totalSuppliers = await _SupplierRepository.GetAllAsync();
            var outOfStockProducts = await _ProductRepository.GetAllAsync(p => p.StockQuantity == 0);
            var recentlyAdded = await _ProductRepository.GetAllAsync(inculdes: new[] { "category" });
            recentlyAdded = recentlyAdded.OrderByDescending(p => p.CreatedDate).Take(4);


            var dashboardViewModel = new DashboardViewModel
            {

                ProductsStockLevels = totalProducts,
                LowStockProducts = lowStockProducts.Count(),
                TotalSuppliers = totalSuppliers.Count(),
                outOfStockProducts = outOfStockProducts.Count(),
                RecentlyAddedProducts = recentlyAdded

            };
            return View("DashBoard",dashboardViewModel);
        }
       


    }
}
