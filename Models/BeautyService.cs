using System.ComponentModel.DataAnnotations;

namespace BeautyBook.Models;

public class BeautyService
{
    public int Id { get; set; }
    public int MasterId { get; set; }
    public Master? Master { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Range(1, 100000)] public decimal Price { get; set; }
    [Range(10, 600)] public int DurationMinutes { get; set; } = 60;
}
