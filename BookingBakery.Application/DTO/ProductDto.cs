using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BookingBakery.Application.DTO
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        /// <summary>
        /// Giá sau khi áp Promotion đang diễn ra (nếu có). Bằng Price nếu không có promotion.
        /// </summary>
        public decimal SalePrice { get; set; }
        /// <summary>True nếu sản phẩm đang được áp 1 chương trình khuyến mãi.</summary>
        public bool HasActivePromotion { get; set; }
        /// <summary>Tên chương trình khuyến mãi đang áp (nếu có).</summary>
        public string? ActivePromotionTitle { get; set; }
        public int StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateProductDto
    {
        [Required(ErrorMessage = "Category ID là bắt buộc.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm không quá 100 ký tự.")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Giá bán là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn hoặc bằng 0.")]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá vốn phải lớn hơn hoặc bằng 0.")]
        public decimal CostPrice { get; set; }

        [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc.")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho phải lớn hơn hoặc bằng 0.")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Hình ảnh sản phẩm là bắt buộc.")]
        public IFormFile Image { get; set; } = null!;
    }

    public class UpdateProductStockDto
    {
        [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc.")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho phải lớn hơn hoặc bằng 0.")]
        public int StockQuantity { get; set; }
    }

    public class UpdateProductPriceDto
    {
        [Required(ErrorMessage = "Giá bán là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn hoặc bằng 0.")]
        public decimal Price { get; set; }
    }

    public class UpdateProductDescriptionDto
    {
        public string? Description { get; set; }
    }

    public class UpdateProductNameAndCategoryDto
    {
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm không quá 100 ký tự.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category ID là bắt buộc.")]
        public int CategoryId { get; set; }
    }
}