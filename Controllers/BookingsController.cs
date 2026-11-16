
using System.Data;
using BeautyBook.Data;
using BeautyBook.Models;
using BeautyBook.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace BeautyBook.Controllers;

[Authorize]
public class BookingsController(
    ApplicationDbContext db,
    UserManager<AppUser> users) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Create(int masterId)
    {
        if (masterId <= 0)
            return NotFound();

        var master = await db.Masters
            .Include(x => x.Services)
            .FirstOrDefaultAsync(x => x.Id == masterId && x.IsActive);

        if (master is null)
            return NotFound();

        ViewBag.Master = master;

        return View(new BookingViewModel
        {
            MasterId = masterId,
            StartAt = DateTime.Now.AddDays(1).Date.AddHours(10)
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingViewModel model)
    {
        // Перевіряємо майстра
        var master = await db.Masters
            .Include(x => x.Services)
            .FirstOrDefaultAsync(x => x.Id == model.MasterId && x.IsActive);

        if (master is null)
            return NotFound();

        ViewBag.Master = master;

        // Перевіряємо правильність послуги
        var service = await db.BeautyServices
            .FirstOrDefaultAsync(x =>
                x.Id == model.BeautyServiceId &&
                x.MasterId == model.MasterId);

        if (service is null)
        {
            ModelState.AddModelError(
                nameof(model.BeautyServiceId),
                "Оберіть послугу цього майстра.");
        }

        // Перевіряємо дату та час
        if (!DomainRules.IsValidAppointmentTime(
                model.StartAt, DateTime.Now))
        {
            ModelState.AddModelError(
                nameof(model.StartAt),
                "Оберіть майбутній час із кроком 15 хвилин.");
        }

        if (!ModelState.IsValid || service is null)
            return View(model);

        var end = model.StartAt.AddMinutes(service.DurationMinutes);

        // Перевіряємо коректність інтервалу
        if (!DomainRules.IsValidInterval(model.StartAt, end))
        {
            ModelState.AddModelError(
                nameof(model.StartAt),
                "Некоректний часовий інтервал.");

            return View(model);
        }

        var userId = users.GetUserId(User);

        if (userId is null)
            return Challenge();

        try
        {
            // Транзакція захищає перевірку та створення запису
            await using var transaction =
                await db.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            // Повторна перевірка зайнятого часу всередині транзакції
            var overlaps = await db.Bookings.AnyAsync(b =>
                b.MasterId == model.MasterId &&
                b.Status == "Confirmed" &&
                b.StartAt < end &&
                b.EndAt > model.StartAt);

            if (overlaps)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    nameof(model.StartAt),
                    "Цей час уже зайнятий. Оберіть інший.");

                return View(model);
            }

            var booking = new Booking
            {
                CustomerId = userId,
                MasterId = model.MasterId,
                BeautyServiceId = service.Id,
                StartAt = model.StartAt,
                EndAt = end,
                Status = "Confirmed"
            };

            db.Bookings.Add(booking);

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return RedirectToAction(nameof(My));
        }
        catch (SqliteException ex)
            when (ex.SqliteErrorCode is 5 or 6)
        {
            ModelState.AddModelError(
                nameof(model.StartAt),
                "Зараз інший клієнт виконує бронювання. " +
                "Будь ласка, перевірте час і спробуйте ще раз.");

            return View(model);
        }
    }

    public async Task<IActionResult> My()
    {
        var id = users.GetUserId(User);

        var items = await db.Bookings
            .Include(b => b.Master)
            .Include(b => b.BeautyService)
            .Where(b => b.CustomerId == id)
            .OrderByDescending(b => b.StartAt)
            .AsNoTracking()
            .ToListAsync();

        return View(items);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = users.GetUserId(User);

        var booking = await db.Bookings
            .FirstOrDefaultAsync(b =>
                b.Id == id && b.CustomerId == userId);

        if (booking is null)
            return NotFound();

        if (booking.StartAt <= DateTime.Now ||
            booking.Status != "Confirmed")
        {
            return BadRequest(
                "Неможливо скасувати цей запис.");
        }

        booking.Status = "Cancelled";

        await db.SaveChangesAsync();

        return RedirectToAction(nameof(My));
    }
}
