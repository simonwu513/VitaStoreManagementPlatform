using Microsoft.AspNetCore.Mvc;
using StoreManagementWebsite.Models;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;


namespace StoreManagementWebsite.Controllers
{
    public class HomeController : Controller
    {
        private StoreManagementPlatformContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly DateTime _todayDate;

        public HomeController(ILogger<HomeController> logger, StoreManagementPlatformContext context)
        {
            _logger = logger;
            _context = context;
            _todayDate = new DateTime(2024, 06, 02);
        }

        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("StoreAccountNumber")))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }
        [HttpPost]
        public IActionResult Index(string StoreAccountNumber, string StorePassword)
        {
            if (string.IsNullOrEmpty(StoreAccountNumber) || string.IsNullOrEmpty(StorePassword))
            {
                TempData["LoginErrorMessage"] = "帳號或密碼不能填空白字串！";
                return RedirectToAction("Index", "Home");
            }

            var store = _context.Stores.FirstOrDefault(s => s.StoreAccountNumber == StoreAccountNumber && s.StorePassword == StorePassword);

            if (store == null)
            {
                TempData["LoginErrorMessage"] = "請輸入正確的帳號密碼！";
                return RedirectToAction("Index", "Home");
            }

            HttpContext.Session.SetString("StoreAccountNumber", StoreAccountNumber);
            //StoreAccountNumber放在伺服器端，只有sessionID放在客戶端

            return RedirectToAction("Login", "Home");

        }


        public IActionResult Login(int? pageNumber)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("StoreAccountNumber")))
            {
                return RedirectToAction("Index", "Home");
            }
            var store = _context.Stores.Where(s => s.StoreAccountNumber == HttpContext.Session.GetString("StoreAccountNumber")).FirstOrDefault();

            if (store == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var todayOrders = _context.Orders.Join(_context.OrderDetails, 
                                                    o => o.OrderId, od => od.OrderId, (o,od) => new {
                                                        OrderId = o.OrderId,
                                                        CustomerId = o.CustomerId,
                                                        StoreId = o.StoreId,
                                                        OrderTime = o.OrderTime,
                                                        CustomerOrderStatus = o.CustomerOrderStatus,
                                                        OrderDeliveryVia = o.OrderDeliveryVia,
                                                        OrderPayment = o.OrderPayment,
                                                        OrderUniformInvoiceVia = o.OrderUniformInvoiceVia,
                                                        OrderEinvoiceNumber = o.OrderEinvoiceNumber,
                                                        OrderAddress = string.Concat(o.OrderAddressCity,o.OrderAddressDistrict,o.OrderAddressDetails),

                                                        ProductID = od.ProductId,
                                                        Quantity = od.Quantity,
                                                }) 
                                            .Where(o => o.OrderTime.Date == _todayDate && o.StoreId == store.StoreId)
                                            .ToList();
            var todayOrdersWithTotalPrice = todayOrders.Join(_context.Products, od => od.ProductID, p => p.ProductId, (od, p) => new { 
                                                    OrderId = od.OrderId,
                                                    CustomerId = od.CustomerId,
                                                    StoreId = od.StoreId,
                                                    OrderTime = od.OrderTime,
                                                    CustomerOrderStatus = od.CustomerOrderStatus,
                                                    OrderDeliveryVia = od.OrderDeliveryVia,
                                                    OrderPayment = od.OrderPayment,
                                                    OrderUniformInvoiceVia = od.OrderUniformInvoiceVia,
                                                    OrderEinvoiceNumber = od.OrderEinvoiceNumber,
                                                    OrderAddress = od.OrderAddress,

                                                    ProductName = p.ProductName,
                                                    ProductPrice = p.ProductUnitPrice,
                                                    Quantity = od.Quantity,
                                                    TotalPrice = od.Quantity * p.ProductUnitPrice
                                                })
                                                .OrderBy(o => o.CustomerOrderStatus)
                                                .ToList();

            var todayOrdersGroupedByOrder = todayOrdersWithTotalPrice.GroupBy(od => od.OrderId)
                                                                     .Select(g => new {
                                                                         OrderId = g.Key,
                                                                         CustomerId = g.First().CustomerId,
                                                                         OrderTime = g.First().OrderTime,                                                                         
                                                                         OrderPayment = g.First().OrderPayment,
                                                                         OrderDeliveryVia = g.First().OrderDeliveryVia,
                                                                         CustomerOrderStatus = g.First().CustomerOrderStatus,
                                                                         TotalPrice = g.Sum(od => od.TotalPrice),

                                                                         ProductQantityText = string.Join(", ",g.Select(od =>  od.ProductName + "(" + od.Quantity + ")")),
                                                                         OrderUniformInvoiceVia = g.First().OrderUniformInvoiceVia,
                                                                         OrderEinvoiceNumber = g.First().OrderEinvoiceNumber,                                                                         
                                                                         OrderAddress = g.First().OrderAddress,
                                                                     })
                                                                     .OrderBy(o => o.CustomerOrderStatus)
                                                                     .ThenBy(o => o.OrderTime)
                                                                     .ToList();

            var todayOrdersGroupedByOrderJoinCustomers = todayOrdersGroupedByOrder.Join(_context.Customers, o => o.CustomerId, c => c.CustomerId, (o, c) => new { 
                                                                         OrderId = o.OrderId,
                                                                         CustomerId = o.CustomerId,
                                                                         CustomerName = c.CustomerName ?? "非會員",
                                                                         
                                                                         OrderTime = o.OrderTime,
                                                                         OrderPayment = o.OrderPayment,
                                                                         OrderDeliveryVia = o.OrderDeliveryVia,
                                                                         CustomerOrderStatus = o.CustomerOrderStatus,
                                                                         TotalPrice = o.TotalPrice,

                                                                         ProductQantityText = o.ProductQantityText,
                                                                         OrderUniformInvoiceVia = o.OrderUniformInvoiceVia,
                                                                         OrderEinvoiceNumber = o.OrderEinvoiceNumber,                                                                         
                                                                         OrderAddress = o.OrderAddress,

                                                                         CustomerCellPhone = c.CustomerCellPhone ?? "未留存",
                                                                         CustomerEmail = c.CustomerEmail ?? "未留存",
                                                                     }).ToList();

            ViewData["todayDate"] = _todayDate.ToString("yyyy-MM-dd");

            int pageSize = 10;
            return View(PaginatedList<dynamic>.Create(todayOrdersGroupedByOrderJoinCustomers.AsQueryable(), pageNumber ?? 1, pageSize));
        }

        [HttpPost]
        public IActionResult Accept(int OrderId)
        {
            var order = _context.Orders.Where(o => o.OrderId == OrderId).FirstOrDefault();
            if (order == null)
            {
                return NotFound();
            }
            order.CustomerOrderStatus = 1;
            _context.SaveChanges();

            return RedirectToAction("Login", "Home");

        }

        [HttpPost]
        public IActionResult Cancel(int OrderId)
        {
            var order = _context.Orders.Where(o => o.OrderId == OrderId).FirstOrDefault();
            if (order == null)
            {
                return NotFound();
            }
            order.CustomerOrderStatus = 4;
            _context.SaveChanges();

            return RedirectToAction("Login", "Home");

        }

        [HttpPost]
        [Route("/Home/Deliver")]
        public JsonResult Deliver([FromBody] DeliverRequest request)
        {
            var order = _context.Orders.Where(o => o.OrderId == request.orderId).FirstOrDefault();
            if (order == null)
            {
                return Json( new { success = false, message = $"{request.orderId} 訂單不存在！" });
            }
            order.CustomerOrderStatus = 2;
            _context.SaveChanges();

            return Json(new { success = true, message = $"{request.orderId} 訂單已配送中/等待自取" });

        }
        public class DeliverRequest
        {
            public int orderId { get; set; }
        }

        [HttpPost]
        [Route("/Home/Finish")]
        public JsonResult Finish([FromBody] FinishRequest request)
        {
            var order = _context.Orders.Where(o => o.OrderId == request.orderId).FirstOrDefault();
            if (order == null)
            {
                return Json(new { success = false, message = $"{request.orderId} 訂單不存在！" });
            }
            order.CustomerOrderStatus = 3;
            _context.SaveChanges();

            return Json(new { success = true, message = $"{request.orderId} 訂單已完成" });

        }

        public class FinishRequest
        {
            public int orderId { get; set; }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
