using Microsoft.EntityFrameworkCore;
using Buoi6.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Buoi6.Models
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình độ chính xác và tỷ lệ cho thuộc tính Price
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)"); // 18 là độ chính xác, 2 là tỷ lệ

            // Các cấu hình khác (nếu có)
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
    }
}
