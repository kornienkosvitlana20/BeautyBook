using System.ComponentModel.DataAnnotations;

namespace BeautyBook.Models;

public class Booking
{
    public int Id { get; set; }
    [Required] public string CustomerId { get; set; } = string.Empty;
    public AppUser? Customer { get; set; }
    public int MasterId { get; set; }
    public Master? Master { get; set; }
    public int BeautyServiceId { get; set; }
    public BeautyService? BeautyService { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    [MaxLength(20)] public string Status { get; set; } = "Confirmed";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
