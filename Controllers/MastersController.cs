
using BeautyBook.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautyBook.Controllers;

public class MastersController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index(
        string? search,
        string? city,
        string? specialty)
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

        ViewBag.Search = search;
        ViewBag.City = city;
        ViewBag.Specialty = specialty;

        var masters = await query
            .OrderBy(m => m.FullName)
            .ToListAsync();

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
