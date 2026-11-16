using BeautyBook.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautyBook.Controllers;
public class MastersController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? search, string? city, string? specialty)
    {
        var query = db.Masters.Include(m => m.Services).Where(m => m.IsActive).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(m => m.FullName.Contains(search) || m.Specialty.Contains(search));
        if (!string.IsNullOrWhiteSpace(city)) query = query.Where(m => m.City.Contains(city));
        if (!string.IsNullOrWhiteSpace(specialty)) query = query.Where(m => m.Specialty.Contains(specialty));
        ViewBag.Search = search; ViewBag.City = city; ViewBag.Specialty = specialty;
        return View(await query.OrderBy(m => m.FullName).ToListAsync());
    }
    public async Task<IActionResult> Details(int id)
    {
        var master = await db.Masters.Include(m => m.Services).AsNoTracking().FirstOrDefaultAsync(m => m.Id == id && m.IsActive);
        return master is null ? NotFound() : View(master);
    }
}
