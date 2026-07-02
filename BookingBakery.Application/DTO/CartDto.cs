using System.ComponentModel.DataAnnotations;

namespace BookingBakery.Application.DTO
{
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }

        /// <summary>Giá gốc của size đã chọn.</summary>
        public decimal Price { get; set; }

        /// <summary>Giá sau khuyến mãi (nếu có). Bằng Price nếu không có promotion.</summary>
        public decimal SalePrice { get; set; }

        public bool HasActivePromotion { get; set; }

        /// <summary>Size đã chọn.</summary>
        public string SizeName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        /// <summary>Tính theo SalePrice — số tiền thực tế phải trả.</summary>
        public decimal Subtotal => SalePrice * Quantity;
    }

    public class CartDto
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public decimal TotalAmount => Items.Sum(i => i.Subtotal);
        public int TotalQuantity => Items.Sum(i => i.Quantity);
        public List<string> RemovedItemNotices { get; set; } = new();
        public List<string> AdjustedItemNotices { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class AddToCartDto
    {
        [Required(ErrorMessage = "Product ID là bắt buộc.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn size.")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Tên size không hợp lệ.")]
        public string SizeName { get; set; } = string.Empty;

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