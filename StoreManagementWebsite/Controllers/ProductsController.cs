using Microsoft.AspNetCore.Mvc;
using StoreManagementWebsite.Models;
using System.Threading.Channels;

namespace StoreManagementWebsite.Controllers
{
    public class ProductsController : Controller
    {
        private StoreManagementPlatformContext _context;
        private readonly ILogger<ProductsController> _logger;
        public ProductsController(ILogger<ProductsController> logger, StoreManagementPlatformContext context)
        {
            _logger = logger;
            _context = context;

        }

        public IActionResult Products() {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("StoreAccountNumber"))) {
                return RedirectToAction("Index", "Home");
            }
            var store = _context.Stores.Where(s => s.StoreAccountNumber == HttpContext.Session.GetString("StoreAccountNumber")).FirstOrDefault();

            if (store == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["StoreName"] = store.StoreName;

            ViewData["productsWithCategories"] = GetProductsWithCategories(store.StoreId);
            ViewData["categoriesInUse"] = GetCategoriesInUse(store.StoreId);

            var temp = GetProductsWithCategories(store.StoreId);

            return View();
        }
        #region 商品分類管理(編輯、刪除、新增)
        [HttpPost]
        public IActionResult ProductCategoriesEdit(IEnumerable<ProductCategory> clientProductCategories)
        {
            // 取得StoreId
            var store = _context.Stores.Where(s => s.StoreAccountNumber == HttpContext.Session.GetString("StoreAccountNumber")).FirstOrDefault();
            if (store == null)
            {
                return RedirectToAction("Index", "Home");
            }
            int _storeId = store.StoreId;

            var productCategory = _context.ProductCategories.Where(c => c.StoreId == _storeId && c.CategoryOnDelete == false).ToList();

            foreach (var clientProductCategory in clientProductCategories)
            {
                var category = productCategory.Where(c => c.CategoryId == clientProductCategory.CategoryId).FirstOrDefault();
                //Console.WriteLine("----------------------------------------------------------------------------------------------------------------------------------------------------------");
                //Console.WriteLine($"CategoryName: {category.CategoryName}, CategoryId: {category.CategoryId}, CategoryOnDelete: {category.CategoryOnDelete}");
                //Console.WriteLine($"ClientCategoryName: { clientProductCategory.CategoryName}, ClientCategoryId: { clientProductCategory.CategoryId}");
                //Console.WriteLine("----------------------------------------------------------------------------------------------------------------------------------------------------------");

                if (category != null)
                {
                    category.CategoryName = clientProductCategory.CategoryName;
                }
                else
                {
                    var errorViewModel = new ErrorViewModel
                    {
                        ErrorMessage = $"資料庫無對應{clientProductCategory.CategoryName}的類別ID",
                    };
                    return View("Error", errorViewModel);

                }
            }

            _context.SaveChanges();

            return RedirectToAction("Products", "Products");
        }


        [HttpPost]
        public IActionResult ProductCategoriesDelete(int categoryId)
        {
            // 取得StoreId
            var store = _context.Stores.Where(s => s.StoreAccountNumber == HttpContext.Session.GetString("StoreAccountNumber")).FirstOrDefault();
            if (store == null)
            {
                return RedirectToAction("Index", "Home");
            }
            int _storeId = store.StoreId;

            var productCategory = _context.ProductCategories.Where(c => c.StoreId == _storeId && c.CategoryId == categoryId).ToList();

            if (productCategory.Count == 0)
            {
                return Json(new { success = false, message = $"資料庫無對應{categoryId}的類別ID" });
            }

            var productsOnSellUnderCategory = _context.Products
                            .Join(_context.ProductCategories, p => p.CategoryId, c => c.CategoryId, (p, c) => new {
                                StoreId = p.StoreId,
                                ProductId = p.ProductId,
                                ProductName = p.ProductName,
                                ProductOnSell = p.ProductOnSell,
                                CategoryId = c.CategoryId,
                                CategoryOnDelete = c.CategoryOnDelete
                            })
                            .Where(p => p.StoreId == _storeId && p.CategoryId == categoryId && p.ProductOnSell == true)
                            .ToList();
            if (productsOnSellUnderCategory.Count > 0)
            {

                // 羅列出所有在售的商品
                List<string> tempProductNames = new List<string>();
                foreach (var product in productsOnSellUnderCategory)
                {
                    tempProductNames.Add(product.ProductName);
                    Console.WriteLine(product.ProductName);
                }

                var errorViewModel = new ErrorViewModel { ErrorMessage = $"{productCategory.First().CategoryName}下有商品在售，無法刪除",
                    ErrorResolution = "請先將該類別下的商品全部下架，才能刪除類別。",
                    ErrorItems = tempProductNames };
                return View("Error", errorViewModel);
            }

            productCategory.First().CategoryOnDelete = true;

            _context.SaveChanges();

            return RedirectToAction("Products", "Products");
        }

