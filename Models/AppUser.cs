using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BeautyBook.Models;

public class AppUser : IdentityUser
{
    [MaxLength(100)] public string FullName { get; set; } = string.Empty;
    [MaxLength(100)] public string? City { get; set; }
}
