// Repository/EFOrderRepository.cs
// Đảm bảo bạn có file ApplicationDbContext.cs trong thư mục Data
using Buoi6.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buoi6.Repository
{
    public class EFOrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public EFOrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                                 .Include(o => o.OrderDetails)
                                 .ThenInclude(od => od.Product)
                                 .ToListAsync();
        }

        public async Task<Order> GetByIdAsync(int id)
        {
            return await _context.Orders
                                 .Include(o => o.OrderDetails)
                                 .ThenInclude(od => od.Product)
                                 .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _context.Orders
                                 .Where(o => o.UserId == userId)
                                 .Include(o => o.OrderDetails)
                                 .ThenInclude(od => od.Product)
                                 .OrderByDescending(o => o.OrderDate)
                                 .ToListAsync();
        }
    }
}