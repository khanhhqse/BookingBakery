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

        [Required(ErrorMessage = "Vui lòng tải lên hình ảnh banner.")]
        public IFormFile BannerImage { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn loại giảm giá.")]
        public PromotionDiscountTypeOption DiscountType { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá trị giảm.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá trị giảm phải lớn hơn 0.")]
        public decimal DiscountValue { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu.")]
        public DateOnly StartDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc.")]
        public DateOnly EndDate { get; set; }

        /// <summary>
        /// Tùy chọn: gắn sẵn sản phẩm ngay lúc tạo promotion.
        /// Mỗi phần tử là ProductId của 1 size cụ thể (vì mỗi size = 1 Product riêng).
        /// Để trống nếu muốn gắn sau qua endpoint /products.
        /// </summary>
        public List<int>? ProductIds { get; set; }
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

        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }

    /// <summary>Gắn thêm sản phẩm vào chương trình khuyến mãi.</summary>
    public class AddPromotionProductRequest
    {
        [Required(ErrorMessage = "Vui lòng cung cấp danh sách sản phẩm.")]
        public List<int> ProductIds { get; set; } = new();

        /// <summary>
        /// Danh sách size được áp promotion. Để trống = áp tất cả size.
        /// VD: ["L", "XL"] = chỉ giảm size L và XL.
        /// </summary>
        public List<string> ApplicableSizes { get; set; } = new();
    }

    /// <summary>Gỡ sản phẩm khỏi chương trình khuyến mãi.</summary>
    public class RemovePromotionProductRequest
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
        public string SizeName { get; set; } = string.Empty;
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