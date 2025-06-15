using Buoi6.Models;
using Buoi6.Repository; // Đảm bảo đã include namespace này
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

// Đảm bảo bạn có các Extension Methods cho Session nếu chưa có
// namespace Buoi6.Extensions
// {
//     public static class SessionExtensions
//     {
//         public static void SetObjectAsJson<T>(this ISession session, string key, T value)
//         {
//             session.SetString(key, System.Text.Json.JsonSerializer.Serialize(value));
//         }
//
//         public static T? GetObjectFromJson<T>(this ISession session, string key)
//         {
//             var value = session.GetString(key);
//             return value == null ? default(T) : System.Text.Json.JsonSerializer.Deserialize<T>(value);
//         }
//     }
// }

namespace Buoi6.Controllers
{
    [Authorize] // Yêu cầu người dùng đăng nhập để truy cập giỏ hàng và thanh toán
    public class ShoppingCartController : Controller
    {
        // Loại bỏ ApplicationDbContext và thay thế bằng repositories
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public ShoppingCartController(IProductRepository productRepository, // Thay thế ApplicationDbContext
                                      IOrderRepository orderRepository, // Thêm OrderRepository
                                      UserManager<IdentityUser> userManager)
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            return View(cart);
        }

        public async Task<IActionResult> Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || cart.Items.Count == 0)
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống, không thể tiến hành thanh toán.";
                return RedirectToAction("Index");
            }

            // Lấy thông tin người dùng nếu có, để điền trước vào form Order
            // (Không cần dùng trực tiếp user ở đây nếu model Order không dùng Email/UserName)
            // var user = await _userManager.GetUserAsync(User);
            var order = new Order();

            ViewBag.Cart = cart;
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Luôn nên có cho các POST request có form
        public async Task<IActionResult> Checkout(Order order)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || cart.Items.Count == 0)
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                // In lỗi ModelState ra console để debug
                // Console.WriteLine("ModelState không hợp lệ");
                // foreach (var kv in ModelState)
                // {
                //     foreach (var err in kv.Value.Errors)
                //     {
                //         Console.WriteLine($"Lỗi: {kv.Key} - {err.ErrorMessage}");
                //     }
                // }

                ViewBag.Cart = cart;
                return View(order);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                order.UserId = user?.Id;
                order.OrderDate = DateTime.UtcNow;
                order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
                order.Status = OrderStatus.Pending; // <-- Đã thêm: Đặt trạng thái ban đầu của đơn hàng

                // Thêm chi tiết đơn hàng
                foreach (var cartItem in cart.Items)
                {
                    // Lấy lại sản phẩm từ DB để đảm bảo thông tin giá chính xác tại thời điểm đặt hàng
                    var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                    if (product != null)
                    {
                        order.OrderDetails.Add(new OrderDetail
                        {
                            ProductId = product.Id, // Sử dụng ID sản phẩm từ DB
                            Quantity = cartItem.Quantity,
                            Price = product.Price // Sử dụng giá sản phẩm từ DB (đảm bảo chính xác)
                        });
                    }
                }

                await _orderRepository.AddAsync(order); // <-- Đã sửa: Sử dụng repository để thêm đơn hàng

                HttpContext.Session.Remove("Cart"); // Xóa giỏ hàng sau khi đặt thành công
                TempData["SuccessMessage"] = $"Đơn hàng #{order.Id} của bạn đã được đặt thành công!";
                return RedirectToAction("OrderCompleted", new { id = order.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi lưu đơn hàng: " + ex.Message;
                // Ghi log lỗi chi tiết hơn trong môi trường phát triển/kiểm thử
                // Console.WriteLine($"Lỗi: {ex.Message}");
                // Console.WriteLine($"StackTrace: {ex.StackTrace}");
                ViewBag.Cart = cart;
                return View(order);
            }
        }


        public IActionResult OrderCompleted(int id)
        {
            return View(id); // truyền Id sang view
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Đảm bảo có nếu nhận từ form POST
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var product = await _productRepository.GetByIdAsync(productId); // <-- Đã sửa: Dùng IProductRepository
            if (product == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
                return NotFound();
            }

            var cartItem = new CartItem
            {
                ProductId = productId,
                Name = product.Name,
                Price = product.Price,
                Quantity = quantity,
                ImageUrl = product.ImageUrl // Đảm bảo Product có thuộc tính ImageUrl hoặc lấy từ Images collection
            };

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            cart.AddItem(cartItem);
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            TempData["SuccessMessage"] = $"{quantity} x {product.Name} đã được thêm vào giỏ hàng.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Đảm bảo có nếu nhận từ form POST
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            cart.RemoveItem(productId);
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            TempData["SuccessMessage"] = "Sản phẩm đã được xóa khỏi giỏ hàng.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Đảm bảo có nếu nhận từ form POST
        public async Task<IActionResult> BuyNow(int productId, int quantity)
        {
            var product = await _productRepository.GetByIdAsync(productId); // <-- Đã sửa: Dùng IProductRepository
            if (product == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
                return NotFound();
            }

            var cartItem = new CartItem
            {
                ProductId = productId,
                Name = product.Name,
                Price = product.Price,
                Quantity = quantity,
                ImageUrl = product.ImageUrl
            };

            var cart = new ShoppingCart();
            cart.AddItem(cartItem);
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            TempData["InfoMessage"] = $"{quantity} x {product.Name} đã được thêm vào giỏ hàng và bạn được chuyển hướng đến thanh toán.";
            return RedirectToAction("Checkout", "ShoppingCart");
        }

        // Phương thức này không còn cần thiết nếu bạn đã inject IProductRepository
        // private async Task<Product?> GetProductFromDatabase(int productId)
        // {
        //     return await _context.Products
        //         .Include(p => p.Images)
        //         .FirstOrDefaultAsync(p => p.Id == productId);
        // }
    }
}