
namespace BeautyBook;

public static class DomainRules
{
    // Перевірка правильності часового інтервалу
    public static bool IsValidInterval(DateTime start, DateTime end)
    {
        return start < end;
    }

    // Перевірка перетину двох записів
    public static bool IntervalsOverlap(
        DateTime startA,
        DateTime endA,
        DateTime startB,
        DateTime endB)
    {
        if (!IsValidInterval(startA, endA) ||
            !IsValidInterval(startB, endB))
        {
            return false;
        }

        return startA < endB && endA > startB;
    }

    // Перевірка правильності часу онлайн-запису
    public static bool IsValidAppointmentTime(
        DateTime startAt,
        DateTime now)
    {
        return startAt > now &&
               startAt.TimeOfDay.Ticks %
               TimeSpan.FromMinutes(15).Ticks == 0;
    }
}
