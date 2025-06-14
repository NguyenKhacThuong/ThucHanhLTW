using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Buoi6.Models
{
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

        public DateTime OrderDate { get; set; }

        public string? UserId { get; set; }
        public decimal TotalPrice { get; set; }

        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
