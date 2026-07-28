using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskManagement.API.DTOs.Category;
using TaskManagement.API.Responses;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(
        ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories =
            await _categoryService.GetAllAsync();

        return Ok(new ApiResponse<IEnumerable<CategoryDto>>
        {
            Success = true,
            Message = "Kategoriler başarıyla getirildi.",
            Data = categories
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category =
            await _categoryService.GetByIdAsync(id);

        return Ok(new ApiResponse<CategoryDto>
        {
            Success = true,
            Message = "Kategori başarıyla getirildi.",
            Data = category
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateCategoryDto createCategoryDto)
    {
        var createdCategory =
            await _categoryService.CreateAsync(
                createCategoryDto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdCategory.Id },
            new ApiResponse<CategoryDto>
            {
                Success = true,
                Message = "Kategori başarıyla oluşturuldu.",
                Data = createdCategory
            });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(
        Guid id,
        UpdateCategoryDto updateCategoryDto)
    {
        var updated =
            await _categoryService.UpdateAsync(
                id,
                updateCategoryDto);

        return Ok(new ApiResponse<CategoryDto>
        {
            Success = true,
            Message = "Kategori başarıyla güncellendi.",
            Data = updated
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _categoryService.DeleteAsync(id);

        return NoContent();
    }
}
