using Buoi6.Models;
using Buoi6.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Buoi6.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository productRepository,
            ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            foreach (var product in products)
            {
                Console.WriteLine($"Product ID: {product.Id}, Name: {product.Name}");
            }
            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile imageUrl, List<IFormFile> additionalImages)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    product.ImageUrl = await SaveImage(imageUrl);
                }

                product.ProductImages = new List<ProductImage>();
                if (additionalImages != null)
                {
                    foreach (var file in additionalImages)
                    {
                        if (file != null && file.Length > 0)
                        {
                            string imagePath = await SaveImage(file);
                            product.ProductImages.Add(new ProductImage { ImageUrl = imagePath });
                        }
                    }
                }

                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile imageUrl, List<IFormFile> additionalImages)
        {
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existingProduct = await _productRepository.GetByIdAsync(id);
                if (existingProduct == null) return NotFound();

                // Cập nhật thông tin cơ bản
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;

                // Xử lý ảnh chính
                if (imageUrl != null)
                {
                    existingProduct.ImageUrl = await SaveImage(imageUrl);
                }

                // Xử lý ảnh bổ sung
                if (existingProduct.ProductImages == null)
                {
                    existingProduct.ProductImages = new List<ProductImage>();
                }

                // Giữ lại ảnh bổ sung hiện có
                var existingImageIds = existingProduct.ProductImages.Select(x => x.Id).ToList();

                // Thêm ảnh bổ sung mới
                if (additionalImages != null)
                {
                    foreach (var file in additionalImages)
                    {
                        if (file != null && file.Length > 0)
                        {
                            string imagePath = await SaveImage(file);
                            existingProduct.ProductImages.Add(new ProductImage { ImageUrl = imagePath, ProductId = id });
                        }
                    }
                }

                await _productRepository.UpdateAsync(existingProduct);
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
            var savePath = Path.Combine("wwwroot/images", uniqueFileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + uniqueFileName;
        }
    }
}