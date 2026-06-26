using BookingBakery.Application.DTO;

namespace BookingBakery.Application.IService
{
    public interface ICartService
    {
        Task<CartDto> GetCartByUserIdAsync(int userId);
        Task<CartDto> AddItemToCartAsync(int userId, AddToCartDto dto);
        Task<CartDto> UpdateItemQuantityAsync(int userId, int productId, int quantity);
        Task<CartDto> RemoveItemFromCartAsync(int userId, int productId);
        Task ClearCartAsync(int userId);
        Task<CartDto> RemoveItemsFromCartAsync(int userId, List<int> productIds);
    }
}