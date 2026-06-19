using System.ComponentModel.DataAnnotations;

namespace BookingBakery.Application.DTO
{
    public class ProductIngredientDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int IngredientId { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public decimal QuantityRequired { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public decimal ProductCostPrice { get; set; }
    }

    public class ProductRecipeDto
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal ProductCostPrice { get; set; }
        public IEnumerable<RecipeIngredientDto> Ingredients { get; set; } = Enumerable.Empty<RecipeIngredientDto>();
    }

    public class RecipeIngredientDto
    {
        public string IngredientName { get; set; } = string.Empty;
        public decimal QuantityRequired { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
    }

    public class CreateProductIngredientDto
    {
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc.")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên nguyên liệu là bắt buộc.")]
        public string IngredientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số lượng cần thiết là bắt buộc.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Số lượng yêu cầu phải lớn hơn 0.")]
        public decimal QuantityRequired { get; set; }
    }

    public class UpdateProductIngredientDto
    {
        [Required(ErrorMessage = "Số lượng cần thiết là bắt buộc.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Số lượng yêu cầu phải lớn hơn 0.")]
        public decimal QuantityRequired { get; set; }
    }
}
