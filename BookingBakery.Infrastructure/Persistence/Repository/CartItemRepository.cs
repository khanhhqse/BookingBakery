using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;
using BookingBakery.Infrastructure.Persistence;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace BookingBakery.Infrastructure.Persistence
{
    public class CartItemRepository : ICartItemRepository
    {
        private readonly IMongoCollection<CartItem> _collection;

        public CartItemRepository(MongoDbContext context)
        {
            _collection = context.GetCollection<CartItem>("cartitems");

            // Drop index cũ (cart_id, product_id) nếu còn tồn tại
            try
            {
                _collection.Indexes.DropOne("cart_id_1_product_id_1");
            }
            catch { /* Index không tồn tại thì bỏ qua */ }

            // Tạo index mới: composite key (cart_id, product_id, size_name)
            var idxKeys = Builders<CartItem>.IndexKeys
                .Ascending(ci => ci.CartId)
                .Ascending(ci => ci.ProductId)
                .Ascending(ci => ci.SizeName);

            _collection.Indexes.CreateOne(
                new CreateIndexModel<CartItem>(idxKeys,
                    new CreateIndexOptions
                    {
                        Unique = true,
                        Name = "idx_cart_product_size_unique"
                    }));
        }

        public async Task<IEnumerable<CartItem>> FindManyAsync(Expression<Func<CartItem, bool>> filter)
        {
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<CartItem?> FindOneAsync(Expression<Func<CartItem, bool>> filter)
        {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(CartItem entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(Expression<Func<CartItem, bool>> filter, CartItem entity)
        {
            await _collection.ReplaceOneAsync(filter, entity);
        }

        public async Task DeleteAsync(Expression<Func<CartItem, bool>> filter)
        {
            await _collection.DeleteOneAsync(filter);
        }

        public async Task DeleteManyAsync(Expression<Func<CartItem, bool>> filter)
        {
            await _collection.DeleteManyAsync(filter);
        }
    }
}