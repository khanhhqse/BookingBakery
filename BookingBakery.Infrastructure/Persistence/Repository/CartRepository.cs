using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace BookingBakery.Infrastructure.Persistence
{
    public class CartRepository : ICartRepository
    {
        private readonly IMongoCollection<Cart> _collection;

        public CartRepository(MongoDbContext context)
        {
            _collection = context.GetCollection<Cart>("carts");

            var cartIdIndex = new CreateIndexModel<Cart>(
                Builders<Cart>.IndexKeys.Ascending(c => c.CartId),
                new CreateIndexOptions { Unique = true });

            // BR-C01: mỗi user chỉ có đúng 1 giỏ hàng -> unique index trên user_id
            var userIdIndex = new CreateIndexModel<Cart>(
                Builders<Cart>.IndexKeys.Ascending(c => c.UserId),
                new CreateIndexOptions { Unique = true });

            _collection.Indexes.CreateMany(new[] { cartIdIndex, userIdIndex });
        }

        public async Task<IEnumerable<Cart>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<Cart?> FindOneAsync(Expression<Func<Cart, bool>> filter)
        {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Cart entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(Expression<Func<Cart, bool>> filter, Cart entity)
        {
            await _collection.ReplaceOneAsync(filter, entity);
        }

        public async Task DeleteAsync(Expression<Func<Cart, bool>> filter)
        {
            await _collection.DeleteOneAsync(filter);
        }
    }
}