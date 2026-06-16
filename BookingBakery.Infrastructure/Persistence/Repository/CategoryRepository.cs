using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace BookingBakery.Infrastructure.Persistence
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IMongoCollection<Category> _collection;

        public CategoryRepository(MongoDbContext context)
        {
            _collection = context.GetCollection<Category>("categories");

            // Tạo unique index cho category_id
            var indexKeys = Builders<Category>.IndexKeys.Ascending(c => c.CategoryId);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<Category>(indexKeys, indexOptions);
            _collection.Indexes.CreateOne(indexModel);
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id, string idField = "category_id")
        {
            var filter = Builders<Category>.Filter.Eq(idField, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Category?> FindOneAsync(Expression<Func<Category, bool>> filter)
        {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Category entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(Expression<Func<Category, bool>> filter, Category entity)
        {
            await _collection.ReplaceOneAsync(filter, entity);
        }

        public async Task DeleteAsync(Expression<Func<Category, bool>> filter)
        {
            await _collection.DeleteOneAsync(filter);
        }
    }
}
