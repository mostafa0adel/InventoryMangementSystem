using InventoryMangementSystem.Intefaces;
using InventoryMangementSystemEntities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryMangementSystem.Controllers
{
    [Authorize]

    public class StockLevelsController : Controller
    {
        private IGenericRepository<StockLevel> _stockLevelRepository;
        private IGenericRepository<Product> _ProductRepository;


        public StockLevelsController(IGenericRepository<StockLevel> stockLevelRepository, IGenericRepository<Product> ProductRepository)
        {
            _stockLevelRepository = stockLevelRepository;
            _ProductRepository = ProductRepository;
        }

        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Report()
        {
            var products = await _ProductRepository.GetAllAsync(inculdes: new[] { "category", "supplier" });
            var stockLevels = await _stockLevelRepository.GetAllAsync();

            // Create a dictionary for quick access to product details
            var productDictionary = products.ToDictionary(p => p.Id);

            // Create a list to hold the combined results
            var result = stockLevels
                .Select(sl => new
                {
                    StockLevel = sl,
                    ProductName = productDictionary.ContainsKey(sl.ProductId) ? productDictionary[sl.ProductId].ProductName : null,
                    ProductImage = productDictionary.ContainsKey(sl.ProductId) ? productDictionary[sl.ProductId].ProductImage : null
                }).ToList();

            ViewData["filter"] = "All Products";
            return View("StockReport", result);

        }

        // GET: StockLevelsController
        public ActionResult Index()
        {
            return View();
        }

        // GET: StockLevelsController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: StockLevelsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: StockLevelsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: StockLevelsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: StockLevelsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: StockLevelsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: StockLevelsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
