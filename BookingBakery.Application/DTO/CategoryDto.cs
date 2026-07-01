using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BookingBakery.Application.DTO
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Tên danh mục là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Tên danh mục không quá 50 ký tự.")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        public IFormFile? Image { get; set; }
    }

    public class UpdateCategoryDto
    {
        [StringLength(50, ErrorMessage = "Tên danh mục không quá 50 ký tự.")]
        public string? Name { get; set; }

        public string? Description { get; set; }
    }
}
