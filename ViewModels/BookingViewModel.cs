using System.ComponentModel.DataAnnotations;
namespace BeautyBook.ViewModels;
public class BookingViewModel
{
    [Range(1, int.MaxValue)] public int MasterId { get; set; }
    [Range(1, int.MaxValue)] public int BeautyServiceId { get; set; }
    [Required] public DateTime StartAt { get; set; }
}
