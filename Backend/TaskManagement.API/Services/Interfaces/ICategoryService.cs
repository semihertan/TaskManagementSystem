using TaskManagement.API.DTOs.Category;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync(Guid userId);

    Task<CategoryDto?> GetByIdAsync(Guid id, Guid userId);

    Task<CategoryDto> CreateAsync(CreateCategoryDto dto, Guid userId);

    Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto dto, Guid userId);

    Task<bool> DeleteAsync(Guid id, Guid userId);
}