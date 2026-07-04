using System.ComponentModel.DataAnnotations;

namespace BookingBakery.Application.DTO
{
    public class ProductSizeRequest
    {
        [Required(ErrorMessage = "Tên size là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Tên size không được vượt quá 50 ký tự.")]
        public string SizeName { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn 0.")]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá vốn không được âm.")]
        public decimal CostPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho không được âm.")]
        public int StockQuantity { get; set; }
    }
}