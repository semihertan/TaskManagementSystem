using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data;
using TaskManagement.API.DTOs.Category;
using TaskManagement.API.Entities;
using TaskManagement.API.Services.Interfaces;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CategoryService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, Guid userId)
    {
        var category = _mapper.Map<Category>(dto);

        category.UserId = userId;
        category.CreatedAt = DateTime.UtcNow;

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null)
            return false;

        _context.Categories.Remove(category);

        await _context.SaveChangesAsync();

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
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null)
            return false;

        _mapper.Map(updateCategoryDto, category);

        await _context.SaveChangesAsync();

        return true;
    }
}