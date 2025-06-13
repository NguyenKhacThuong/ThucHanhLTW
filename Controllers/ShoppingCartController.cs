using Buoi6.Models;
using Buoi6.Repository; // Đảm bảo SessionExtensions nằm trong namespace này hoặc bạn cần điều chỉnh
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq; // Cần thiết cho .Any() và .Sum()
using System.Threading.Tasks; // Cần thiết cho async/await
using Microsoft.EntityFrameworkCore; // Cần thiết nếu GetProductFromDatabase dùng Include hoặc các truy vấn phức tạp
using System; // Cần thiết cho DateTime.UtcNow

namespace Buoi6.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ShoppingCartController(ApplicationDbContext context,
                                      UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            return View(cart);
        }

        public async Task<IActionResult> Checkout() // Đổi thành async Task vì có thể cần lấy thông tin user
        {
            // Lấy giỏ hàng từ session để kiểm tra trước khi hiển thị form Order
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || cart.Items.Count == 0) // Thay thế !cart.Items.Any()
            {
                // Nếu giỏ hàng trống, chuyển hướng về trang giỏ hàng
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống, không thể tiến hành thanh toán.";
                return RedirectToAction("Index");
            }

            // Có thể pre-populate thông tin người dùng vào form Order nếu có
            // Để pre-populate, bạn cần truyền một Order object vào View
            var user = await _userManager.GetUserAsync(User);
            var order = new Order();
            if (user != null)
            {
                // Giả định Order có các thuộc tính tương ứng
                // Bạn cần thêm các thuộc tính này vào Order model nếu chưa có
                // order.CustomerName = user.UserName;
                // order.PhoneNumber = user.PhoneNumber;
                // order.Email = user.Email;
                // order.Address = user.Address; // Nếu có thuộc tính Address trong IdentityUser
            }

            // Kiểm tra xem Model.ProductName có tồn tại trong CartItem không.
            // Nếu CartItem vẫn dùng Name, hãy thay đổi logic ở dưới.
            // Hiện tại, CartItem có ProductId, Name, Price, Quantity, ImageUrl.
            // Views/ShoppingCart/Index.cshtml dùng item.ProductName.
            // Để đồng bộ, tôi sẽ đổi CartItem.Name thành CartItem.ProductName và Product.Name khi tạo CartItem.
            // HOẶC chỉ cần thay đổi Index.cshtml để sử dụng item.Name nếu bạn muốn giữ nguyên CartItem.Name.
            // Tôi sẽ giữ CartItem có Name và sửa Index.cshtml cho phù hợp để tránh thêm thuộc tính không cần thiết.
            // (Đã thay đổi ở bước trước đó trong CartItem.cs, nên giữ lạiProductName ở đây)

            // Truyền giỏ hàng cho View để hiển thị chi tiết đơn hàng
            // Nếu View Checkout.cshtml cần thông tin giỏ hàng để hiển thị, bạn cần truyền nó đi.
            // Hiện tại Checkout.cshtml đang nhận Order model, nếu muốn hiển thị giỏ hàng, bạn cần tạo một ViewModel
            // hoặc truyền cart qua ViewBag/TempData, hoặc truyền một ViewModel chứa cả Order và Cart.
            // Tạm thời, tôi sẽ truyền Cart để Checkout.cshtml có thể truy cập nếu cần.
            // Hoặc nếu Checkout.cshtml chỉ cần Order, thì giữ nguyên `return View(new Order());`
            // Với code hiện tại của Checkout.cshtml, nó mong đợi một model là ShoppingCart (nếu dùng @model ShoppingCart)
            // hoặc Order (nếu dùng @model Order). Nếu bạn dùng @model Order, thì cart cần được truy cập qua ViewBag.
            // Giả định Checkout.cshtml mong đợi Order, và giỏ hàng được xử lý nội bộ hoặc qua ViewBag.

            ViewBag.Cart = cart; // Truyền giỏ hàng qua ViewBag để Checkout.cshtml có thể truy cập

            return View(order); // Truyền đối tượng Order (có thể đã pre-populate)
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || cart.Items.Count == 0) // Thay thế !cart.Items.Any()
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống, không thể tiến hành thanh toán.";
                return RedirectToAction("Index");
            }

            // Kiểm tra ModelState để đảm bảo dữ liệu form hợp lệ (tên, địa chỉ, sđt nếu có)
            if (!ModelState.IsValid)
            {
                ViewBag.Cart = cart; // Đảm bảo giỏ hàng vẫn được truyền khi có lỗi validation
                return View(order);
            }

            var user = await _userManager.GetUserAsync(User);
            order.UserId = user?.Id; // Có thể null nếu user chưa đăng nhập (mặc dù có Authorize)
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            order.OrderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price // Giá tại thời điểm đặt hàng
                // Không cần ImageUrl hay ProductName ở OrderDetail trừ khi bạn muốn lưu trữ chúng trực tiếp ở đây
            }).ToList();

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("Cart"); // Xóa giỏ hàng sau khi đặt hàng thành công
            TempData["SuccessMessage"] = $"Đơn hàng #{order.Id} của bạn đã được đặt thành công!";
            return View("OrderCompleted", order.Id); // Trang xác nhận đơn hàng, truyền ID đơn hàng
        }

        [HttpPost] // Luôn sử dụng HttpPost cho các hành động thay đổi dữ liệu
        public async Task<IActionResult> AddToCart(int productId, int quantity) // Đổi thành async Task
        {
            var product = await GetProductFromDatabase(productId); // Gọi async
            if (product == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
                return NotFound(); // Trả về 404 nếu sản phẩm không tồn tại
            }

            var cartItem = new CartItem
            {
                ProductId = productId,
                Name = product.Name, // Sử dụng Name từ Product.cs
                Price = product.Price,
                Quantity = quantity,
                ImageUrl = product.ImageUrl // ĐẢM BẢO GÁN ImageUrl VÀO CARTITEM
            };

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            cart.AddItem(cartItem);
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            TempData["SuccessMessage"] = $"{quantity} x {product.Name} đã được thêm vào giỏ hàng.";
            return RedirectToAction("Index"); // Chuyển hướng về trang giỏ hàng
        }

        // Xử lý xóa sản phẩm khỏi giỏ hàng
        [HttpPost] // Nên dùng HttpPost cho hành động thay đổi trạng thái
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            cart.RemoveItem(productId);
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            TempData["SuccessMessage"] = "Sản phẩm đã được xóa khỏi giỏ hàng.";
            return RedirectToAction("Index");
        }

        // ---------- THÊM ACTION BUYNOW TẠI ĐÂY ----------
        [HttpPost] // Luôn dùng HttpPost cho các hành động mua hàng/thay đổi trạng thái
        public async Task<IActionResult> BuyNow(int productId, int quantity) // Đổi thành async Task
        {
            // 1. Lấy thông tin sản phẩm và thêm vào giỏ hàng (tạm thời)
            var product = await GetProductFromDatabase(productId); // Gọi async
            if (product == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
                return NotFound();
            }

            var cartItem = new CartItem
            {
                ProductId = productId,
                Name = product.Name, // Sử dụng Name từ Product.cs
                Price = product.Price,
                Quantity = quantity,
                ImageUrl = product.ImageUrl // ĐẢM BẢO GÁN ImageUrl VÀO CARTITEM
            };

            // Tạo một giỏ hàng mới hoặc sử dụng giỏ hàng hiện có để đảm bảo chỉ có sản phẩm này cho "Mua ngay"
            // Cách đơn giản nhất là xóa giỏ hàng cũ và thêm sản phẩm này vào
            var cart = new ShoppingCart();
            cart.AddItem(cartItem);
            HttpContext.Session.SetObjectAsJson("Cart", cart); // Lưu giỏ hàng mới vào Session

            TempData["InfoMessage"] = $"{quantity} x {product.Name} đã được thêm vào giỏ hàng và bạn được chuyển hướng đến thanh toán.";
            // 2. Chuyển hướng người dùng thẳng đến trang thanh toán (Checkout)
            return RedirectToAction("Checkout", "ShoppingCart");
        }
        // -------------------------------------------------

        // Phương thức hỗ trợ để lấy sản phẩm từ database
        private async Task<Product?> GetProductFromDatabase(int productId) // Đổi thành async Task<Product?>
        {
            // Cải thiện: Sử dụng FindAsync cho khóa chính hoặc FirstOrDefaultAsync
            // Đảm bảo Product có thể bao gồm Category nếu cần hiển thị tên danh mục trong giỏ hàng
            return await _context.Products
                                 .Include(p => p.Images) // Bao gồm ProductImages nếu bạn muốn hiển thị nhiều ảnh
                                 .FirstOrDefaultAsync(p => p.Id == productId);
        }
    }
}