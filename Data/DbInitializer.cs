using BeautyBook.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BeautyBook.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Customer", "Master", "Admin" })
            if (!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole(role));
        if (!await db.Masters.AnyAsync())
        {
            var m1 = new Master { FullName = "Анна Коваленко", Specialty = "Перукар", City = "Київ", Description = "Жіночі та чоловічі стрижки, фарбування." };
            var m2 = new Master { FullName = "Марія Петренко", Specialty = "Майстер манікюру", City = "Львів", Description = "Манікюр, покриття гель-лаком та дизайн." };
            db.Masters.AddRange(m1, m2);
            await db.SaveChangesAsync();
            db.BeautyServices.AddRange(
                new BeautyService { MasterId = m1.Id, Name = "Стрижка", Price = 500, DurationMinutes = 60 },
                new BeautyService { MasterId = m1.Id, Name = "Фарбування", Price = 1800, DurationMinutes = 120 },
                new BeautyService { MasterId = m2.Id, Name = "Манікюр з покриттям", Price = 700, DurationMinutes = 90 });
            await db.SaveChangesAsync();
        }
    }
}
