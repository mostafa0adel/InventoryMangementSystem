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

        // GET: SuppliersController/Report
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

        // GET: SuppliersController
        public async Task<ActionResult> Index()
        {
            var suppliers = await _SupplierRepository.GetAllAsync();
            return View("SuppliersList", suppliers);
        }

        // GET: SuppliersController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var supplier = await _SupplierRepository.GetByIdAsync(id);
            return View(supplier);
        }

        // GET: SuppliersController/Create
        [Authorize(Roles = "Administrator")]

        public ActionResult Create()
        {
            return View("AddNewSupplier");
        }

        // POST: SuppliersController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Supplier item)
        {
            try
            {
                var Isexists = _SupplierRepository.GetAllAsync().Result.Any(s => s.SupplierName == item.SupplierName);
                if (Isexists == true)
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

        // GET: SuppliersController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var supplier = await _SupplierRepository.GetByIdAsync(id);
            return View(supplier);
        }



        // POST: SuppliersController/Edit/5
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


        // GET: CategoriesController/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            var supplier = await _SupplierRepository.GetByIdAsync(id);
            return View(supplier);
        }

        // POST: CategoriesController/Delete/5
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
