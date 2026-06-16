using BookingBakery.Domain.Models;
using System.Linq.Expressions;

namespace BookingBakery.Domain.IDomain
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id, string idField = "category_id");
        Task<Category?> FindOneAsync(Expression<Func<Category, bool>> filter);
        Task CreateAsync(Category entity);
        Task UpdateAsync(Expression<Func<Category, bool>> filter, Category entity);
        Task DeleteAsync(Expression<Func<Category, bool>> filter);
    }
}
