using System.ComponentModel.DataAnnotations;

namespace BeautyBook.ViewModels;
public class RegisterViewModel
{
    [Required(ErrorMessage = "Вкажіть ім'я та прізвище.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Ім'я має містити від 2 до 100 символів.")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Вкажіть email.")]
    [EmailAddress(ErrorMessage = "Вкажіть коректний email.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Вкажіть пароль.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль має містити щонайменше 8 символів.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Повторіть пароль.")]
    [Compare(nameof(Password), ErrorMessage = "Паролі не збігаються.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";

    public string? City { get; set; }
}
