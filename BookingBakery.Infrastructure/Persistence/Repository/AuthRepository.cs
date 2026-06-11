using BookingBakery.Domain.IDomain;
using BookingBakery.Domain.Models;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace BookingBakery.Infrastructure.Persistence
{
    public class AuthRepository : IAuthRepository
    {
        private readonly IMongoCollection<User> _collection;

        public AuthRepository(MongoDbContext context)
        {
            _collection = context.GetCollection<User>("users");
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id, string idField = "user_id")
        {
            var filter = Builders<User>.Filter.Eq(idField, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<User?> FindOneAsync(Expression<Func<User, bool>> filter)
        {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(User entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(Expression<Func<User, bool>> filter, User entity)
        {
            await _collection.ReplaceOneAsync(filter, entity);
        }

        public async Task DeleteAsync(Expression<Func<User, bool>> filter)
        {
            await _collection.DeleteOneAsync(filter);
        }
    }
}
