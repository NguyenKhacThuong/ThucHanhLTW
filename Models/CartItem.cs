using Microsoft.AspNetCore.Mvc;

namespace Buoi6.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; } // Tên sản phẩm
        public decimal Price { get; set; } // Giá sản phẩm
        public int Quantity { get; set; } // Số lượng
        public string? ImageUrl { get; set; } // THÊM DÒNG NÀY ĐỂ LƯU URL HÌNH ẢNH CỦA SẢN PHẨM
    }
}