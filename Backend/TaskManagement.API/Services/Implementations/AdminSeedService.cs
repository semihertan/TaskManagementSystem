using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data;
using TaskManagement.API.Entities;
using TaskManagement.API.Enums;
using TaskManagement.API.Services.Interfaces;

namespace TaskManagement.API.Services.Implementations;

public class AdminSeedService : IAdminSeedService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminSeedService> _logger;

    public AdminSeedService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<AdminSeedService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var email = _configuration["AdminSeed:Email"]?.Trim();
        var username = _configuration["AdminSeed:Username"]?.Trim();
        var password = _configuration["AdminSeed:Password"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            _logger.LogInformation("Admin seed bilgileri bulunamadığı için işlem atlandı.");
            return;
        }

        if (!MailAddress.TryCreate(email, out _) || username.Length < 3 || password.Length < 12)
        {
            _logger.LogWarning(
                "Admin seed bilgileri geçersiz. E-posta, en az 3 karakter kullanıcı adı ve en az 12 karakter parola gereklidir.");
            return;
        }

        if (await _context.Users.AsNoTracking().AnyAsync(user => user.Role == UserRole.Admin))
        {
            _logger.LogInformation("Sistemde bir admin bulunduğu için admin seed işlemi atlandı.");
            return;
        }

        if (await _context.Users.AsNoTracking().AnyAsync(user =>
            user.Email == email || user.Username == username))
        {
            _logger.LogWarning(
                "Admin seed e-posta adresi veya kullanıcı adı başka bir hesap tarafından kullanılıyor.");
            return;
        }

        var now = DateTime.UtcNow;
        var admin = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FirstName = "System",
            LastName = "Admin",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Users.Add(admin);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Development admin kullanıcısı oluşturuldu: {Email}", email);
    }
}
