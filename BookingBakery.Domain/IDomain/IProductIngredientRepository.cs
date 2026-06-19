using BookingBakery.Domain.Models;
using System.Linq.Expressions;

namespace BookingBakery.Domain.IDomain
{
    public interface IProductIngredientRepository
    {
        Task<IEnumerable<ProductIngredient>> FindManyAsync(Expression<Func<ProductIngredient, bool>> filter);
        Task<ProductIngredient?> FindOneAsync(Expression<Func<ProductIngredient, bool>> filter);
        Task CreateAsync(ProductIngredient entity);
        Task UpdateAsync(Expression<Func<ProductIngredient, bool>> filter, ProductIngredient entity);
        Task DeleteAsync(Expression<Func<ProductIngredient, bool>> filter);
    }
}
