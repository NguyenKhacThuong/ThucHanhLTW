using Microsoft.AspNetCore.Mvc;

namespace Buoi6.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public Order? Order { get; set; }
        public Product? Product { get; set; }

        public List<OrderDetail> OrderDetails { get; set; } = new(); // ✅ thêm = new()

    }
}
