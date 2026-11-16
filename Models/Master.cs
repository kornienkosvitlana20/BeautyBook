using System.ComponentModel.DataAnnotations;

namespace BeautyBook.Models;

public class Master
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string FullName { get; set; } = string.Empty;
    [Required, MaxLength(80)] public string Specialty { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string City { get; set; } = string.Empty;
    [MaxLength(500)] public string Description { get; set; } = string.Empty;
    [MaxLength(300)] public string? PhotoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<BeautyService> Services { get; set; } = new List<BeautyService>();
}
