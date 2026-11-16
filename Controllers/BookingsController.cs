using BeautyBook.Data;
using BeautyBook.Models;
using BeautyBook.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautyBook.Controllers;
[Authorize]
public class BookingsController(ApplicationDbContext db, UserManager<AppUser> users) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Create(int masterId)
    {
        var master = await db.Masters.Include(x => x.Services).FirstOrDefaultAsync(x => x.Id == masterId && x.IsActive);
        if (master is null) return NotFound();
        ViewBag.Master = master;
        return View(new BookingViewModel { MasterId = masterId, StartAt = DateTime.Now.AddDays(1).Date.AddHours(10) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingViewModel model)
    {
        var service = await db.BeautyServices.Include(x => x.Master)
            .FirstOrDefaultAsync(x => x.Id == model.BeautyServiceId && x.MasterId == model.MasterId && x.Master!.IsActive);
        if (service is null) ModelState.AddModelError(nameof(model.BeautyServiceId), "Оберіть послугу цього майстра.");
        if (!DomainRules.IsValidAppointmentTime(model.StartAt, DateTime.Now)) ModelState.AddModelError(nameof(model.StartAt), "Оберіть майбутній час із кроком 15 хвилин.");
        if (service is not null)
        {
            var end = model.StartAt.AddMinutes(service.DurationMinutes);
            var overlaps = await db.Bookings.AnyAsync(b => b.MasterId == model.MasterId && b.Status == "Confirmed" &&
                b.StartAt < end && b.EndAt > model.StartAt);
            if (overlaps) ModelState.AddModelError(nameof(model.StartAt), "Цей час уже зайнятий. Оберіть інший.");
        }
        if (!ModelState.IsValid || service is null)
        {
            ViewBag.Master = await db.Masters.Include(x => x.Services).FirstAsync(x => x.Id == model.MasterId);
            return View(model);
        }
        var userId = users.GetUserId(User);
        if (userId is null) return Challenge();
        var booking = new Booking { CustomerId = userId, MasterId = model.MasterId, BeautyServiceId = service.Id,
            StartAt = model.StartAt, EndAt = model.StartAt.AddMinutes(service.DurationMinutes) };
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(My));
    }

    public async Task<IActionResult> My()
    {
        var id = users.GetUserId(User);
        var items = await db.Bookings.Include(b => b.Master).Include(b => b.BeautyService)
            .Where(b => b.CustomerId == id).OrderByDescending(b => b.StartAt).AsNoTracking().ToListAsync();
        return View(items);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = users.GetUserId(User);
        var booking = await db.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.CustomerId == userId);
        if (booking is null) return NotFound();
        if (booking.StartAt <= DateTime.Now || booking.Status != "Confirmed")
            return BadRequest("Неможливо скасувати цей запис.");
        booking.Status = "Cancelled";
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(My));
    }
}
