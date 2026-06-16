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
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Xem toàn bộ danh sách danh mục (Mọi người đều có quyền xem kể cả khách chưa đăng nhập)
        /// </summary>
        /// <returns>Danh sách các danh mục</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        /// <summary>
        /// Xem thông tin danh mục theo ID (Chỉ Admin và Staff)
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound(new { message = $"Không tìm thấy danh mục với ID = {id}." });

            return Ok(category);
        }

        /// <summary>
        /// Thêm danh mục mới (Chỉ Admin và Staff)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var category = await _categoryService.AddCategoryAsync(dto);
                return Ok(category);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Sửa thông tin danh mục (Chỉ Admin và Staff)
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _categoryService.UpdateCategoryAsync(id, dto);
            if (category == null)
                return NotFound(new { message = $"Không tìm thấy danh mục với ID = {id}." });

            return Ok(category);
        }

        /// <summary>
        /// Xóa danh mục (Chỉ Admin và Staff)
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _categoryService.DeleteCategoryAsync(id);
            if (!success)
                return NotFound(new { message = $"Không tìm thấy danh mục với ID = {id}." });

            return Ok(new { message = "Xóa danh mục thành công." });
        }

        /// <summary>
        /// Xóa danh mục theo tên (Chỉ Admin và Staff)
        /// </summary>
        [HttpDelete("by-name/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteByName(string name)
        {
            var success = await _categoryService.DeleteCategoryByNameAsync(name);
            if (!success)
                return NotFound(new { message = $"Không tìm thấy danh mục với tên = {name}." });

            return Ok(new { message = "Xóa danh mục theo tên thành công." });
        }

        /// <summary>
        /// Sửa thông tin danh mục theo tên (Chỉ Admin và Staff)
        /// </summary>
        [HttpPut("by-name/{name}")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateByName(string name, [FromBody] UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var category = await _categoryService.UpdateCategoryByNameAsync(name, dto);
                if (category == null)
                    return NotFound(new { message = $"Không tìm thấy danh mục với tên = {name}." });

                return Ok(category);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
