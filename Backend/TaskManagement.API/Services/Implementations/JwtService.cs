using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskManagement.API.Entities;
using TaskManagement.API.Services.Interfaces;

namespace TaskManagement.API.Services.Implementations;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        // Secret Key'i appsettings.json'dan al
        var key = _configuration["Jwt:Key"]!;

        // Security Key oluştur
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key));

        // İmzalama bilgilerini oluştur
        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        // Token içine yazılacak bilgiler (Claims)
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),

            // İleride User.FindFirst(ClaimTypes.NameIdentifier) kullanabilmek için
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),

            // İleride User.Identity.Name kullanabilmek için
            new Claim(ClaimTypes.Name, user.Username)
        };

        // Token oluştur
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
            signingCredentials: credentials
        );

        // String olarak döndür
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}