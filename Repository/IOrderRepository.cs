// Repository/IOrderRepository.cs
using Buoi6.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Buoi6.Repository
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order> GetByIdAsync(int id);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(int id);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);
    }
}