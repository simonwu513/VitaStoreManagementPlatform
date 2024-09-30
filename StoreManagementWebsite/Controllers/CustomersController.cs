using Microsoft.AspNetCore.Mvc;
using StoreManagementWebsite.Models;
using System.Linq;

namespace StoreManagementWebsite.Controllers
{
    public class CustomersController : Controller
    {
        private StoreManagementPlatformContext _context;
        private readonly ILogger<CustomersController> _logger;
        public CustomersController(ILogger<CustomersController> logger, StoreManagementPlatformContext context)
        {
            _logger = logger;
            _context = context;

        }
        public IActionResult CustomerOrderIndex()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("CustomerEmail")))
            {
                return View();
            }
            else
            {
                return RedirectToAction("CustomerOrderLogin", "Customers");
            }
        }

        [HttpPost]
        public IActionResult CustomerOrderIndex(string CustomerEmail, string CustomerPassword)
        {
            var customer = _context.Customers.Where(c => c.CustomerEmail == CustomerEmail && c.CustomerPassword.Trim() == CustomerPassword).FirstOrDefault();

            if (customer == null)
            {
                return View();
            }

            HttpContext.Session.SetString("CustomerEmail", customer.CustomerEmail);

            return RedirectToAction("CustomerOrderLogin", "Customers");
           
        }

        public IActionResult CustomerOrderLogin()
        {

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("CustomerEmail")))
            { 
                return RedirectToAction("CustomerOrderIndex", "Customers");
            }

            var customer = _context.Customers.Where(c => c.CustomerEmail == HttpContext.Session.GetString("CustomerEmail")).FirstOrDefault();
         
            ViewData["Customer"] = customer;


            var CustomerOrder = from o in _context.Orders
                                join od in _context.OrderDetails on o.OrderId equals od.OrderId
                                join p in _context.Products on od.ProductId equals p.ProductId
                                join c in _context.Customers on o.CustomerId equals c.CustomerId
                                join s in _context.Stores on o.StoreId equals s.StoreId
                                where o.CustomerId == customer.CustomerId && o.OrderFinishedTime == null
                                select new
                                {
                                    OrderId = o.OrderId,
                                    StoreId = o.StoreId,
                                    StoreName = s.StoreName,
                                    StoreAddress = string.Concat(s.StoreAddressCity, s.StoreAddressDistrict, s.StoreAddressDetails),
                                    StorePhoneNumber = s.StorePhoneNumber,
                                    OrderTime = o.OrderTime,
                                    CustomerId = o.CustomerId,
                                    CustomerName = c.CustomerName,
                                    OrderAddress = string.Concat(o.OrderAddressCity, o.OrderAddressDistrict, o.OrderAddressDetails),
                                    OrderDeliveryVia = o.OrderDeliveryVia == true ? "外送" : "自取",
                                    OrderPayment = o.OrderPayment == true ? "linePay" : "現金",
                                    OrderUniformInvoiceVia = GetInvoicingType(o.OrderUniformInvoiceVia),
                                    OrderEinvoiceNumber = o.OrderEinvoiceNumber == null ? "" : o.OrderEinvoiceNumber,
                                    CustomerOrderStatus = GetCustomerOrderStatus(o.CustomerOrderStatus),
                                    ProductId = od.ProductId,
                                    ProductName = p.ProductName,
                                    Quantity = od.Quantity,
                                    UnitPrice = od.UnitPrice,
                                    OrderTotalAmount = od.Quantity * od.UnitPrice,
                                };
            if (CustomerOrder.Count() == 0)
            {
                return View(CustomerOrder);
            }

            var CustomerOrderGroupByOrderId = from o in CustomerOrder
                                              group o by o.OrderId into g
                                              select new
                                              {
                                                  OrderId = g.Key,
                                                  OrderTime = g.First().OrderTime.ToString("yyyy-MM-dd HH:mm"),
                                                  StoreId = g.First().StoreId,
                                                  StoreName = g.First().StoreName,
                                                  StoreAddress = g.First().StoreAddress,
                                                  StorePhoneNumber = g.First().StorePhoneNumber,
                                                  CustomerId = g.First().CustomerId,
                                                  CustomerName = g.First().CustomerName,
                                                  OrderAddress = g.First().OrderAddress,
                                                  OrderDeliveryVia = g.First().OrderDeliveryVia,
                                                  OrderPayment = g.First().OrderPayment,
                                                  OrderUniformInvoiceVia = g.First().OrderUniformInvoiceVia,
                                                  OrderEinvoiceNumber = g.First().OrderEinvoiceNumber,
                                                  CustomerOrderStatus = g.First().CustomerOrderStatus,
                                                  ProductProductQantityText = string.Join(", ", g.Select(p => $"{p.ProductName}({p.Quantity})")),
                                                  TotalAmount = g.Sum(p => p.OrderTotalAmount).ToString("C0"),
                                              };

            ViewData["CustomerOrder"] = CustomerOrder;
            return View(CustomerOrderGroupByOrderId.FirstOrDefault());
        }

     
        public JsonResult GetOrderStatus(int orderId)
        {
            var order = _context.Orders.Where(o => o.OrderId == orderId).FirstOrDefault();
            if (order == null)
            {
                return Json(new { success = false, message = $"{orderId} 訂單不存在" });
            }

            return Json(new { success = true, data = GetCustomerOrderStatus(order.CustomerOrderStatus) });
        }

        public static string GetInvoicingType(byte? OrderUniformInvoiceVia)
        {
            switch (OrderUniformInvoiceVia)
            {
                case 0:
                    return "不開立發票";
                case 1:
                    return "紙本發票";
                case 2:
                    return "手機載具";
                default:
                    return "紙本發票";
        }


        }

        public static string GetCustomerOrderStatus(byte? CustomerOrderStatus)
        {
            switch (CustomerOrderStatus)
            {
                case 0:
                    return "待接單";
                case 1:
                    return "製作中";
                case 2:
                    return "配送中/等待自取";
                case 3:
                    return "已完成";
                case 4:
                    return "店家退單";
                case 5:
                    return "客戶取消訂單";
                default:
                    return "待接單";
            }
        }
    }
}
