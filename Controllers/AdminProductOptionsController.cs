using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using katachi.Models.Shop;

namespace katachi.Controllers
{
    public class AdminProductOptionsController : Controller
    {
        private readonly KatachiDbContext _context;

        public AdminProductOptionsController(KatachiDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Options)
                    .ThenInclude(o => o.Values)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return NotFound();
            return View(product);
        }

        public async Task<IActionResult> CreateOption(int productId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) return NotFound();

            ViewBag.Product = product;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateOption(int productId, string name)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) return NotFound();

            var maxSortOrder = await _context.ProductOptions
                .Where(o => o.ProductId == productId)
                .MaxAsync(o => (int?)o.SortOrder) ?? 0;

            _context.ProductOptions.Add(new ProductOption
            {
                ProductId = productId,
                Name = name,
                SortOrder = maxSortOrder + 1
            });
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { productId });
        }

        public async Task<IActionResult> CreateValue(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return NotFound();

            ViewBag.Product = product;
            ViewBag.Options = product.Options.OrderBy(o => o.SortOrder).ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateValue(int productId, int optionId, string text, int? price, int? originalPrice, string? imageUrl)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) return NotFound();

            var option = await _context.ProductOptions.FirstOrDefaultAsync(o => o.Id == optionId && o.ProductId == productId);
            if (option == null) return NotFound();

            var maxSortOrder = await _context.ProductOptionValues
                .Where(v => v.ProductOptionId == optionId)
                .MaxAsync(v => (int?)v.SortOrder) ?? 0;

            _context.ProductOptionValues.Add(new ProductOptionValue
            {
                ProductOptionId = optionId,
                Text = text,
                Price = price,
                OriginalPrice = originalPrice,
                ImageUrl = imageUrl,
                SortOrder = maxSortOrder + 1
            });
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { productId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteValue(int productId, int valueId)
        {
            var value = await _context.ProductOptionValues.FirstOrDefaultAsync(v => v.Id == valueId);
            if (value == null) return NotFound();

            _context.ProductOptionValues.Remove(value);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { productId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOption(int productId, int optionId)
        {
            var option = await _context.ProductOptions
                .Include(o => o.Values)
                .FirstOrDefaultAsync(o => o.Id == optionId && o.ProductId == productId);

            if (option == null) return NotFound();
            if (option.Values.Any()) return BadRequest("這個選項類型底下還有選項值，請先刪除選項值再刪除選項類型。");

            _context.ProductOptions.Remove(option);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { productId });
        }

        public async Task<IActionResult> EditValue(int productId, int valueId)
        {
            var value = await _context.ProductOptionValues
                .Include(v => v.ProductOption)
                .FirstOrDefaultAsync(v => v.Id == valueId);

            if (value == null) return NotFound();

            ViewBag.ProductId = productId;
            return View(value);
        }

        [HttpPost]
        public async Task<IActionResult> EditValue(int productId, int valueId, string text, int? price, int? originalPrice, string? imageUrl)
        {
            var value = await _context.ProductOptionValues.FirstOrDefaultAsync(v => v.Id == valueId);
            if (value == null) return NotFound();

            value.Text = text;
            value.Price = price;
            value.OriginalPrice = originalPrice;
            value.ImageUrl = imageUrl;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { productId });
        }
    }
}
