using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreManagementWebsite.Models;
using System.Linq;

namespace StoreManagementWebsite.Controllers
{
    public class OrdersController : Controller
    {
        private StoreManagementPlatformContext _context;
        private readonly ILogger<OrdersController> _logger;
        private DateTime _todayDate;
        public OrdersController(ILogger<OrdersController> logger, StoreManagementPlatformContext context)
        {
            _logger = logger;
            _context = context;
            _todayDate = new DateTime(2024, 06, 02);

        }

        public IActionResult PastOrders(int? pageNumber, string? searchStringForOrderInfo, string? searchStringForCustomerReview, string? searchStringForReviewRating)
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

            ViewData["StoreName"] = store.StoreName;
            ViewData["currentFilterForOrderInfo"] = searchStringForOrderInfo;
            ViewData["currentFilterForCustomerReview"] = searchStringForCustomerReview;
            ViewData["currentFilterReviewRating"] = searchStringForReviewRating;

            var result = PastOrderWithReviewsAndCustomers(store.StoreId, _todayDate);

            if (!string.IsNullOrEmpty(searchStringForOrderInfo))
            {

                result = result.Where(s =>
                                (s.CustomerName != null && s.CustomerName.Contains(searchStringForOrderInfo)) ||
                                (s.OrderId.ToString().Contains(searchStringForOrderInfo)) ||
                                (s.ProductQantityText != null && s.ProductQantityText.Contains(searchStringForOrderInfo))
                                ).ToList();
            }

            if (!string.IsNullOrEmpty(searchStringForCustomerReview))
            {
                result = result.Where(s =>
                                (s.ReviewContent != null && s.ReviewContent.Contains(searchStringForCustomerReview)) ||
                                (s.StoreReplyContent != null && s.StoreReplyContent.Contains(searchStringForCustomerReview))
                                ).ToList();
            }

            if (!string.IsNullOrEmpty(searchStringForReviewRating))
            {
                result = result.Where(s =>
                                (s.ReviewRating != null && s.ReviewRating.ToString() == searchStringForReviewRating)
                                ).ToList();
            }


            int pageSize = 10;
            return View(PaginatedList<dynamic>.Create(result.AsQueryable(), pageNumber ?? 1, pageSize));


        }

        
        [HttpPost]
        public IActionResult ReviewReply(int ReviewId, string StoreReplyContent)
        {
            var review = _context.Reviews.Where(r => r.ReviewId == ReviewId).FirstOrDefault();
            if (review == null)
            {
                return NotFound();
            }

            review.StoreReplyContent = StoreReplyContent;
            review.StoreReplyTime = DateTime.Now;
            _context.SaveChanges();

            return RedirectToAction("PastOrders");
        }


