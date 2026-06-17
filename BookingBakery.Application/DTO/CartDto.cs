using System.ComponentModel.DataAnnotations;

namespace BookingBakery.Application.DTO
{
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal => Price * Quantity;
    }

    public class CartDto
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public decimal TotalAmount => Items.Sum(i => i.Subtotal);
        public int TotalQuantity => Items.Sum(i => i.Quantity);

        // BR-C03: sản phẩm bị tự xóa khỏi giỏ (hết hàng/không còn tồn tại)
        public List<string> RemovedItemNotices { get; set; } = new();

        // Mới: sản phẩm bị tự giảm số lượng vì tồn kho không đủ
        public List<string> AdjustedItemNotices { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class AddToCartDto
    {
        [Required(ErrorMessage = "Product ID là bắt buộc.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc.")]
        [Range(1, 50, ErrorMessage = "Số lượng phải từ 1 đến 50.")]
        public int Quantity { get; set; }
    }

    public class UpdateCartItemQuantityDto
    {
        [Required(ErrorMessage = "Số lượng là bắt buộc.")]
        [Range(1, 50, ErrorMessage = "Số lượng phải từ 1 đến 50.")]
        public int Quantity { get; set; }
    }
}