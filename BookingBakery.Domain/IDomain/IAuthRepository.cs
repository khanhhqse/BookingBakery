using BookingBakery.Domain.Models;
using System.Linq.Expressions;

namespace BookingBakery.Domain.IDomain
{
    public interface IAuthRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id, string idField = "user_id");
        Task<User?> FindOneAsync(Expression<Func<User, bool>> filter);
        Task CreateAsync(User entity);
        Task UpdateAsync(Expression<Func<User, bool>> filter, User entity);
        Task DeleteAsync(Expression<Func<User, bool>> filter);
        Task<IEnumerable<User>> FindManyAsync(Expression<Func<User, bool>> filter);
    }
}
