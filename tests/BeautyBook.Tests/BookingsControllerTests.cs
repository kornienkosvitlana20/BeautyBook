using System.Security.Claims;
using BeautyBook.Controllers;
using BeautyBook.Data;
using BeautyBook.Models;
using BeautyBook.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BeautyBook.Tests;

/// <summary>
/// Тести BookingsController на окремій тестовій базі SQLite in-memory.
/// Основна база beautybook.db не використовується.
/// </summary>
public sealed class BookingsControllerTests : IDisposable
{
    private const string Alice = "user-alice";
    private const string Bob = "user-bob";

    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _services;
    private readonly ApplicationDbContext _db;

    private readonly int _master1Id;
    private readonly int _master2Id;
    private readonly int _haircutId;      // послуга майстра 1, 60 хв
    private readonly int _manicureId;     // послуга майстра 2, 90 хв

    public BookingsControllerTests()
    {
        // База живе, поки відкрите з'єднання
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(_connection));
        services.AddIdentityCore<AppUser>().AddEntityFrameworkStores<ApplicationDbContext>();
        _services = services.BuildServiceProvider();

        _db = _services.GetRequiredService<ApplicationDbContext>();
        _db.Database.EnsureCreated();

        _db.Users.AddRange(
            new AppUser { Id = Alice, UserName = "alice@test.ua", Email = "alice@test.ua", FullName = "Аліса" },
            new AppUser { Id = Bob, UserName = "bob@test.ua", Email = "bob@test.ua", FullName = "Богдан" });

        var m1 = new Master { FullName = "Анна Коваленко", Specialty = "Перукар", City = "Київ" };
        var m2 = new Master { FullName = "Марія Петренко", Specialty = "Манікюр", City = "Львів" };
        _db.Masters.AddRange(m1, m2);
        _db.SaveChanges();

        var haircut = new BeautyService { MasterId = m1.Id, Name = "Стрижка", Price = 500, DurationMinutes = 60 };
        var manicure = new BeautyService { MasterId = m2.Id, Name = "Манікюр", Price = 700, DurationMinutes = 90 };
        _db.BeautyServices.AddRange(haircut, manicure);
        _db.SaveChanges();

