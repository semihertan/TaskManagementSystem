using System.ComponentModel.DataAnnotations;

namespace TaskManagement.API.DTOs.User;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Mevcut şifre alanı zorunludur.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni şifre alanı zorunludur.")]
    [MinLength(6, ErrorMessage = "Yeni şifre en az 6 karakter olmalıdır.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni şifre tekrar alanı zorunludur.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Yeni şifreler eşleşmiyor.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
