// Models/Order.cs (Sửa đổi)
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Buoi6.Models
{
    public enum OrderStatus
    {
        Pending,    // Chờ xử lý
        Processing, // Đang xử lý
        Shipped,    // Đã giao hàng
        Delivered,  // Đã nhận hàng
        Cancelled,  // Đã hủy
        Paid        // Đã thanh toán (nếu bạn muốn tách trạng thái thanh toán riêng)
    }

    public class Order
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        public string ShippingAddress { get; set; }

        public string? Notes { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now; // Gán giá trị mặc định

        public string? UserId { get; set; }
        public decimal TotalPrice { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending; // Thêm trạng thái đơn hàng

        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        // Thêm Navigation Property tới IdentityUser nếu bạn muốn lấy thông tin user đặt hàng
        // public IdentityUser User { get; set; }
    }
}