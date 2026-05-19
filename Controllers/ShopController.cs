using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using katachi.Models.Shop;
using System.Security.Claims;

namespace katachi.Controllers
{
    public class ShopController : Controller
    {
        private readonly KatachiDbContext _context;
        private readonly katachi.Models.Shop.KatachiDbContext _katachiContext;

        public ShopController(KatachiDbContext context, katachi.Models.Shop.KatachiDbContext katachiContext)
        {
            _context = context;
            _katachiContext = katachiContext;
        }

        public IActionResult Index()
        {
            return View("fitness-shop");
        }

        public IActionResult ProductDetail()
        {
            return View("product-detail");
        }

        public IActionResult Checkout()
        {
            return View("checkout");
        }

        public IActionResult Login()
        {
            return RedirectToAction("Index", "Account");
        }

        public IActionResult Register()
        {
            return RedirectToAction("Index", "Account", new { tab = "register" });
        }

        public IActionResult OrderComplete()
        {
            return View("order-complete");
        }

        // 結帳頁自動帶入目前登入會員的基本資料。
        [HttpGet]
        public async Task<IActionResult> CurrentMember()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Unauthorized();
            }

            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdText, out var userId))
            {
                return Unauthorized();
            }

            var member = await _katachiContext.Users
                .AsNoTracking()
                .Where(user => user.UserId == userId)
                .Select(user => new
                {
                    name = user.Name,
                    email = user.Email
                })
                .FirstOrDefaultAsync();

            if (member == null)
            {
                return NotFound();
            }

            return Json(member);
        }

        // 會員訂單追蹤：只顯示目前登入會員自己的訂單。
        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdText, out var userId))
            {
                return RedirectToAction("Index", "Account", new { returnUrl = Url.Action("MyOrders", "Shop") });
            }

            var orders = await _context.Orders
                .AsSplitQuery()
                .Include(order => order.Items)
                    .ThenInclude(item => item.OptionValues)
                        .ThenInclude(value => value.ProductOptionValue)
                .Include(order => order.Items)
                    .ThenInclude(item => item.Product)
                        .ThenInclude(product => product!.Options)
                            .ThenInclude(option => option.Values)
                .Where(order => order.UserId == userId)
                .OrderByDescending(order => order.CreatedAt)
                .ToListAsync();

            return View("my-orders", orders);
        }
    }
}

