using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Buoi6.Models;
using Buoi6.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering; // Thêm thư viện này

namespace Buoi6.Controllers
{
    [Authorize] // Yêu cầu người dùng đăng nhập để xem đơn hàng
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public OrderController(IOrderRepository orderRepository, UserManager<IdentityUser> userManager)
        {
            _orderRepository = orderRepository;
            _userManager = userManager;
        }

        // Action cho Admin để quản lý tất cả các đơn hàng
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageOrders()
        {
            var allOrders = await _orderRepository.GetAllAsync();
            return View(allOrders);
        }

        // Hiển thị danh sách đơn hàng của người dùng hiện tại
        public async Task<IActionResult> UserOrders()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var userOrders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            return View(userOrders);
        }

        // Hiển thị chi tiết một đơn hàng cụ thể
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null || order.UserId != userId)
                {
                    return Forbid();
                }
            }

            // **MỚI**: Chuẩn bị SelectList cho dropdown trạng thái nếu là Admin
            if (User.IsInRole("Admin"))
            {
                // Enum.GetValues lấy tất cả các giá trị của enum OrderStatus
                // order.Status là giá trị mặc định được chọn
                ViewBag.StatusList = new SelectList(Enum.GetValues(typeof(OrderStatus)), order.Status);
            }

            return View(order);
        }

        // Action để Admin cập nhật trạng thái đơn hàng
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus newStatus)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return NotFound();
            }

            try
            {
                order.Status = newStatus;
                await _orderRepository.UpdateAsync(order); // Giả định IOrderRepository có phương thức UpdateAsync

                TempData["SuccessMessage"] = $"Trạng thái đơn hàng #{order.Id} đã được cập nhật thành '{newStatus}'.";
                return RedirectToAction(nameof(OrderDetails), new { id = order.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật trạng thái: {ex.Message}";
                return RedirectToAction(nameof(OrderDetails), new { id = order.Id });
            }
        }
    }
}