using InventoryMangementSystem.Intefaces;
using InventoryMangementSystemEntities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace InventoryMangementSystem.Controllers
{
    // must be logged in to access this controller
    [Authorize]
    public class ProductsController : Controller
    {
       private  IGenericRepository<Product> _ProductRepository;
        private IGenericRepository<Category> _CategoryRepository;
        private IGenericRepository<Supplier> _SupplierRepository;
        private IGenericRepository<StockLevel> _StockLevelRepository;
        private IWebHostEnvironment _environment;
        private IUploudFile _uploadFile;
        public ProductsController(IGenericRepository<Product> ProductRepository, IGenericRepository<Category> categoryRepository, IWebHostEnvironment environment, IUploudFile uploadFile, IGenericRepository<Supplier> supplierRepository, IGenericRepository<StockLevel> stockLevelRepository)
        {
            _ProductRepository = ProductRepository;
            _CategoryRepository = categoryRepository;
            _environment = environment;
            _uploadFile = uploadFile;
            _SupplierRepository = supplierRepository;
            _StockLevelRepository = stockLevelRepository;
        }

        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Report()
        {
            var products = await _ProductRepository.GetAllAsync(inculdes: new[] { "category", "supplier" });
            var stockLevels = await _StockLevelRepository.GetAllAsync();

            var quantityChanges = stockLevels
                .Where(sl => sl.QuantityChange < 0 && sl.ChangeType == "remove")
                .ToDictionary(sl => sl.ProductId, sl => sl.QuantityChange);
            var result = new List<dynamic>(); // Using dynamic type for flexibility

            var removedProducts = products
                .Where(p => quantityChanges.ContainsKey(p.Id))
                .Select(p => new
                {
                    Product = p,
                    QuantityChanged = quantityChanges[p.Id]
                }).ToList();

            removedProducts.ForEach(p => result.Add(p));
            
            ViewData["filter"] = "All Products";
            return View("PurchaseReport", result);

        }

        // GET: ProductsController
        public async Task<ActionResult> Index()
        {
            var products = await _ProductRepository.GetAllAsync(inculdes: new[] { "category", "supplier" });
            ViewData["filter"] = "All Products";
            return View("ProductsList", products);
        }
        public async Task<ActionResult> Filter(string filter)
        {
            IEnumerable<Product> products = new List<Product>();
            if (string.IsNullOrEmpty(filter))
            {
                products = await _ProductRepository.GetAllAsync(inculdes: new[] { "category", "supplier" });
            }
            else
            {
                ViewBag.filter = filter;
                
                    if (filter == "low")
                    {
                    products = await _ProductRepository.GetAllAsync(p => p.StockQuantity <= p.LowStockThreshold && p.StockQuantity >0, inculdes: new[] { "category", "supplier" });
                    }
                    else if (filter == "out")
                    {
                    products = await _ProductRepository.GetAllAsync(p => p.StockQuantity == 0, inculdes: new[] { "category", "supplier" });
                    }
                else
                {
                    products = await _ProductRepository.GetAllAsync(inculdes: new[] { "category", "supplier" });
                }
             
            }
           return PartialView("_ProductsCards",products);
        }


        // GET: ProductsController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var product = await _ProductRepository.GetByIdAsync(id);
            product.category = await _CategoryRepository.GetByIdAsync(product.categoryId);
            product.supplier = await _SupplierRepository.GetByIdAsync(product.supplierId);
            return View(product);
        }

        // GET: ProductsController/Create
        public async Task<ActionResult> Create()
        {
            var categories = await _CategoryRepository.GetAllAsync();
            ViewBag.categoryList = categories;
            ViewBag.supplierList = await _SupplierRepository.GetAllAsync();
            return View();
        }

        // POST: ProductsController/Create
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

        // GET: ProductsController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var categories = await _CategoryRepository.GetAllAsync();
            var product = await _ProductRepository.GetByIdAsync(id);
            ViewBag.categoryList = categories;
            ViewBag.supplierList = await _SupplierRepository.GetAllAsync();
            return View(product);
        }

        // POST: ProductsController/Edit/5
        [HttpPost]
        public async Task<ActionResult> Edit(int id, Product item,int oldQTY)
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
                if(stockChange != 0)
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

        // GET: ProductsController/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            var product = await _ProductRepository.GetByIdAsync(id);
            product.category = await _CategoryRepository.GetByIdAsync(product.categoryId);
            product.supplier = await _SupplierRepository.GetByIdAsync(product.supplierId);
            return View(product);
        }

        // POST: ProductsController/Delete/5
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

        // low stock items

        public async Task<ActionResult> LowStock()
        {
            var products = await _ProductRepository.GetAllAsync(p => p.StockQuantity <= p.LowStockThreshold && p.StockQuantity > 0, inculdes: new[] { "category", "supplier" });
            ViewData["filter"] = "low stock products";
            return View("ProductsList", products);
        }
        public async Task<ActionResult> OutOfStock()
        {
            var products = await _ProductRepository.GetAllAsync(p => p.StockQuantity == 0, inculdes: new[] { "category", "supplier" });
            ViewData["filter"] = "out of stock products";
            return View("ProductsList", products);
        }
        //for update the product stock (add or remove)
        public async Task<ActionResult> UpdateStock()
        {
            ViewBag.ProductList = await _ProductRepository.GetAllAsync();
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> UpdateStock(int id, int quantity, string type)
        {
            try
            {
                var product = await _ProductRepository.GetByIdAsync(id);
                product.StockQuantity += type == "add" ? quantity : -quantity;
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
