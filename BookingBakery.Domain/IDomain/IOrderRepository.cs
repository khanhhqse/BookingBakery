using BookingBakery.Domain.Models;

namespace BookingBakery.Domain.IDomain
{
    public interface IOrderRepository
    {
        Task<Order?> GetByOrderIdAsync(int orderId);
        Task<List<Order>> GetByUserIdAsync(int userId);
        Task<List<Order>> GetAllAsync(int page, int pageSize);
        Task<int> GetNextOrderIdAsync();
        Task CreateAsync(Order order);
        Task UpdateAsync(Order order);
    }
}