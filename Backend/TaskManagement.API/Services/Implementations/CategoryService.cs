using System.Reflection.Metadata;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.API.Data;
using TaskManagement.API.DTOs.Category;
using TaskManagement.API.Entities;
using TaskManagement.API.Services.Interfaces;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ApplicationDbContext context, IMapper mapper, ILogger<CategoryService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }
    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, Guid userId)
    {
        _logger.LogInformation(
            "Creating category: {CategoryName}",
            dto.Name);
        var category = _mapper.Map<Category>(dto);

        category.UserId = userId;
        category.CreatedAt = DateTime.UtcNow;

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Category created successfully. Id: {CategoryId}",
            category.Id);

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        _logger.LogInformation(
            "Deleting category. Id: {CategoryId}",
            id);

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null)
        {
            _logger.LogWarning(
                "Category not found for delete. Id: {CategoryId}",
                id);
            return false;
        }

        _context.Categories.Remove(category);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Category deleted successfully. Id: {CategoryId}",
            id);

        return true;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync(Guid userId)
    {
        var categories = await _context.Categories
        .Where(c => c.UserId == userId)
        .ToListAsync();

        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id, Guid userId)
    {
            var category = await _context.Categories
        .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null)
            return null;

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateCategoryDto updateCategoryDto, Guid userId)
    {
        _logger.LogInformation(
            "Updating category. Id: {CategoryId}",
            id);
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null)
        {
            _logger.LogWarning(
                "Category not found for update. Id: {CategoryId}",
                id);
            return false;
        }

        _mapper.Map(updateCategoryDto, category);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Category updated successfully. Id: {CategoryId}",
            id);

        return true;
    }
}