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
        public string SizeName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? StorageInstructions { get; set; }
        public decimal Price { get; set; }
        public decimal SalePrice { get; set; }
        public bool HasActivePromotion { get; set; }
        public string? ActivePromotionTitle { get; set; }
        public decimal CostPrice { get; set; }
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

        /// <summary>Size của sản phẩm này. VD: S, M, L, XL.</summary>
        //[Required(ErrorMessage = "Vui lòng nhập tên size.")]
        [StringLength(20, ErrorMessage = "Tên size không quá 20 ký tự.")]
        public string SizeName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? StorageInstructions { get; set; }

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

    public class UpdateProductStorageInstructionsDto
    {
        public string? StorageInstructions { get; set; }
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