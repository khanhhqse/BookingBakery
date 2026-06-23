using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;
using BookingBakery.Infrastructure.Persistence;
using MongoDB.Driver;

namespace BookingBakery.Domain.IDomain
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IMongoCollection<Order> _orders;

        public OrderRepository(MongoDbContext context)
        {
            _orders = context.GetCollection<Order>("orders");

            // Unique index cho order_id
            var idxOrderId = Builders<Order>.IndexKeys.Ascending(o => o.OrderId);
            _orders.Indexes.CreateOne(
                new CreateIndexModel<Order>(idxOrderId,
                    new CreateIndexOptions { Unique = true, Name = "idx_order_id" }));

            // Index tra cứu theo user_id
            var idxUserId = Builders<Order>.IndexKeys.Ascending(o => o.UserId);
            _orders.Indexes.CreateOne(
                new CreateIndexModel<Order>(idxUserId,
                    new CreateIndexOptions { Name = "idx_order_user_id" }));

            // Index tra cứu theo status
            var idxStatus = Builders<Order>.IndexKeys.Ascending(o => o.Status);
            _orders.Indexes.CreateOne(
                new CreateIndexModel<Order>(idxStatus,
                    new CreateIndexOptions { Name = "idx_order_status" }));
        }

        public async Task<Order?> GetByOrderIdAsync(int orderId)
        {
            return await _orders
                .Find(o => o.OrderId == orderId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Order>> GetByUserIdAsync(int userId)
        {
            // Mới nhất lên trước cho Customer
            return await _orders
                .Find(o => o.UserId == userId)
                .SortByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Order>> GetAllAsync(int page, int pageSize)
        {
            // BR-O04: FIFO — cũ nhất lên trước để Staff xử lý đúng thứ tự
            return await _orders
                .Find(_ => true)
                .SortBy(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetNextOrderIdAsync()
        {
            var last = await _orders
                .Find(_ => true)
                .SortByDescending(o => o.OrderId)
                .Limit(1)
                .FirstOrDefaultAsync();

            return (last?.OrderId ?? 0) + 1;
        }

        public async Task CreateAsync(Order order)
        {
            await _orders.InsertOneAsync(order);
        }

        public async Task UpdateAsync(Order order)
        {
            order.UpdatedAt = DateTime.UtcNow;
            await _orders.ReplaceOneAsync(o => o.OrderId == order.OrderId, order);
        }
    }
}