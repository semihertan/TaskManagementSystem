using System.ComponentModel.DataAnnotations;

namespace TaskManagement.API.DTOs.User;

public class UpdateUserDto
{
    [Required]
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;
}