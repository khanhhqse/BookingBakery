using BookingBakery.Domain.Models;
using System.Linq.Expressions;

namespace BookingBakery.Domain.IDomain
{
    public interface IIngredientRepository
    {
        Task<IEnumerable<Ingredient>> GetAllAsync();
        Task<Ingredient?> GetByIdAsync(int id, string idField = "ingredient_id");
        Task<Ingredient?> FindOneAsync(Expression<Func<Ingredient, bool>> filter);
        Task CreateAsync(Ingredient entity);
        Task UpdateAsync(Expression<Func<Ingredient, bool>> filter, Ingredient entity);
        Task DeleteAsync(Expression<Func<Ingredient, bool>> filter);
    }
}
