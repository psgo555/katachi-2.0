using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using katachi.Models.Shop;
using System.Security.Claims;

//前台結帳頁送出訂單
namespace katachi.Controllers
{
    [Route("api/shop/orders")]
    [ApiController]
    public class ShopOrderApiController : ControllerBase
    {
        private readonly KatachiDbContext _context;

        public ShopOrderApiController(KatachiDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (request.Items == null || request.Items.Count == 0)
            {
                return BadRequest("購物車沒有商品");
            }

            var productCodes = request.Items
                .Where(item => !item.Id.StartsWith("bundle-", StringComparison.OrdinalIgnoreCase))
                .Select(item => item.Id)
                .Distinct()
                .ToList();

            var products = await _context.Products
                .Include(product => product.Options)
                    .ThenInclude(option => option.Values)
                .Where(product => productCodes.Contains(product.ProductCode))
                .ToDictionaryAsync(product => product.ProductCode);

            foreach (var item in request.Items.Where(item => !item.Id.StartsWith("bundle-", StringComparison.OrdinalIgnoreCase)))
            {
                if (!products.TryGetValue(item.Id, out var product))
                {
                    return BadRequest($"找不到商品：{item.Id}");
                }

                if (product.Stock < item.Qty)
                {
                    return BadRequest($"{product.Name} 庫存不足");
                }

                product.Stock -= item.Qty;
            }

            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? userId = int.TryParse(userIdText, out var parsedUserId) ? parsedUserId : null;

            var order = new Order
            {
                OrderNumber = "K" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                UserId = userId,
                Status = "待處理",
                RecipientName = request.Recipient.Name,
                RecipientPhone = request.Recipient.Phone,
                RecipientEmail = request.Recipient.Email,
                RecipientAddress = request.Recipient.Address,
                Subtotal = request.Subtotal,
                Shipping = request.Shipping,
                MemberDiscount = request.MemberDiscount,
                CouponDiscount = request.CouponDiscount,
                Total = request.Total,
                CreatedAt = DateTime.Now
            };

            foreach (var item in request.Items)
            {
                products.TryGetValue(item.Id, out var product);

                var orderItem = new OrderItem
                {
                    ProductId = product?.Id,
                    ProductCode = item.Id,
                    ProductName = item.Name,
                    OptionText = item.Subtitle ?? string.Empty,
                    UnitPrice = item.Price,
                    Quantity = item.Qty,
                    Subtotal = item.Price * item.Qty
                };

                foreach (var valueId in ResolveSelectedOptionValueIds(item, product))
                {
                    orderItem.OptionValues.Add(new OrderItemOptionValue
                    {
                        ProductOptionValueId = valueId
                    });
                }

                order.Items.Add(orderItem);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                orderId = order.Id,
                orderNumber = order.OrderNumber
            });
        }

        private static IEnumerable<int> ResolveSelectedOptionValueIds(CreateOrderItemRequest item, Product? product)
        {
            if (item.OptionValueIds.Any())
            {
                return item.OptionValueIds.Distinct();
            }

            if (product == null)
            {
                return Enumerable.Empty<int>();
            }

            var selectedPairs = ParseOptionText(item.Subtitle);
            var selectedIds = new List<int>();

            foreach (var pair in selectedPairs)
            {
                var value = product.Options
                    .FirstOrDefault(option => option.Name == pair.Key)?
                    .Values
                    .FirstOrDefault(value => value.Text == pair.Value);

                if (value != null)
                {
                    selectedIds.Add(value.Id);
                }
            }

            if (selectedIds.Count > 0)
            {
                return selectedIds;
            }

            var imageValue = product.Options
                .SelectMany(option => option.Values)
                .OrderBy(value => value.SortOrder)
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value.ImageUrl));

            return imageValue == null ? Enumerable.Empty<int>() : new[] { imageValue.Id };
        }

        private static Dictionary<string, string> ParseOptionText(string? optionText)
        {
            if (string.IsNullOrWhiteSpace(optionText))
            {
                return new Dictionary<string, string>();
            }

            return optionText
                .Split(" / ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(part => part.Split('：', 2, StringSplitOptions.TrimEntries))
                .Where(parts => parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[0]) && !string.IsNullOrWhiteSpace(parts[1]))
                .GroupBy(parts => parts[0])
                .ToDictionary(group => group.Key, group => group.Last()[1]);
        }
    }

    public class CreateOrderRequest
    {
        public List<CreateOrderItemRequest> Items { get; set; } = new();
        public CreateOrderRecipientRequest Recipient { get; set; } = new();
        public int Subtotal { get; set; }
        public int Shipping { get; set; }
        public int MemberDiscount { get; set; }
        public int CouponDiscount { get; set; }
        public int Total { get; set; }
    }

    public class CreateOrderItemRequest
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public int Price { get; set; }
        public int Qty { get; set; }
        public string Image { get; set; } = string.Empty;
        public List<int> OptionValueIds { get; set; } = new();
    }

    public class CreateOrderRecipientRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