        public IEnumerable<dynamic> PastOrderWithReviewsAndCustomers(int StoreId, DateTime todayDate)
        {
            var orders = _context.Orders.Join(_context.OrderDetails,
                                                  o => o.OrderId, od => od.OrderId, (o, od) => new {
                                                      OrderId = o.OrderId,
                                                      CustomerId = o.CustomerId,
                                                      StoreId = o.StoreId,
                                                      OrderTime = o.OrderTime,
                                                      OrderFinishedTime = o.OrderFinishedTime,
                                                      CustomerOrderStatus = o.CustomerOrderStatus,
                                                      OrderDeliveryVia = o.OrderDeliveryVia,
                                                      OrderPayment = o.OrderPayment,
                                                      OrderUniformInvoiceVia = o.OrderUniformInvoiceVia,
                                                      OrderEinvoiceNumber = o.OrderEinvoiceNumber,
                                                      OrderAddress = string.Concat(o.OrderAddressCity, o.OrderAddressDistrict, o.OrderAddressDetails),

                                                      ProductId = od.ProductId,
                                                      Quantity = od.Quantity,
                                                  })
                                          .Where(o => o.StoreId == StoreId && o.OrderTime < todayDate)
                                          .ToList();
            var ordersWithTotalPrice = orders.Join(_context.Products, od => od.ProductId, p => p.ProductId, (od, p) => new {
                OrderId = od.OrderId,
                CustomerId = od.CustomerId,
                StoreId = od.StoreId,
                OrderTime = od.OrderTime,
                OrderFinishedTime = od.OrderFinishedTime,
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

            var ordersGroupedByOrder = ordersWithTotalPrice.GroupBy(od => od.OrderId)
                                                                     .Select(g => new {
                                                                         OrderId = g.Key,
                                                                         CustomerId = g.First().CustomerId,
                                                                         OrderTime = g.First().OrderTime,
                                                                         OrderFinishedTime = g.First().OrderFinishedTime,
                                                                         OrderPayment = g.First().OrderPayment,
                                                                         OrderDeliveryVia = g.First().OrderDeliveryVia,
                                                                         CustomerOrderStatus = g.First().CustomerOrderStatus,
                                                                         TotalPrice = g.Sum(od => od.TotalPrice),

                                                                         ProductQantityText = string.Join(", ", g.Select(od => od.ProductName + "(" + od.Quantity + ")")),
                                                                         OrderUniformInvoiceVia = g.First().OrderUniformInvoiceVia,
                                                                         OrderEinvoiceNumber = g.First().OrderEinvoiceNumber,
                                                                         OrderAddress = g.First().OrderAddress,
                                                                     })
                                                                     .OrderByDescending(o => o.OrderTime)
                                                                     .ThenBy(o => o.CustomerOrderStatus)
                                                                     .ToList();

            var ordersGroupedByOrderMergeWithReviews = ordersGroupedByOrder
                .GroupJoin(
                    _context.Reviews,
                    o => o.OrderId,
                    r => r.OrderId,
                    (o, reviews) => new {
                        OrderId = o.OrderId,
                        CustomerId = o.CustomerId,
                        OrderFinishedTime = o.OrderFinishedTime,
                        OrderTime = o.OrderTime,
                        OrderPayment = o.OrderPayment,
                        OrderDeliveryVia = o.OrderDeliveryVia,
                        CustomerOrderStatus = o.CustomerOrderStatus,
                        TotalPrice = o.TotalPrice,
                        ProductQantityText = o.ProductQantityText,
                        OrderUniformInvoiceVia = o.OrderUniformInvoiceVia,
                        OrderEinvoiceNumber = o.OrderEinvoiceNumber,
                        OrderAddress = o.OrderAddress,

                        // 使用 DefaultIfEmpty() 取得 left join 的效果
                        ReviewId = reviews.Select(r => r.ReviewId).DefaultIfEmpty().FirstOrDefault(),
                        ReviewTime = reviews.Select(r => r.ReviewTime).DefaultIfEmpty().FirstOrDefault(),
                        ReviewContent = reviews.Select(r => r.ReviewContent).DefaultIfEmpty().FirstOrDefault(),
                        ReviewRating = reviews.Select(r => r.ReviewRating).DefaultIfEmpty().FirstOrDefault(),
                        StoreReplyTime = reviews.Select(r => r.StoreReplyTime).DefaultIfEmpty().FirstOrDefault(),
                        StoreReplyContent = reviews.Select(r => r.StoreReplyContent).DefaultIfEmpty().FirstOrDefault()
                    }
                )
                .OrderByDescending(o => o.OrderTime)
                .ThenBy(o => o.CustomerOrderStatus)
                .ToList();
            

            var ordersGroupedByOrderMergeWithReviewsAndCustomers = ordersGroupedByOrderMergeWithReviews.Join(_context.Customers, o => o.CustomerId, c => c.CustomerId, (o, c) => new {
                OrderId = o.OrderId,
                CustomerId = o.CustomerId,
                OrderFinishedTime = o.OrderFinishedTime,
                OrderTime = o.OrderTime,
                OrderPayment = o.OrderPayment,
                OrderDeliveryVia = o.OrderDeliveryVia,
                CustomerOrderStatus = o.CustomerOrderStatus,
                TotalPrice = o.TotalPrice,

                ProductQantityText = o.ProductQantityText,
                OrderUniformInvoiceVia = o.OrderUniformInvoiceVia,
                OrderEinvoiceNumber = o.OrderEinvoiceNumber,
                OrderAddress = o.OrderAddress,

                ReviewId = o.ReviewId,
                ReviewTime = o.ReviewTime,
                ReviewContent = o.ReviewContent,
                ReviewRating = o.ReviewRating,
                StoreReplyTime = o.StoreReplyTime,
                StoreReplyContent = o.StoreReplyContent,

                CustomerName = c.CustomerName,
                CustomerCellPhone = c.CustomerCellPhone,
                CustomerEmail = c.CustomerEmail
            }).OrderByDescending(o => o.OrderTime)
               .ThenBy(o => o.CustomerOrderStatus)
               .ToList();

            return ordersGroupedByOrderMergeWithReviewsAndCustomers;
        }

        
    }
}
