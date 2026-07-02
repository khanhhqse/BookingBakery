using BookingBakery.Application.DTO;

namespace BookingBakery.Application.IService
{
    public interface ICartService
    {
        Task<CartDto> GetCartByUserIdAsync(int userId);
        Task<CartDto> AddToCartAsync(int userId, AddToCartDto dto);

        /// <summary>Cập nhật số lượng — cần truyền cả sizeName vì composite key gồm 3 field.</summary>
        Task<CartDto> UpdateCartItemQuantityAsync(int userId, int productId, string sizeName, UpdateCartItemQuantityDto dto);

        /// <summary>Xóa 1 item theo productId + sizeName.</summary>
        Task<CartDto> RemoveCartItemAsync(int userId, int productId, string sizeName);

        /// <summary>Xóa nhiều item theo danh sách productId (xóa tất cả size của product đó).</summary>
        Task<CartDto> RemoveItemsFromCartAsync(int userId, List<int> productIds);

        Task<CartDto> ClearCartAsync(int userId);
    }
}