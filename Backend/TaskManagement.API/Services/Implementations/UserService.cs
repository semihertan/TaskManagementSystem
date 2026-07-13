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

    public UserService(ApplicationDbContext context, IMapper mapper, IJwtService jwtService)
    {
        _context = context;
        _mapper = mapper;
        _jwtService = jwtService;
    }
    public async Task<UserDto> RegisterAsync(CreateUserDto createUserDto)
    {
        // kayıtlı var mı kontrolü
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == createUserDto.Email || x.Username == createUserDto.Username);
        
        if(existingUser != null)
        {
            throw new Exception("Bu Email'e kayıtlı başka bir hesap bulunmaktadır!");
        }
         
        // entity oluşturma
        var user = _mapper.Map<User>(createUserDto);
        // şifre hashle
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

        // kaydet
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }
    public async Task<UserDto?> GetProfileAsync(Guid userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return null;
        }

        return _mapper.Map<UserDto>(user);
    }

    public async Task<string> LoginAsync(LoginDto loginDto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == loginDto.Email);

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(
            loginDto.Password,
            user.PasswordHash);

        if (!isPasswordValid)
        {
            throw new Exception("Email veya şifre hatalı");
        }

        return _jwtService.GenerateToken(user);
    }
}