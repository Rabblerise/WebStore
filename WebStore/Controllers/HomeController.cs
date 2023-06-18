using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Diagnostics;
using System.Security.Claims;

using WebStore.Data;
using WebStore.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace WebStore.Controllers
{
    [ApiController]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IAuthenticationService _authenticationService;

        private ApplicationDbContext db;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IAuthenticationService authenticationService)
        {
            _logger = logger;
            _configuration = configuration;
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connectionString).Options;
            _authenticationService = authenticationService;
            db = new ApplicationDbContext(options);
        }

        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            IQueryable<Item> items = db.Items.Include(item => item.Users);

            string sortBy = Request.Query["sortBy"].ToString();

            List<Item> sortedItems;

            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy)
                {
                    case "name":
                        sortedItems = items.OrderBy(item => item.Name).ToList();
                        break;
                    case "price":
                        sortedItems = items.OrderBy(item => item.Price).ToList();
                        break;
                    case "group":
                        sortedItems = items.OrderBy(item => item.Group).ToList();
                        break;
                    default:
                        sortedItems = items.OrderBy(item => item.Name).ToList();
                        break;
                }
            }
            else
            {
                sortedItems = items.ToList();
            }

            ViewBag.SortBy = sortBy ?? "name";

            return View(sortedItems);
        }

        [HttpGet]
        [Route("checkout/{itemId}")]
        public IActionResult Checkout(int itemId)
        {
            List<Region> regions = db.Regions.ToList();

            ViewBag.itemId = itemId;
            ViewBag.Regions = regions;
            ViewBag.RegionId = regions.FirstOrDefault()?.Id;
            return View();
        }

        [HttpPost]
        [Authorize]
        [Route("checkout/{itemId}")]
        public IActionResult CheckoutPost(int itemId)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            int regionId = int.Parse(Request.Form["regionId"]);
            int amount = int.Parse(Request.Form["amount"]);

            Order order = new Order
            {
                Date = DateTime.Now,
                RegionId = regionId,
                ItemId = itemId,
                Amount = amount,
                UserId = userId
            };

            db.Orders.Add(order);
            db.SaveChanges();

            return RedirectToAction("");
        }

        [HttpGet]
        [Route("privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        [Route("CreateOrder")]
        public IActionResult CreateOrder()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [Route("/Items/Create")]
        public IActionResult Create(Item model)
        {
            if (ModelState.IsValid)
            {
                // Получение идентификатора авторизованного пользователя
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Создание нового экземпляра Item и заполнение его свойств значениями из модели представления
                Item newItem = new Item
                {
                    Name = model.Name,
                    Price = model.Price,
                    Group = model.Group,
                    UserId = userId
                };

                // Сохранение нового товара в базу данных
                db.Items.Add(newItem);
                db.SaveChanges();

                // Перенаправление на страницу с подтверждением
                return RedirectToAction("Confirmation");
            }

            // Если модель представления не прошла валидацию, возвращаем представление с ошибками

            return View(model);
        }

        [HttpGet]
        [Route("Confirmation")]
        [Authorize]
        public IActionResult Confirmation()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        [Route("MyOrders")]
        public IActionResult MyOrders()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            List<Order> orders = db.Orders.Include(o => o.Region).Include(o => o.Item).Where(o => o.UserId == userId).ToList();
            return View(orders);
        }

        [HttpPost]
        [Authorize]
        [Route("RemoveOrder/{orderId}")]
        public IActionResult RemoveOrder(int orderId)
        {
            Order order = db.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                db.Orders.Remove(order);
                db.SaveChanges();
            }
            return RedirectToAction("MyOrders");
        }

        [HttpGet]
        [Authorize]
        [Route("AllOrders")]
        public IActionResult AllOrders()
        {
            List<Order> orders = db.Orders.Include(o => o.Region).Include(o => o.Item).Include(o => o.Users).ToList();
            return View(orders);
        }

        [HttpGet]
        [Authorize]
        [Route("MyItem")]
        public IActionResult MyItem()
        {
            
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; 
            List<Item> userItems = db.Items.Where(item => item.UserId == userId).ToList();

            return View(userItems);
        }
        [HttpGet]
        [Authorize]
        [Route("Items/Edit/{itemId}")]
        public IActionResult Edit(int itemId)
        {
            
            Item item = db.Items.FirstOrDefault(i => i.Id == itemId);

            
            if (item == null)
            {
                return NotFound(); 
            }

            return View(item);
        }

        [HttpPost]
        [Authorize]
        [Route("Items/Edit/{itemId}")]
        public IActionResult Edit(int itemId, Item updatedItem)
        {
            Item item = db.Items.FirstOrDefault(i => i.Id == itemId);

            if (item == null)
            {
                return NotFound(); 
            }

            item.Name = updatedItem.Name;
            item.Price = updatedItem.Price;
            item.Group = updatedItem.Group;

            db.SaveChanges(); 

            return RedirectToAction("MyItem"); 
        }

        [HttpGet]
        [Authorize]
        [Route("Items/Delete/{itemId}")]
        public IActionResult Delete(int itemId)
        {
            Item item = db.Items.FirstOrDefault(i => i.Id == itemId);

            if (item == null)
            {
                return NotFound(); 
            }

            return View(item);
        }

        [HttpPost]
        [Authorize]
        [Route("Items/Delete/{itemId}")]
        public IActionResult DeleteConfirmed(int itemId)
        {
            Item item = db.Items.FirstOrDefault(i => i.Id == itemId);

            if (item == null)
            {
                return NotFound();
            }

            db.Items.Remove(item);
            db.SaveChanges(); 

            return RedirectToAction("MyItem"); 
        }

        [Authorize]
        [Route("Items/ExportItemsToExcel")]
        public ActionResult ExportItemsToExcel()
        {
            List<Item> items = db.Items.ToList();

            ExcelPackage.LicenseContext = new OfficeOpenXml.LicenseContext();

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Items");
                worksheet.Cells.LoadFromCollection(items, true);

                // Настраиваем стили ячеек
                worksheet.Cells.AutoFitColumns();
                worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                // Устанавливаем заголовки столбцов
                for (int i = 1; i <= worksheet.Dimension.Columns; i++)
                {
                    worksheet.Cells[1, i].Style.Font.Bold = true;
                }

                MemoryStream stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Items.xlsx");
            }
        }

        [Authorize]
        [Route("Orders/ExportOrdersToExcel")]
        public ActionResult ExportOrdersToExcel()
        {
            List<Order> orders = db.Orders.ToList();

            ExcelPackage.LicenseContext = new OfficeOpenXml.LicenseContext();

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Orders");
                worksheet.Cells.LoadFromCollection(orders, true);

                // Настраиваем стили ячеек
                worksheet.Cells.AutoFitColumns();
                worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                // Устанавливаем заголовки столбцов
                for (int i = 1; i <= worksheet.Dimension.Columns; i++)
                {
                    worksheet.Cells[1, i].Style.Font.Bold = true;
                }

                MemoryStream stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Orders.xlsx");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet]
        [Route("error")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}