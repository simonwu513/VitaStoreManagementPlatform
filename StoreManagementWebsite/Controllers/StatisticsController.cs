using Microsoft.AspNetCore.Mvc;
using StoreManagementWebsite.Models;

namespace StoreManagementWebsite.Controllers
{
    public class StatisticsController : Controller
    {
       
        private StoreManagementPlatformContext _context;
        private readonly ILogger<StatisticsController> _logger;
        private readonly DateTime _todayDate;

        public StatisticsController(ILogger<StatisticsController> logger, StoreManagementPlatformContext context)
        {
            _logger = logger;
            _context = context;
            _todayDate = new DateTime(2024, 06, 02);
        }

        public IActionResult Month()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("StoreAccountNumber")))
            {
                return RedirectToAction("Index", "Home");
            }
            var storeAccountNumber = HttpContext.Session.GetString("StoreAccountNumber");
            var store = _context.Stores.Where(s => s.StoreAccountNumber == storeAccountNumber).FirstOrDefault();
            if (store == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["StoreName"] = store.StoreName;

            var sales = _context.Orders.Where(s => s.StoreId == store.StoreId && s.OrderTime.Month == _todayDate.AddMonths(-1).Month && s.CustomerOrderStatus == 3).ToList();
            var salesWithProducts = (from o in sales
                                    join od in _context.OrderDetails on o.OrderId equals od.OrderId
                                    select new
                                    {
                                        OrderId = o.OrderId,
                                        CustomerId = o.CustomerId,
                                        OrderTime = o.OrderTime.ToString("yyyy-MM-dd"),
                                        OrderAddressDistrict = o.OrderAddressDistrict,
                                        OrderDeliveryVia = o.OrderDeliveryVia == true ? "外送" : "自取",
                                        OrderPayment = o.OrderPayment == true ? "linePay" : "現金",
                                        
                                        ProductId = od.ProductId,
                                        Quantity = od.Quantity,
                                        ProductUnitPrice = od.UnitPrice,
                                        TotalAmount = od.Quantity * od.UnitPrice,
                                    }).ToList();

            var SalesWithProductsMergedCustomersAndProducts = (from o in salesWithProducts
                                                              join c in _context.Customers on o.CustomerId equals c.CustomerId
                                                              join p in _context.Products on o.ProductId equals p.ProductId
                                                              join pc in _context.ProductCategories on p.CategoryId equals pc.CategoryId
                                                              select new
                                                              {
                                                                  OrderId = o.OrderId,
                                                                  CustomerId = o.CustomerId,
                                                                  CustomerName = c.CustomerName,
                                                                  OrderTime = o.OrderTime,
                                                                  OrderAddressDistrict = o.OrderAddressDistrict,
                                                                  OrderDeliveryVia = o.OrderDeliveryVia,
                                                                  OrderPayment = o.OrderPayment,

                                                                  ProductId = o.ProductId,
                                                                  ProductName = p.ProductName,
                                                                  CategoryId = p.CategoryId,
                                                                  CateogryName = pc.CategoryName,
                                                                  Quantity = o.Quantity,
                                                                  ProductUnitPrice = o.ProductUnitPrice,
                                                                  TotalAmount = o.TotalAmount,
                                                              }).ToList();

            var SalesWithProductsGroupedByOrderId = SalesWithProductsMergedCustomersAndProducts.GroupBy(s => s.OrderId).Select(g => new
            {
                OrderId = g.Key,
                CustomerId = g.First().CustomerId,
                CustomerName = g.First().CustomerName,
                OrderTime = g.First().OrderTime,
                OrderAddressDistrict = g.First().OrderAddressDistrict,
                OrderDeliveryVia = g.First().OrderDeliveryVia,
                OrderPayment = g.First().OrderPayment,

                TotalAmount = g.Sum(s => s.TotalAmount),
                TotalQuantity = g.Sum(s => s.Quantity),
                ProductQuantityTexts = string.Join(", ", g.Select(od => od.ProductName + "(" + od.Quantity + ")"))
            }).ToList();


            // BarPlotProduct
            var forBarPlotProduct = SalesWithProductsMergedCustomersAndProducts.GroupBy(s => s.ProductName).Select(g => new 
            {   
                ProductName = g.Key,
                ProductTotalAmount = g.Sum(s => s.TotalAmount)

            }).ToArray();

            ViewData["forBarPlotProduct"] = forBarPlotProduct;
            Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------------------------------");
            foreach (var item in forBarPlotProduct)
            {
                Console.WriteLine(item.ProductName + " : " + item.ProductTotalAmount);
            }
            Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------------------------------");


            return View();
            
        }
    }
}
