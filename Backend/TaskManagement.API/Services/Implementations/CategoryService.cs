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
    private readonly ILogger<CategoryService> _logger;
    private readonly ICurrentUserService _currentUser;

    public CategoryService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<CategoryService> logger,
        ICurrentUserService currentUser)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<CategoryDto> CreateAsync(
        CreateCategoryDto dto)
    {
        _logger.LogInformation(
            "Creating category: {CategoryName}",
            dto.Name);

        var category = _mapper.Map<Category>(dto);

        category.UserId = _currentUser.UserId;
        category.CreatedAt = DateTime.UtcNow;

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Category created successfully. Id: {CategoryId}",
            category.Id);

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task DeleteAsync(
        Guid id)
    {
        _logger.LogInformation(
            "Deleting category. Id: {CategoryId}",
            id);

        var category = await _context.Categories
            .FirstOrDefaultAsync(c =>
                c.Id == id &&
                (_currentUser.IsAdmin || c.UserId == _currentUser.UserId));

        if (category is null)
        {
            _logger.LogWarning(
                "Category not found for delete. Id: {CategoryId}",
                id);

            throw new KeyNotFoundException(
                "Kategori bulunamadı.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Category deleted successfully. Id: {CategoryId}",
            id);
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        IQueryable<Category> query = _context.Categories.AsNoTracking();

        if (!_currentUser.IsAdmin)
        {
            query = query.Where(c => c.UserId == _currentUser.UserId);
        }

        var categories = await query
            .OrderBy(c => c.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<CategoryDto>>(
            categories);
    }

    public async Task<CategoryDto> GetByIdAsync(
        Guid id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.Id == id &&
                (_currentUser.IsAdmin || c.UserId == _currentUser.UserId));

        if (category is null)
        {
            throw new KeyNotFoundException(
                "Kategori bulunamadı.");
        }

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> UpdateAsync(
        Guid id,
        UpdateCategoryDto updateCategoryDto)
    {
        _logger.LogInformation(
            "Updating category. Id: {CategoryId}",
            id);

        var category = await _context.Categories
            .FirstOrDefaultAsync(c =>
                c.Id == id &&
                (_currentUser.IsAdmin || c.UserId == _currentUser.UserId));

        if (category is null)
        {
            throw new KeyNotFoundException(
                "Kategori bulunamadı.");
        }

        _mapper.Map(updateCategoryDto, category);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Category updated successfully. Id: {CategoryId}",
            id);

        return _mapper.Map<CategoryDto>(category);
    }
}
