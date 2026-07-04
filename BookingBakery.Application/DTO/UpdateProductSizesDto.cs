using System.ComponentModel.DataAnnotations;

namespace BookingBakery.Application.DTO
{
    public class UpdateProductSizesDto
    {
        [Required(ErrorMessage = "Danh sách size là bắt buộc.")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 size.")]
        public List<ProductSizeRequest> Sizes { get; set; } = new();
    }
}