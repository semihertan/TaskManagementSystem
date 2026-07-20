using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data;
using TaskManagement.API.DTOs.User;
using TaskManagement.API.Entities;
using TaskManagement.API.Services.Interfaces;

namespace TaskManagement.API.Services.Implementations;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;
    private readonly ILogger<UserService> _logger;

    public UserService(ApplicationDbContext context, IMapper mapper, IJwtService jwtService, ILogger<UserService> logger)
    {
        _context = context;
        _mapper = mapper;
        _jwtService = jwtService;
        _logger = logger;
    }
    public async Task<UserDto> RegisterAsync(CreateUserDto createUserDto)
    {
        _logger.LogInformation(
            "Registering user: {Email}",
            createUserDto.Email);

        var exists = await _context.Users
            .AsNoTracking()
            .AnyAsync(x =>
                x.Email == createUserDto.Email ||
                x.Username == createUserDto.Username);
         
        // entity oluşturma
        var user = _mapper.Map<User>(createUserDto);
        // şifre hashle
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

        // kaydet
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "User registered successfully. Id: {UserId}",
            user.Id);

        return _mapper.Map<UserDto>(user);
    }
    public async Task<UserDto?> GetProfileAsync(Guid userId)
    {
        var user = await _context.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return null;
        }

        return _mapper.Map<UserDto>(user);
    }

    public async Task<string> LoginAsync(LoginDto loginDto)
    {
        _logger.LogInformation(
            "Login attempt for {Email}",
            loginDto.Email);

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == loginDto.Email);

        if (user == null)
        {
            _logger.LogWarning(
                "Login failed. User not found: {Email}",
                loginDto.Email);

            throw new Exception("Email veya şifre hatalı.");
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(
            loginDto.Password,
            user.PasswordHash);

        if (!isPasswordValid)
        {
            _logger.LogWarning(
                "Login failed. Invalid password for {Email}",
                loginDto.Email);
            throw new Exception("Email veya şifre hatalı");
        }

        _logger.LogInformation(
            "User logged in successfully. {Email}",
            loginDto.Email);

        return _jwtService.GenerateToken(user);
    }

    public async Task<bool> UserExistsAsync(
    string email,
    string username)
    {
        return await _context.Users
            .AsNoTracking()
            .AnyAsync(x =>
                x.Email == email ||
                x.Username == username);
    }
}