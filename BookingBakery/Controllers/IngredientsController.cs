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
    public class IngredientsController : ControllerBase
    {
        private readonly IIngredientService _ingredientService;

        public IngredientsController(IIngredientService ingredientService)
        {
            _ingredientService = ingredientService;
        }

        /// <summary>
        /// Tạo mới một nguyên liệu mới (Admin và Staff)
        /// </summary>
        [HttpPost]
        [EndpointSummary("Tạo mới nguyên liệu")]
        [EndpointDescription("Cho phép Nhân viên và Admin thêm mới nguyên liệu vào kho")]
        [ProducesResponseType(typeof(IngredientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateIngredientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _ingredientService.AddIngredientAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật số lượng tồn kho của nguyên liệu theo tên (Admin và Staff)
        /// </summary>
        [HttpPut("stock-by-name/{name}")]
        [EndpointSummary("Cập nhật số lượng tồn kho nguyên liệu theo tên")]
        [EndpointDescription("Cho phép Nhân viên và Admin cập nhật số lượng tồn kho hiện tại của một nguyên liệu bằng tên của nguyên liệu đó")]
        [ProducesResponseType(typeof(IngredientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateStockByName(string name, [FromBody] UpdateIngredientStockDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _ingredientService.UpdateStockByNameAsync(name, dto.CurrentStock);
            if (result == null)
                return NotFound(new { message = $"Không tìm thấy nguyên liệu có tên '{name}'." });

            return Ok(result);
        }

        /// <summary>
        /// Cập nhật đơn giá nguyên liệu theo tên (Chỉ Admin)
        /// </summary>
        [HttpPut("cost-by-name/{name}")]
        [Authorize(Roles = "1")]
        [EndpointSummary("Cập nhật đơn giá nguyên liệu theo tên")]
        [EndpointDescription("Chỉ cho phép Admin cập nhật đơn giá (giá nhập) của nguyên liệu bằng tên")]
        [ProducesResponseType(typeof(IngredientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCostByName(string name, [FromBody] UpdateIngredientCostDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _ingredientService.UpdateCostByNameAsync(name, dto.CostPerUnit);
            if (result == null)
                return NotFound(new { message = $"Không tìm thấy nguyên liệu có tên '{name}'." });

            return Ok(result);
        }

        /// <summary>
        /// Cập nhật đơn giá nguyên liệu theo ID (Chỉ Admin)
        /// </summary>
        [HttpPut("{id:int}/cost")]
        [Authorize(Roles = "1")]
        [EndpointSummary("Cập nhật đơn giá nguyên liệu theo ID")]
        [EndpointDescription("Chỉ cho phép Admin cập nhật đơn giá (giá nhập) của nguyên liệu bằng ID")]
        [ProducesResponseType(typeof(IngredientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCostById(int id, [FromBody] UpdateIngredientCostDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _ingredientService.UpdateCostByIdAsync(id, dto.CostPerUnit);
            if (result == null)
                return NotFound(new { message = $"Không tìm thấy nguyên liệu có ID = {id}." });

            return Ok(result);
        }

        /// <summary>
        /// Cập nhật thông tin nguyên liệu theo ID (Chỉ Admin)
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "1")]
        [EndpointSummary("Cập nhật nguyên liệu theo ID")]
        [EndpointDescription("Chỉ cho phép Admin cập nhật thông tin chi tiết của nguyên liệu")]
        [ProducesResponseType(typeof(IngredientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateIngredientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _ingredientService.UpdateIngredientAsync(id, dto);
                if (result == null)
                    return NotFound(new { message = $"Không tìm thấy nguyên liệu có ID = {id}." });

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật thông tin nguyên liệu theo tên (Chỉ Admin)
        /// </summary>
        [HttpPut("by-name/{name}")]
        [Authorize(Roles = "1")]
        [EndpointSummary("Cập nhật nguyên liệu theo tên")]
        [EndpointDescription("Chỉ cho phép Admin cập nhật thông tin chi tiết của nguyên liệu bằng tên")]
        [ProducesResponseType(typeof(IngredientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateByName(string name, [FromBody] UpdateIngredientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _ingredientService.UpdateIngredientByNameAsync(name, dto);
                if (result == null)
                    return NotFound(new { message = $"Không tìm thấy nguyên liệu có tên '{name}'." });

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa nguyên liệu theo tên (Admin và Staff)
        /// </summary>
        [HttpDelete("by-name/{name}")]
        [EndpointSummary("Xóa nguyên liệu theo tên")]
        [EndpointDescription("Cho phép Nhân viên và Admin xóa thông tin nguyên liệu bằng tên")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteByName(string name)
        {
            var success = await _ingredientService.DeleteIngredientByNameAsync(name);
            if (!success)
                return NotFound(new { message = $"Không tìm thấy nguyên liệu có tên '{name}'." });

            return Ok(new { message = "Xóa nguyên liệu thành công." });
        }

        /// <summary>
        /// Xóa nguyên liệu theo ID (Admin và Staff)
        /// </summary>
        [HttpDelete("{id:int}")]
        [EndpointSummary("Xóa nguyên liệu theo ID")]
        [EndpointDescription("Cho phép Nhân viên và Admin xóa thông tin nguyên liệu bằng ID")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteById(int id)
        {
            var success = await _ingredientService.DeleteIngredientByIdAsync(id);
            if (!success)
                return NotFound(new { message = $"Không tìm thấy nguyên liệu có ID = {id}." });

            return Ok(new { message = "Xóa nguyên liệu thành công." });
        }

        /// <summary>
        /// Xem toàn bộ danh sách nguyên liệu (Admin và Staff)
        /// </summary>
        [HttpGet]
        [EndpointSummary("Xem toàn bộ danh sách nguyên liệu")]
        [EndpointDescription("Lấy danh sách tất cả các nguyên liệu hiện có trong hệ thống")]
        [ProducesResponseType(typeof(IEnumerable<IngredientDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _ingredientService.GetAllIngredientsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Xem toàn bộ nguyên liệu còn lại trong kho (Admin và Staff)
        /// </summary>
        [HttpGet("in-stock")]
        [EndpointSummary("Xem toàn bộ nguyên liệu còn lại trong kho")]
        [EndpointDescription("Lấy danh sách nguyên liệu có số lượng tồn kho lớn hơn 0")]
        [ProducesResponseType(typeof(IEnumerable<IngredientDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetInStock()
        {
            var result = await _ingredientService.GetIngredientsInStockAsync();
            return Ok(result);
        }

        /// <summary>
        /// Xem toàn bộ nguyên liệu đã hết trong kho (Admin và Staff)
        /// </summary>
        [HttpGet("sold-out")]
        [EndpointSummary("Xem toàn bộ nguyên liệu đã hết trong kho")]
        [EndpointDescription("Lấy danh sách nguyên liệu có số lượng tồn kho bé hơn hoặc bằng 0 (status = sold_out)")]
        [ProducesResponseType(typeof(IEnumerable<IngredientDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSoldOut()
        {
            var result = await _ingredientService.GetIngredientsSoldOutAsync();
            return Ok(result);
        }

        /// <summary>
        /// Xem toàn bộ nguyên liệu sắp hết trong kho (stock <= 10) (Admin và Staff)
        /// </summary>
        [HttpGet("low-stock")]
        [EndpointSummary("Xem nguyên liệu sắp hết trong kho (stock <= 10)")]
        [EndpointDescription("Lấy danh sách các nguyên liệu có số lượng tồn kho bé hơn hoặc bằng 10 để cảnh báo")]
        [ProducesResponseType(typeof(IEnumerable<IngredientDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLowStock()
        {
            var result = await _ingredientService.GetLowStockIngredientsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Xem thông tin nguyên liệu theo tên (Admin và Staff)
        /// </summary>
        [HttpGet("by-name/{name}")]
        [EndpointSummary("Xem thông tin nguyên liệu theo tên")]
        [EndpointDescription("Tìm kiếm chi tiết của một nguyên liệu dựa vào tên")]
        [ProducesResponseType(typeof(IngredientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByName(string name)
        {
            var result = await _ingredientService.GetIngredientByNameAsync(name);
            if (result == null)
                return NotFound(new { message = $"Không tìm thấy nguyên liệu có tên '{name}'." });

            return Ok(result);
        }
    }
}