        [HttpPost]
        public IActionResult ProductCategoriesCreate(string CategoryNCreate)
        {
            // 取得StoreId
            var store = _context.Stores.Where(s => s.StoreAccountNumber == HttpContext.Session.GetString("StoreAccountNumber")).FirstOrDefault();
            if (store == null)
            {
                return RedirectToAction("Index", "Home");
            }
            int _storeId = store.StoreId;

            var productCategory = new ProductCategory
            {
                StoreId = _storeId,
                CategoryName = CategoryNCreate,
                CategoryOnDelete = false
            };

            _context.ProductCategories.Add(productCategory);
            _context.SaveChanges();

            return RedirectToAction("Products", "Products");
        }
        #endregion

        #region 商品管理(調整上架狀態、編輯、新增)
        [HttpPost]
        public IActionResult ProductsOnSellSwitch(int ProductId, bool ProductOnSell)
        {
            // 取得StoreId
            var store = _context.Stores.Where(s => s.StoreAccountNumber == HttpContext.Session.GetString("StoreAccountNumber")).FirstOrDefault();
            if (store == null)
            {
                return RedirectToAction("Index", "Home");
            }
            int _storeId = store.StoreId;
            var product = _context.Products.Where(p => p.StoreId == _storeId && p.ProductId == ProductId).FirstOrDefault();
            Console.WriteLine("----------------------------------------------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine($"ProductOnSell: {ProductOnSell}, ProductId: {ProductId}");
            Console.WriteLine(_storeId + "," + product == null);
            Console.WriteLine("----------------------------------------------------------------------------------------------------------------------------------------------------------");


            if (product == null)
            {
                return Json(new { success = false, message = $"資料庫無對應{ProductId}的商品ID" });
            }

            product.ProductOnSell = ProductOnSell;

            _context.SaveChanges();

            return Json(new { success = true, message = $"{product.ProductName} 商品已更新為 {(product.ProductOnSell ? "上架" : "下架")}"});
        }

        //public class ProductsOnSellSwitchRequest
        //{
        //    public int ProductId { get; set; }
        //    public bool ProductOnSell { get; set; }
        //}



        [Route("Products/Edit/{ProductId}")]
        public IActionResult Edit(int ProductId)
        {
            // 取得StoreId
            var store = _context.Stores.Where(s => s.StoreAccountNumber == HttpContext.Session.GetString("StoreAccountNumber")).FirstOrDefault();
            if (store == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var productsWithCategories = GetProductsWithCategories(store.StoreId);
            var product = productsWithCategories.Where(cp => cp.ProductId == ProductId).FirstOrDefault();

            if (product == null)
            {
                RedirectToAction("Products", "Products");
            }
            ViewData["specificProduct"] = product;
            ViewData["categoriesInUse"] = GetCategoriesInUse(store.StoreId);

            return View("Edit");

          
        }


        [HttpPost]
        [Route("Products/Edit/{ProductId}")]
        public IActionResult Edit(int ProductId, string ProductName, int CategoryId, decimal ProductUnitPrice, short ProductUnitsInStock, IFormFile? ProductImage)
        {
            // 取得StoreId
            var store = _context.Stores.Where(s => s.StoreAccountNumber == HttpContext.Session.GetString("StoreAccountNumber")).FirstOrDefault();
            if (store == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var product = _context.Products.Where(p => p.StoreId == store.StoreId && p.ProductId == ProductId).FirstOrDefault();

            if (product == null)
            {
                return RedirectToAction("Products", "Products");
            }

            product.ProductName = ProductName;
            product.CategoryId = CategoryId;
            product.ProductUnitPrice = ProductUnitPrice;
            product.ProductUnitsInStock = ProductUnitsInStock;

            if (ProductImage != null)
            {
                var fileType = Path.GetExtension(ProductImage.FileName);
                var fileName = $"Product_{product.ProductId}" + fileType;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image", "Store", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ProductImage.CopyTo(stream);
                }

                string oldProductImage = product.ProductImage;
                if (oldProductImage != null)
                {
                    string oldFileName = Path.GetFileName(oldProductImage);
                    string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image", "Store", oldFileName);
                    Console.WriteLine(oldFilePath);

                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }

                }

                product.ProductImage = "image/Store/" + fileName;

            }

            _context.SaveChanges();

            return RedirectToAction("Products", "Products");

        }

        [HttpPost]
        public IActionResult DeleteProductImage(int ProductId)
        {
            // 取得StoreId
            var store = _context.Stores.Where(s => s.StoreAccountNumber == HttpContext.Session.GetString("StoreAccountNumber")).FirstOrDefault();
            if (store == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var product = _context.Products.Where(p => p.StoreId == store.StoreId && p.ProductId == ProductId).FirstOrDefault();

            if (product == null)
            {
                return RedirectToAction("Products", "Products");
            }

            string oldProductImage = product.ProductImage;
            if (oldProductImage != null)
            {
                string oldFileName = Path.GetFileName(oldProductImage);
                string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image", "Store", oldFileName);

                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

            }

            product.ProductImage = null;

            _context.SaveChanges();

            return RedirectToAction("Edit", "Products", new { ProductId = ProductId });
        }

        
        [Route("Products/Copy/{ProductId}")]
        public IActionResult Copy(int ProductId)
        {
            // 取得StoreId
            var store = _context.Stores.Where(s => s.StoreAccountNumber == HttpContext.Session.GetString("StoreAccountNumber")).FirstOrDefault();
            if (store == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var product = _context.Products.Where(p => p.StoreId == store.StoreId && p.ProductId == ProductId).FirstOrDefault();

            if (product == null)
            {
                return RedirectToAction("Products", "Products");
            }

            ViewData["specificProduct"] = product;
            ViewData["categoriesInUse"] = GetCategoriesInUse(store.StoreId);

            return View("Copy");
        }

        [HttpPost]
        [Route("Products/Copy/{ProductId}")]
        public IActionResult Copy(int ProductId, string ProductName, int CategoryId, decimal ProductUnitPrice, short ProductUnitsInStock, IFormFile? ProductImage)
        {

            Console.WriteLine("----------------------------------------------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine($"ProductId: {ProductId}, ProductName: {ProductName}, CategoryId: {CategoryId}, ProductUnitPrice: {ProductUnitPrice}, ProductUnitsInStock: {ProductUnitsInStock}");
            Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------------------------");

            var store = _context.Stores.Where(s => s.StoreAccountNumber == HttpContext.Session.GetString("StoreAccountNumber")).FirstOrDefault();
            if (store == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var newProduct = new Product
            {
                StoreId = store.StoreId,
                ProductName = ProductName,
                CategoryId = CategoryId,
                ProductUnitPrice = ProductUnitPrice,
                ProductUnitsInStock = ProductUnitsInStock,
                ProductOnSell = true,
            };

            _context.Products.Add(newProduct);
            _context.SaveChanges();

            // 若提供圖片的話，儲存圖片
            if (ProductImage != null)
            {
                var fileType = Path.GetExtension(ProductImage.FileName);
                var fileName = $"Product_{newProduct.ProductId}" + fileType;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image", "Store", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ProductImage.CopyTo(stream);
                }

                newProduct.ProductImage = "image/Store/" + fileName;
                _context.SaveChanges();

            }


            return RedirectToAction("Edit", "Products", new { ProductId = newProduct.ProductId });
        }


        [Route("Products/Create")]
        public IActionResult Create()
        {
            var store = _context.Stores.Where(s => s.StoreAccountNumber == HttpContext.Session.GetString("StoreAccountNumber")).FirstOrDefault();
            if (store == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["categoriesInUse"] = GetCategoriesInUse(store.StoreId);

            return View("Create");
        }

        [HttpPost]
        [Route("Products/Create")]
        public IActionResult Create(int ProductId, string ProductName, int CategoryId, decimal ProductUnitPrice, short ProductUnitsInStock, IFormFile? ProductImage)
        {

            Console.WriteLine("----------------------------------------------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine($"ProductId: {ProductId}, ProductName: {ProductName}, CategoryId: {CategoryId}, ProductUnitPrice: {ProductUnitPrice}, ProductUnitsInStock: {ProductUnitsInStock}");
            Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------------------------");

            var store = _context.Stores.Where(s => s.StoreAccountNumber == HttpContext.Session.GetString("StoreAccountNumber")).FirstOrDefault();
            if (store == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var newProduct = new Product
            {
                StoreId = store.StoreId,
                ProductName = ProductName,
                CategoryId = CategoryId,
                ProductUnitPrice = ProductUnitPrice,
                ProductUnitsInStock = ProductUnitsInStock,
                ProductOnSell = true,
            };

            _context.Products.Add(newProduct);
            _context.SaveChanges();

            // 若提供圖片的話，儲存圖片
            if (ProductImage != null)
            {
                var fileType = Path.GetExtension(ProductImage.FileName);
                var fileName = $"Product_{newProduct.ProductId}" + fileType;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image", "Store", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ProductImage.CopyTo(stream);
                }

                newProduct.ProductImage = "image/Store/" + fileName;
                _context.SaveChanges();

            }


            return RedirectToAction("Edit", "Products", new { ProductId = newProduct.ProductId });
        }
        #endregion

        public IEnumerable<dynamic> GetCategoriesInUse(int _StoreId)
        {
            var categoriesInUse = _context.ProductCategories
                    .Where(c => c.StoreId == _StoreId && c.CategoryOnDelete == false)
                    .OrderBy(c => c.CategoryId)
                    .ToList()
                    .Select((c, index) => new
                    {
                        CategoryId = c.CategoryId,
                        CategoryName = c.CategoryName,
                        CategoryOnDelete = c.CategoryOnDelete,
                        CPidx = index   //設定資料欄位索引
                    })
                    .ToList();

            return categoriesInUse;
        }
        public IEnumerable<dynamic> GetProductsWithCategories(int _storeId)
        {
            var productsWithCategories = _context.Products.Where(p => p.StoreId == _storeId)
                  .Join(_context.ProductCategories, p => p.CategoryId, c => c.CategoryId, (p, c) => new {
                      ProductId = p.ProductId,
                      ProductImage = p.ProductImage,
                      ProductName = p.ProductName,
                      CategoryName = c.CategoryName,
                      ProductUnitPrice = p.ProductUnitPrice,
                      ProductUnitsInStock = p.ProductUnitsInStock,
                      ProductOnSell = p.ProductOnSell,

                      CategoryId = c.CategoryId,
                      CategoryOnDelete = c.CategoryOnDelete,

                  })
                  .OrderByDescending(p => p.ProductOnSell)
                  .ThenBy(p => p.CategoryId)
                  .ThenBy(p => p.ProductId)
                  .ToList()
                  .Select((cp, index) => new
                  {
                      ProductId = cp.ProductId,
                      ProductImage = cp.ProductImage,
                      ProductName = cp.ProductName,
                      CategoryName = cp.CategoryName,
                      ProductUnitPrice = cp.ProductUnitPrice,
                      ProductUnitsInStock = cp.ProductUnitsInStock,
                      ProductOnSell = cp.ProductOnSell,

                      CategoryId = cp.CategoryId,
                      CategoryOnDelete = cp.CategoryOnDelete,
                      pidx = index   //設定資料欄位索引
                  })
                  .ToList();

            return productsWithCategories;
        }
    }
}
