using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TaskManagement.API.DTOs.Category;
using TaskManagement.API.Responses;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]

public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var categories = await _categoryService.GetAllAsync(userId);

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
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var category = await _categoryService.GetByIdAsync(id, userId);

    if (category == null)
    {
        return NotFound(new ApiResponse<object>
        {
            Success = false,
            Message = "Kategori bulunamadı.",
            Data = null
        });
    }

        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryDto createCategoryDto)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var createdCategory = await _categoryService.CreateAsync(createCategoryDto, userId);

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
    public async Task<IActionResult> Update(Guid id, UpdateCategoryDto updateCategoryDto)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var updated = await _categoryService.UpdateAsync(id, updateCategoryDto, userId);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var deleted = await _categoryService.DeleteAsync(id, userId);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}