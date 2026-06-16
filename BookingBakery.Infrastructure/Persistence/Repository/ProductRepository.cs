using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace BookingBakery.Infrastructure.Persistence
{
    public class ProductRepository : IProductRepository
    {
        private readonly IMongoCollection<Product> _collection;

        public ProductRepository(MongoDbContext context)
        {
            _collection = context.GetCollection<Product>("products");

            // Tạo unique index cho product_id
            var indexKeys = Builders<Product>.IndexKeys.Ascending(p => p.ProductId);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<Product>(indexKeys, indexOptions);
            _collection.Indexes.CreateOne(indexModel);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id, string idField = "product_id")
        {
            var filter = Builders<Product>.Filter.Eq(idField, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Product?> FindOneAsync(Expression<Func<Product, bool>> filter)
        {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Product entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(Expression<Func<Product, bool>> filter, Product entity)
        {
            await _collection.ReplaceOneAsync(filter, entity);
        }

        public async Task DeleteAsync(Expression<Func<Product, bool>> filter)
        {
            await _collection.DeleteOneAsync(filter);
        }
    }
}
