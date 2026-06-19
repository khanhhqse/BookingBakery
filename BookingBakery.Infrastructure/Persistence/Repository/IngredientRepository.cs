using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace BookingBakery.Infrastructure.Persistence
{
    public class IngredientRepository : IIngredientRepository
    {
        private readonly IMongoCollection<Ingredient> _collection;

        public IngredientRepository(MongoDbContext context)
        {
            _collection = context.GetCollection<Ingredient>("ingredients");

            // Tạo unique index cho ingredient_id để đảm bảo tính duy nhất
            var indexKeys = Builders<Ingredient>.IndexKeys.Ascending(i => i.IngredientId);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<Ingredient>(indexKeys, indexOptions);
            _collection.Indexes.CreateOne(indexModel);
        }

        public async Task<IEnumerable<Ingredient>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<Ingredient?> GetByIdAsync(int id, string idField = "ingredient_id")
        {
            var filter = Builders<Ingredient>.Filter.Eq(idField, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Ingredient?> FindOneAsync(Expression<Func<Ingredient, bool>> filter)
        {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Ingredient entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(Expression<Func<Ingredient, bool>> filter, Ingredient entity)
        {
            await _collection.ReplaceOneAsync(filter, entity);
        }

        public async Task DeleteAsync(Expression<Func<Ingredient, bool>> filter)
        {
            await _collection.DeleteOneAsync(filter);
        }
    }
}
