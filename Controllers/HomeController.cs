using Microsoft.AspNetCore.Mvc;
using Buoi6.Models;
using Buoi6.Repository;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Buoi6.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public HomeController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Hiển thị danh sách sản phẩm công khai
        public async Task<IActionResult> Products()
        {
            var products = await _productRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync(); // Lấy danh sách danh mục
            ViewBag.Categories = categories; // Gán vào ViewBag
            return View(products);
        }

        // Hiển thị sản phẩm theo danh mục
        public async Task<IActionResult> ProductsByCategory(int? categoryId)
        {
            var products = await _productRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync(); // Lấy danh sách danh mục
            ViewBag.Categories = categories; // Gán vào ViewBag
            ViewBag.CategoryId = categoryId;

            // Nếu categoryId rỗng hoặc 0, hiển thị tất cả sản phẩm
            if (!categoryId.HasValue || categoryId == 0)
            {
                return View("Products", products);
            }

            // Lọc sản phẩm theo danh mục
            var filteredProducts = products.Where(p => p.CategoryId == categoryId).ToList();
            return View("Products", filteredProducts);
        }

        // Hiển thị chi tiết sản phẩm
        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
    }
}