using System.ComponentModel.DataAnnotations;

namespace BeautyBook.ViewModels;

public class BookingViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Майстра не вказано.")]
    public int MasterId { get; set; }

    [Display(Name = "Послуга")]
    [Required(ErrorMessage = "Оберіть послугу.")]
    [Range(1, int.MaxValue, ErrorMessage = "Оберіть послугу.")]
    public int? BeautyServiceId { get; set; }

    [Display(Name = "Дата")]
    [Required(ErrorMessage = "Оберіть дату запису.")]
    [DataType(DataType.Date)]
    public DateTime? Date { get; set; }

    [Display(Name = "Час")]
    [Required(ErrorMessage = "Оберіть час запису.")]
    public TimeSpan? Time { get; set; }

    // Дата й час початку запису, зібрані з окремих полів форми.
    public DateTime StartAt => (Date ?? DateTime.MinValue).Date + (Time ?? TimeSpan.Zero);
}
