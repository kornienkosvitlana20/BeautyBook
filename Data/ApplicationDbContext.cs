using BeautyBook.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BeautyBook.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<AppUser>(options)
{
    public DbSet<Master> Masters => Set<Master>();
    public DbSet<BeautyService> BeautyServices => Set<BeautyService>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Master>().HasIndex(x => new { x.City, x.Specialty });
        builder.Entity<BeautyService>().HasOne(x => x.Master).WithMany(x => x.Services)
            .HasForeignKey(x => x.MasterId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Booking>().HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Booking>().HasOne(x => x.Master).WithMany().HasForeignKey(x => x.MasterId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Booking>().HasOne(x => x.BeautyService).WithMany().HasForeignKey(x => x.BeautyServiceId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Booking>().HasIndex(x => new { x.MasterId, x.StartAt, x.EndAt });
        builder.Entity<Booking>().Property(x => x.Status).HasDefaultValue("Confirmed");
    }
}
