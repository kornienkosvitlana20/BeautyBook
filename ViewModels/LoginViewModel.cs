using System.ComponentModel.DataAnnotations;
namespace BeautyBook.ViewModels;
public class LoginViewModel
{
    [Required(ErrorMessage = "Вкажіть email.")]
    [EmailAddress(ErrorMessage = "Вкажіть коректний email.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Вкажіть пароль.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }
}
