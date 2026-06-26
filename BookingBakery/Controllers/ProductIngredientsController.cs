using BookingBakery.Application.DTO;
using BookingBakery.Application.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingBakery.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "1,2")]
    [Produces("application/json")]
    public class ProductIngredientsController : ControllerBase
    {
        private readonly IProductIngredientService _productIngredientService;

        public ProductIngredientsController(IProductIngredientService productIngredientService)
        {
            _productIngredientService = productIngredientService;
        }

        /// <summary>
        /// Tạo mới liên kết nguyên liệu cho sản phẩm theo tên sản phẩm và tên nguyên liệu (Admin và Staff)
        /// </summary>
        [HttpPost]
        [EndpointSummary("Tạo mới liên kết nguyên liệu cho sản phẩm")]
        [EndpointDescription("Cho phép Nhân viên và Admin liên kết một nguyên liệu vào sản phẩm cụ thể bằng tên của chúng")]
        [ProducesResponseType(typeof(ProductIngredientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateProductIngredientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _productIngredientService.AddProductIngredientAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Chỉnh sửa số lượng nguyên liệu yêu cầu theo tên sản phẩm và tên nguyên liệu (Admin và Staff)
        /// </summary>
        [HttpPut("by-names")]
        [EndpointSummary("Chỉnh sửa số lượng nguyên liệu yêu cầu của sản phẩm")]
        [EndpointDescription("Cho phép Nhân viên và Admin thay đổi số lượng cần thiết của một nguyên liệu trong một sản phẩm bằng tên")]
        [ProducesResponseType(typeof(ProductIngredientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(
            [FromQuery] string productName, 
            [FromQuery] string ingredientName, 
            [FromBody] UpdateProductIngredientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _productIngredientService.UpdateProductIngredientAsync(productName, ingredientName, dto);
                if (result == null)
                    return NotFound(new { message = $"Không tìm thấy liên kết giữa sản phẩm '{productName}' và nguyên liệu '{ingredientName}'." });

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Xem danh sách nguyên liệu cần thiết của một sản phẩm theo tên sản phẩm (Admin và Staff)
        /// </summary>
        [HttpGet("by-product/{productName}")]
        [EndpointSummary("Xem các nguyên liệu cần thiết theo tên sản phẩm")]
        [EndpointDescription("Lấy danh sách tất cả các nguyên liệu cùng định mức định lượng tương ứng của một sản phẩm cụ thể bằng tên sản phẩm")]
        [ProducesResponseType(typeof(ProductRecipeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByProduct(string productName)
        {
            try
            {
                var result = await _productIngredientService.GetIngredientsByProductNameAsync(productName);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa liên kết nguyên liệu khỏi sản phẩm theo tên sản phẩm và tên nguyên liệu (Admin và Staff)
        /// </summary>
        [HttpDelete("by-names")]
        [EndpointSummary("Xóa liên kết nguyên liệu khỏi sản phẩm")]
        [EndpointDescription("Cho phép Nhân viên và Admin xóa định lượng của một nguyên liệu ra khỏi một sản phẩm bằng tên")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromQuery] string productName, [FromQuery] string ingredientName)
        {
            var success = await _productIngredientService.DeleteProductIngredientAsync(productName, ingredientName);
            if (!success)
                return NotFound(new { message = $"Không tìm thấy liên kết để xóa giữa sản phẩm '{productName}' và nguyên liệu '{ingredientName}'." });

            return Ok(new { message = "Xóa liên kết nguyên liệu khỏi sản phẩm thành công." });
        }
    }
}
