using Microsoft.AspNetCore.Mvc;
using Buoi6.Models;
using Buoi6.Repository;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Buoi6.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ApplicationDbContext _context; // Sử dụng DbContext hiện có
        private readonly IWebHostEnvironment _hostingEnvironment;

        public HomeController(IProductRepository productRepository, ICategoryRepository categoryRepository, ApplicationDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Products()
        {
            var products = await _productRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = categories;
            return View(products);
        }

        public async Task<IActionResult> ProductsByCategory(int? categoryId)
        {
            var products = await _productRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = categories;
            ViewBag.CategoryId = categoryId;

            if (!categoryId.HasValue || categoryId == 0)
            {
                return View("Products", products);
            }

            var filteredProducts = products.Where(p => p.CategoryId == categoryId).ToList();
            return View("Products", filteredProducts);
        }

        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> AddImage(int productId, string imageUrl, IFormFile imageFile)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return NotFound();

            if (product.ProductImages == null)
            {
                product.ProductImages = new List<ProductImage>();
            }

            string imagePath = imageUrl ?? string.Empty;
            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                imagePath = "/uploads/" + uniqueFileName;
            }
            else if (!string.IsNullOrEmpty(imageUrl))
            {
                imagePath = imageUrl;
            }
            else
            {
                return BadRequest("Vui lòng nhập URL hoặc tải lên file ảnh.");
            }

            product.ProductImages.Add(new ProductImage { ImageUrl = imagePath, ProductId = productId });
            await _context.SaveChangesAsync();
            return RedirectToAction("ProductDetails", new { id = productId });
        }
    }
}