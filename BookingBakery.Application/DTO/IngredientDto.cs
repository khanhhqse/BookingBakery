using System.ComponentModel.DataAnnotations;

namespace BookingBakery.Application.DTO
{
    public class IngredientDto
    {
        public int IngredientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public decimal CostPerUnit { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateIngredientDto
    {
        [Required(ErrorMessage = "Tên nguyên liệu là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên nguyên liệu không vượt quá 100 ký tự.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Đơn vị tính là bắt buộc.")]
        [StringLength(20, ErrorMessage = "Đơn vị tính không vượt quá 20 ký tự.")]
        public string Unit { get; set; } = string.Empty;

        [Range(0.001, double.MaxValue, ErrorMessage = "Số lượng tồn kho phải lớn hơn 0.")]
        public decimal CurrentStock { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0.")]
        public decimal CostPerUnit { get; set; }
    }

    public class UpdateIngredientStockDto
    {
        [Range(0, double.MaxValue, ErrorMessage = "Số lượng tồn kho phải lớn hơn hoặc bằng 0.")]
        public decimal CurrentStock { get; set; }
    }

    public class UpdateIngredientCostDto
    {
        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn hoặc bằng 0.")]
        public decimal CostPerUnit { get; set; }
    }

    public class UpdateIngredientDto
    {
        [StringLength(100, ErrorMessage = "Tên nguyên liệu không vượt quá 100 ký tự.")]
        public string? Name { get; set; }

        [StringLength(20, ErrorMessage = "Đơn vị tính không vượt quá 20 ký tự.")]
        public string? Unit { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Số lượng tồn kho phải lớn hơn hoặc bằng 0.")]
        public decimal? CurrentStock { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn hoặc bằng 0.")]
        public decimal? CostPerUnit { get; set; }
    }
}
