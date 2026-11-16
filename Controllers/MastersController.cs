
using BeautyBook.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautyBook.Controllers;

public class MastersController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index(
        string? search,
        string? city,
        string? specialty,
        string? sort)
    {
        search = search?.Trim();
        city = city?.Trim();
        specialty = specialty?.Trim();

        var query = db.Masters
            .Include(m => m.Services)
            .Where(m => m.IsActive)
            .AsNoTracking();

        // Пошук за ім'ям, спеціальністю або назвою послуги
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(m =>
                m.FullName.Contains(search) ||
                m.Specialty.Contains(search) ||
                m.Services.Any(s => s.Name.Contains(search)));
        }

        // Фільтрація за містом
        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(m => m.City.Contains(city));
        }

        // Фільтрація за спеціальністю
        if (!string.IsNullOrWhiteSpace(specialty))
        {
            query = query.Where(m => m.Specialty.Contains(specialty));
        }

        // Сортування каталогу
        if (sort == "price_asc" || sort == "price_desc")
        {
            // Завантажуємо відфільтрованих майстрів із послугами.
            // Сортуємо за ціною в C#, оскільки SQLite не підтримує Min(decimal).
            var mastersByPrice = await query.ToListAsync();

            var sortedMasters = sort == "price_asc"
                ? mastersByPrice
                    .OrderBy(m => !m.Services.Any())
                    .ThenBy(m => m.Services.Min(s => s.Price))
                    .ThenBy(m => m.FullName)
                    .ToList()
                : mastersByPrice
                    .OrderBy(m => !m.Services.Any())
                    .ThenByDescending(m => m.Services.Min(s => s.Price))
                    .ThenBy(m => m.FullName)
                    .ToList();

            ViewBag.Search = search;
            ViewBag.City = city;
            ViewBag.Specialty = specialty;
            ViewBag.Sort = sort;

            return View(sortedMasters);
        }

        // Звичайне сортування за ім'ям
        query = query.OrderBy(m => m.FullName);

        ViewBag.Search = search;
        ViewBag.City = city;
        ViewBag.Specialty = specialty;
        ViewBag.Sort = sort;

        var masters = await query.ToListAsync();

        return View(masters);
    }

    public async Task<IActionResult> Details(int id)
    {
        if (id <= 0)
            return NotFound();

        var master = await db.Masters
            .Include(m => m.Services)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

        return master is null ? NotFound() : View(master);
    }
}
