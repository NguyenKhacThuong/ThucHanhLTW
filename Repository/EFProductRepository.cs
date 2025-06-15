using Buoi6.Models;
using Microsoft.EntityFrameworkCore;

namespace Buoi6.Repository
{
    public class EFProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public EFProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(x => x.Category)
                .Include(x => x.ProductImages)
                .ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(x => x.Category)
                .Include(x => x.ProductImages)
                .SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddAsync(Product product)
        {
            // Đảm bảo ProductImages được gắn với context
            if (product.ProductImages != null && product.ProductImages.Any())
            {
                _context.ProductImages.AddRange(product.ProductImages);
            }
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            // Lấy sản phẩm hiện tại từ context để tránh xung đột
            var existingProduct = await _context.Products
                .Include(x => x.ProductImages)
                .SingleOrDefaultAsync(x => x.Id == product.Id);

            if (existingProduct != null)
            {
                // Cập nhật các thuộc tính cơ bản
                _context.Entry(existingProduct).CurrentValues.SetValues(product);

                // Xử lý ProductImages
                if (product.ProductImages != null)
                {
                    // Xóa các ảnh bổ sung cũ nếu không còn trong danh sách mới
                    var existingImageIds = existingProduct.ProductImages.Select(x => x.Id).ToList();
                    var newImageIds = product.ProductImages.Where(x => x.Id != 0).Select(x => x.Id).ToList();
                    var imagesToRemove = existingProduct.ProductImages.Where(x => !newImageIds.Contains(x.Id)).ToList();
                    _context.ProductImages.RemoveRange(imagesToRemove);

                    // Thêm hoặc cập nhật các ảnh bổ sung mới
                    foreach (var image in product.ProductImages)
                    {
                        if (image.Id == 0) // Ảnh mới
                        {
                            image.ProductId = product.Id;
                            _context.ProductImages.Add(image);
                        }
                        else // Ảnh đã tồn tại, cập nhật nếu cần
                        {
                            _context.ProductImages.Update(image);
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products
                .Include(x => x.ProductImages)
                .SingleOrDefaultAsync(x => x.Id == id);
            if (product != null)
            {
                _context.ProductImages.RemoveRange(product.ProductImages);
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }
    }
}