        _master1Id = m1.Id;
        _master2Id = m2.Id;
        _haircutId = haircut.Id;
        _manicureId = manicure.Id;
    }

    public void Dispose()
    {
        _services.Dispose();
        _connection.Dispose();
    }

    private static DateTime Tomorrow => DateTime.Today.AddDays(1);

    private BookingsController CreateController(string userId)
    {
        var controller = new BookingsController(_db, _services.GetRequiredService<UserManager<AppUser>>());
        var http = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "Test"))
        };
        controller.ControllerContext = new ControllerContext { HttpContext = http };
        controller.TempData = new TempDataDictionary(http, new InMemoryTempDataProvider());
        return controller;
    }

    private Booking AddBooking(string customerId, DateTime start, string status = "Confirmed")
    {
        var booking = new Booking
        {
            CustomerId = customerId, MasterId = _master1Id, BeautyServiceId = _haircutId,
            StartAt = start, EndAt = start.AddMinutes(60), Status = status
        };
        _db.Bookings.Add(booking);
        _db.SaveChanges();
        _db.ChangeTracker.Clear();
        return booking;
    }

    private static BookingViewModel Form(int masterId, int serviceId, DateTime date, TimeSpan time)
        => new() { MasterId = masterId, BeautyServiceId = serviceId, Date = date, Time = time };

    private static string Errors(BookingsController c)
        => string.Join(" ", c.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

    // ---------- Створення запису ----------

    [Fact]
    public async Task Create_ValidData_SavesBookingAndRedirectsToMy()
    {
        var c = CreateController(Alice);

        var result = await c.Create(Form(_master1Id, _haircutId, Tomorrow, new TimeSpan(10, 15, 0)));

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(BookingsController.My), redirect.ActionName);
        var saved = Assert.Single(_db.Bookings.AsNoTracking());
        Assert.Equal(Alice, saved.CustomerId);
        Assert.Equal("Confirmed", saved.Status);
        Assert.Equal(Tomorrow.AddHours(10).AddMinutes(15), saved.StartAt);
        Assert.Equal(saved.StartAt.AddMinutes(60), saved.EndAt);
        Assert.Contains("Запис створено", c.TempData["Success"] as string);
    }

    [Fact]
    public async Task Create_DateInPast_IsRejected()
    {
        var c = CreateController(Alice);

        var result = await c.Create(Form(_master1Id, _haircutId, DateTime.Today.AddDays(-1), new TimeSpan(10, 0, 0)));

        Assert.IsType<ViewResult>(result);
        Assert.False(c.ModelState.IsValid);
        Assert.Contains("минулу дату", Errors(c));
        Assert.Empty(_db.Bookings);
    }

    [Fact]
    public async Task Create_TimeNotMultipleOf15Minutes_IsRejected()
    {
        var c = CreateController(Alice);

        var result = await c.Create(Form(_master1Id, _haircutId, Tomorrow, new TimeSpan(10, 17, 0)));

        Assert.IsType<ViewResult>(result);
        Assert.Contains("кратним 15 хвилинам", Errors(c));
        Assert.Empty(_db.Bookings);
    }

    [Fact]
    public async Task Create_ServiceOfAnotherMaster_IsRejected()
    {
        var c = CreateController(Alice);

        // Майстер 1, але послуга майстра 2
        var result = await c.Create(Form(_master1Id, _manicureId, Tomorrow, new TimeSpan(10, 0, 0)));

        Assert.IsType<ViewResult>(result);
        Assert.Contains("не належить цьому майстру", Errors(c));
        Assert.Empty(_db.Bookings);
    }

    [Fact]
    public async Task Create_UnknownMaster_ReturnsNotFound()
    {
        var c = CreateController(Alice);

        var result = await c.Create(Form(9999, _haircutId, Tomorrow, new TimeSpan(10, 0, 0)));

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_OverlappingConfirmedBooking_IsRejected()
    {
        AddBooking(Bob, Tomorrow.AddHours(10));            // 10:00–11:00 зайнято
        var c = CreateController(Alice);

        var result = await c.Create(Form(_master1Id, _haircutId, Tomorrow, new TimeSpan(10, 30, 0)));

        Assert.IsType<ViewResult>(result);
        Assert.Contains("уже зайнятий", Errors(c));
        Assert.Equal(1, _db.Bookings.Count());
    }

    [Fact]
    public async Task Create_SameTimeAsCancelledBooking_IsAllowed()
    {
        AddBooking(Bob, Tomorrow.AddHours(10), status: "Cancelled");
        var c = CreateController(Alice);

        var result = await c.Create(Form(_master1Id, _haircutId, Tomorrow, new TimeSpan(10, 0, 0)));

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(2, _db.Bookings.Count());
    }

    // ---------- Мої записи та захист чужих записів ----------

    [Fact]
    public void Controller_RequiresAuthorization()
    {
        Assert.NotNull(typeof(BookingsController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).SingleOrDefault());
    }

    [Fact]
    public async Task My_ShowsOnlyCurrentUsersBookings()
    {
        var mine = AddBooking(Alice, Tomorrow.AddHours(10));
        AddBooking(Bob, Tomorrow.AddHours(12));
        var c = CreateController(Alice);

        var result = await c.My();

        var view = Assert.IsType<ViewResult>(result);
        var items = Assert.IsAssignableFrom<IEnumerable<Booking>>(view.Model).ToList();
        var only = Assert.Single(items);
        Assert.Equal(mine.Id, only.Id);
        Assert.All(items, b => Assert.Equal(Alice, b.CustomerId));
    }

    [Fact]
    public async Task Cancel_SomeoneElsesBooking_ReturnsNotFoundAndKeepsStatus()
    {
        var bobs = AddBooking(Bob, Tomorrow.AddHours(10));
        var c = CreateController(Alice);

        var result = await c.Cancel(bobs.Id);

        Assert.IsType<NotFoundResult>(result);
        Assert.Equal("Confirmed", _db.Bookings.AsNoTracking().Single(b => b.Id == bobs.Id).Status);
    }

    [Fact]
    public async Task Cancel_OwnFutureBooking_SetsStatusCancelledWithoutDeleting()
    {
        var mine = AddBooking(Alice, Tomorrow.AddHours(10));
        var c = CreateController(Alice);

        var result = await c.Cancel(mine.Id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(BookingsController.My), redirect.ActionName);
        var stored = _db.Bookings.AsNoTracking().Single(b => b.Id == mine.Id);   // запис не видалено
        Assert.Equal("Cancelled", stored.Status);
        Assert.Equal("Запис скасовано.", c.TempData["Success"]);
    }

    [Fact]
    public async Task Cancel_PastBooking_IsNotAllowed()
    {
        var past = AddBooking(Alice, DateTime.Today.AddDays(-1).AddHours(10));
        var c = CreateController(Alice);

        var result = await c.Cancel(past.Id);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Confirmed", _db.Bookings.AsNoTracking().Single(b => b.Id == past.Id).Status);
        Assert.Contains("уже відбувся", c.TempData["Error"] as string);
    }

    private sealed class InMemoryTempDataProvider : ITempDataProvider
    {
        private IDictionary<string, object> _data = new Dictionary<string, object>();
        public IDictionary<string, object> LoadTempData(HttpContext context) => _data;
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) => _data = values;
    }
}
