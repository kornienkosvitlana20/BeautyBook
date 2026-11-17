using Microsoft.AspNetCore.Identity;

namespace BeautyBook.Services;

/// <summary>
/// Українські тексти помилок ASP.NET Core Identity (реєстрація, пароль, email).
/// Підключається в Program.cs через AddErrorDescriber.
/// </summary>
public class UkrainianIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError() =>
        new() { Code = nameof(DefaultError), Description = "Сталася невідома помилка. Спробуйте ще раз." };

    public override IdentityError ConcurrencyFailure() =>
        new() { Code = nameof(ConcurrencyFailure), Description = "Дані було змінено іншим запитом. Оновіть сторінку та спробуйте ще раз." };

    public override IdentityError PasswordMismatch() =>
        new() { Code = nameof(PasswordMismatch), Description = "Неправильний пароль." };

    public override IdentityError InvalidToken() =>
        new() { Code = nameof(InvalidToken), Description = "Недійсний токен." };

    public override IdentityError LoginAlreadyAssociated() =>
        new() { Code = nameof(LoginAlreadyAssociated), Description = "Користувач із таким входом уже існує." };

    public override IdentityError InvalidUserName(string? userName) =>
        new() { Code = nameof(InvalidUserName), Description = $"Ім'я користувача «{userName}» недійсне." };

    public override IdentityError InvalidEmail(string? email) =>
        new() { Code = nameof(InvalidEmail), Description = $"Email «{email}» недійсний." };

    public override IdentityError DuplicateUserName(string userName) =>
        new() { Code = nameof(DuplicateUserName), Description = $"Користувач «{userName}» уже зареєстрований." };

    public override IdentityError DuplicateEmail(string email) =>
        new() { Code = nameof(DuplicateEmail), Description = $"Email «{email}» уже використовується." };

    public override IdentityError InvalidRoleName(string? role) =>
        new() { Code = nameof(InvalidRoleName), Description = $"Назва ролі «{role}» недійсна." };

    public override IdentityError DuplicateRoleName(string role) =>
        new() { Code = nameof(DuplicateRoleName), Description = $"Роль «{role}» уже існує." };

    public override IdentityError UserAlreadyHasPassword() =>
        new() { Code = nameof(UserAlreadyHasPassword), Description = "У користувача вже встановлено пароль." };

    public override IdentityError UserLockoutNotEnabled() =>
        new() { Code = nameof(UserLockoutNotEnabled), Description = "Блокування для цього користувача не ввімкнено." };

    public override IdentityError UserAlreadyInRole(string role) =>
        new() { Code = nameof(UserAlreadyInRole), Description = $"Користувач уже має роль «{role}»." };

    public override IdentityError UserNotInRole(string role) =>
        new() { Code = nameof(UserNotInRole), Description = $"Користувач не має ролі «{role}»." };

    public override IdentityError PasswordTooShort(int length) =>
        new() { Code = nameof(PasswordTooShort), Description = $"Пароль має містити щонайменше {length} символів." };

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) =>
        new() { Code = nameof(PasswordRequiresUniqueChars), Description = $"Пароль має містити щонайменше {uniqueChars} різних символів." };

    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Пароль має містити хоча б один спеціальний символ (наприклад, ! @ # $)." };

    public override IdentityError PasswordRequiresDigit() =>
        new() { Code = nameof(PasswordRequiresDigit), Description = "Пароль має містити хоча б одну цифру (0–9)." };

    public override IdentityError PasswordRequiresLower() =>
        new() { Code = nameof(PasswordRequiresLower), Description = "Пароль має містити хоча б одну малу літеру (a–z)." };

    public override IdentityError PasswordRequiresUpper() =>
        new() { Code = nameof(PasswordRequiresUpper), Description = "Пароль має містити хоча б одну велику літеру (A–Z)." };

    public override IdentityError RecoveryCodeRedemptionFailed() =>
        new() { Code = nameof(RecoveryCodeRedemptionFailed), Description = "Не вдалося використати код відновлення." };
}
