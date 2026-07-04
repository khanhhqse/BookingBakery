using BookingBakery.Application.DTO;

namespace BookingBakery.Application.IService
{
    public interface ICartService
    {
        Task<CartDto> GetCartByUserIdAsync(int userId);
        Task<CartDto> AddToCartAsync(int userId, AddToCartDto dto);
        Task<CartDto> UpdateCartItemQuantityAsync(int userId, int productId, UpdateCartItemQuantityDto dto);
        Task<CartDto> RemoveCartItemAsync(int userId, int productId);
        Task<CartDto> RemoveItemsFromCartAsync(int userId, List<int> productIds);
        Task<CartDto> ClearCartAsync(int userId);
    }
}