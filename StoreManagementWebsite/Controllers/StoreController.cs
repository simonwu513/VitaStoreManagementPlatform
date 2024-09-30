using Microsoft.AspNetCore.Mvc;
using StoreManagementWebsite.Models;
using Microsoft.EntityFrameworkCore;

namespace StoreManagementWebsite.Controllers
{
    public class StoreController : Controller
    {
        private StoreManagementPlatformContext _context;
        private readonly ILogger<StoreController> _logger;
        private Dictionary<string, int> _storeOpeningDay = new Dictionary<string, int>();
        private Dictionary<string, string> _storeOpeningDayChinese = new Dictionary<string, string>();
        

        public StoreController(ILogger<StoreController> logger, StoreManagementPlatformContext context)
        {
            _logger = logger;
            _context = context;

            _storeOpeningDay.Add("Mon", 0);
            _storeOpeningDay.Add("Tue", 1);
            _storeOpeningDay.Add("Wed", 2);
            _storeOpeningDay.Add("Thu", 3);
            _storeOpeningDay.Add("Fri", 4);
            _storeOpeningDay.Add("Sat", 5);
            _storeOpeningDay.Add("Sun", 6);

            _storeOpeningDayChinese.Add("Mon", "星期一");
            _storeOpeningDayChinese.Add("Tue", "星期二");
            _storeOpeningDayChinese.Add("Wed", "星期三");
            _storeOpeningDayChinese.Add("Thu", "星期四");
            _storeOpeningDayChinese.Add("Fri", "星期五");
            _storeOpeningDayChinese.Add("Sat", "星期六");
            _storeOpeningDayChinese.Add("Sun", "星期日");

        }

        public IActionResult StoreInfo()
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

            var storeOpeningHours = _context.StoreOpeningHours.Where(s => s.StoreId == store.StoreId)
                                        .Select( s => new
                                        {
                                            MyWeekDay = s.MyWeekDay,
                                            MyWeekDayChinese = _storeOpeningDayChinese[s.MyWeekDay],
                                            MyWeekDayNum = _storeOpeningDay[s.MyWeekDay],
                                            StoreOpenOrNot = s.StoreOpenOrNot,
                                            StoreOpeningTime = s.StoreOpeningTime.HasValue ? s.StoreOpeningTime.Value.ToString("HH:mm") : "",
                                            StoreClosingTime = s.StoreClosingTime.HasValue?　s.StoreClosingTime.Value.ToString("HH:mm") : ""

                                        })                                        
                                        .ToList();  

            ViewData["storeOpeningHours"] = storeOpeningHours.OrderBy(s => s.MyWeekDayNum).ToList();

            return View(store);
        }

        [HttpPost]
        public IActionResult StoreInfo(Store clientstore, IEnumerable<StoreOpeningHour> clientOpeningHours, IFormFile clientStoreImage) {
            
            var store = _context.Stores.Where(s => s.StoreAccountNumber == HttpContext.Session.GetString("StoreAccountNumber")).FirstOrDefault();

            // Stores
            if (store == null)
            {
                return RedirectToAction("Index", "Home");
            }

            store.StoreName = clientstore.StoreName;
            store.StoreAddressCity = clientstore.StoreAddressCity;
            store.StoreAddressDistrict = clientstore.StoreAddressDistrict;
            store.StoreAddressDetails = clientstore.StoreAddressDetails;
            store.StorePhoneNumber = clientstore.StorePhoneNumber;
            store.StoreLinePay = clientstore.StoreLinePay;
            store.StoreUniformInvoiceVia = clientstore.StoreUniformInvoiceVia;

            //StoreImages
            if (clientStoreImage != null)
            {
                var fileType = Path.GetExtension(clientStoreImage.FileName);
                var fileName = $"Store_{store.StoreId}" + fileType;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image", "Store", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    clientStoreImage.CopyTo(stream);
                }

                string oldStoreImage = store.StoreImage;
                if (oldStoreImage != null)
                {
                    string oldFileName = Path.GetFileName(oldStoreImage);
                    string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image", "Store", oldFileName);
                    Console.WriteLine(oldFilePath);

                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }

                }

                store.StoreImage = "image/Store/" + fileName;

            }


            //StoreOpeningHours
            var storeOpeningHours = _context.StoreOpeningHours.Where(s => s.StoreId == store.StoreId).ToList();

            foreach (var item in clientOpeningHours)
            {

                var existingOpeningHour = _context.StoreOpeningHours.FirstOrDefault(s =>
                    s.StoreId == store.StoreId && s.MyWeekDay == item.MyWeekDay);

                if (existingOpeningHour != null)
                {
                    existingOpeningHour.StoreOpenOrNot = item.StoreOpenOrNot == true ? true : false;
                    if(item.StoreOpenOrNot == true)
                    {
                        existingOpeningHour.StoreOpeningTime = item.StoreOpeningTime;
                        existingOpeningHour.StoreClosingTime = item.StoreClosingTime;
                    }

                    
                }
            }

            _context.SaveChanges();

            return RedirectToAction("StoreInfo", "Store");
        }



        
    }
}
