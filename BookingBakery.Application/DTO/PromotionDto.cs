using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BookingBakery.Application.DTO
{
    public enum PromotionDiscountTypeOption
    {
        Percent = 1,
        Fixed = 2
    }

    // ─────────────────────────────────────────────────────────────
    // REQUEST
    // ─────────────────────────────────────────────────────────────

    public class CreatePromotionRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề chương trình khuyến mãi.")]
        [StringLength(100, MinimumLength = 5,
            ErrorMessage = "Tiêu đề phải từ 5 đến 100 ký tự.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Nội dung không được vượt quá 2000 ký tự.")]
        public string? Content { get; set; }

        /// <summary>Ảnh banner — bắt buộc khi tạo mới.</summary>
        [Required(ErrorMessage = "Vui lòng tải lên hình ảnh banner.")]
        public IFormFile BannerImage { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn loại giảm giá.")]
        public PromotionDiscountTypeOption DiscountType { get; set; }

        /// <summary>Nếu Percent: 1-100. Nếu Fixed: số tiền > 0.</summary>
        [Required(ErrorMessage = "Vui lòng nhập giá trị giảm.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá trị giảm phải lớn hơn 0.")]
        public decimal DiscountValue { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc.")]
        public DateTime EndDate { get; set; }

        /// <summary>Danh sách product_id muốn gắn vào chương trình khuyến mãi này.</summary>
        public List<int> ProductIds { get; set; } = new();
    }

    public class UpdatePromotionRequest
    {
        [StringLength(100, MinimumLength = 5,
            ErrorMessage = "Tiêu đề phải từ 5 đến 100 ký tự.")]
        public string? Title { get; set; }

        [StringLength(2000, ErrorMessage = "Nội dung không được vượt quá 2000 ký tự.")]
        public string? Content { get; set; }

        /// <summary>Để trống nếu không muốn đổi ảnh banner.</summary>
        public IFormFile? BannerImage { get; set; }

        public PromotionDiscountTypeOption? DiscountType { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Giá trị giảm phải lớn hơn 0.")]
        public decimal? DiscountValue { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>Gắn thêm / bỏ sản phẩm khỏi chương trình khuyến mãi.</summary>
    public class UpdatePromotionProductsRequest
    {
        [Required(ErrorMessage = "Vui lòng cung cấp danh sách sản phẩm.")]
        public List<int> ProductIds { get; set; } = new();
    }

    // ─────────────────────────────────────────────────────────────
    // RESPONSE
    // ─────────────────────────────────────────────────────────────

    public class PromotionProductItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal SalePrice { get; set; }
    }

    public class PromotionResponse
    {
        public int PromotionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? BannerUrl { get; set; }
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsOngoing { get; set; }
        public List<PromotionProductItem> Products { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PromotionSummaryResponse
    {
        public int PromotionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? BannerUrl { get; set; }
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsOngoing { get; set; }
        public int ProductCount { get; set; }
    }
}