
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
            Date = DateTime.Today.AddDays(1),
            Time = TimeSpan.FromHours(10)
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

        // Якщо послугу не обрано взагалі, помилку вже додала валідація моделі
        if (service is null && model.BeautyServiceId > 0)
        {
            ModelState.AddModelError(
                nameof(model.BeautyServiceId),
                "Обрана послуга не належить цьому майстру. Оберіть послугу зі списку.");
        }
        else if (service is null && !HasError(nameof(model.BeautyServiceId)))
        {
            ModelState.AddModelError(
                nameof(model.BeautyServiceId),
                "Оберіть послугу.");
        }

        // Перевіряємо дату та час (окремі повідомлення для кожної причини)
        if (model.Date is null || model.Time is null)
        {
            if (!HasError(nameof(model.Date)) && !HasError(nameof(model.Time)))
            {
                ModelState.AddModelError(
                    nameof(model.StartAt),
                    "Оберіть дату та час запису.");
            }
        }
        else
        {
            var now = DateTime.Now;

            if (model.StartAt <= now)
            {
                ModelState.AddModelError(
                    nameof(model.StartAt),
                    "Не можна записатися на минулу дату або час. Оберіть майбутній час.");
            }
            else if (!DomainRules.IsValidAppointmentTime(model.StartAt, now))
            {
                ModelState.AddModelError(
                    nameof(model.StartAt),
                    "Час запису має бути кратним 15 хвилинам (наприклад, 10:00, 10:15, 10:30).");
            }
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

            TempData["Success"] =
                $"Запис створено: {service.Name} у майстра {master.FullName}, " +
                $"{model.StartAt:dd.MM.yyyy} о {model.StartAt:HH:mm}.";

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

    private bool HasError(string key)
        => ModelState.TryGetValue(key, out var entry) && entry.Errors.Count > 0;

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

        if (userId is null)
            return Challenge();

        // Шукаємо запис лише серед записів поточного користувача:
        // чужий запис для нього "не існує" (404)
        var booking = await db.Bookings
            .FirstOrDefaultAsync(b =>
                b.Id == id && b.CustomerId == userId);

        if (booking is null)
            return NotFound();

        if (booking.Status != "Confirmed")
        {
            TempData["Error"] = "Цей запис уже скасовано.";
            return RedirectToAction(nameof(My));
        }

        if (booking.StartAt <= DateTime.Now)
        {
            TempData["Error"] = "Не можна скасувати запис, який уже відбувся або розпочався.";
            return RedirectToAction(nameof(My));
        }

        // Не видаляємо запис, а лише змінюємо статус
        booking.Status = "Cancelled";

        await db.SaveChangesAsync();

        TempData["Success"] = "Запис скасовано.";

        return RedirectToAction(nameof(My));
    }
}
