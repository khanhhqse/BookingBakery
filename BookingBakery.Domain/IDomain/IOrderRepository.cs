using BookingBakery.Domain.Models;

namespace BookingBakery.Domain.IDomain
{
    public interface IOrderRepository
    {
        Task<Order?> GetByOrderIdAsync(int orderId);
        Task<List<Order>> GetByUserIdAsync(int userId);
        Task<List<Order>> GetAllAsync(int page, int pageSize, string? status, DateTime? fromDate, DateTime? toDate);
        Task<int> GetNextOrderIdAsync();
        Task CreateAsync(Order order);
        Task UpdateAsync(Order order);
    }
}