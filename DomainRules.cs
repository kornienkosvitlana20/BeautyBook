namespace BeautyBook;
public static class DomainRules
{
    public static bool IntervalsOverlap(DateTime startA, DateTime endA, DateTime startB, DateTime endB)
        => startA < endB && endA > startB;

    public static bool IsValidAppointmentTime(DateTime startAt, DateTime now)
        => startAt > now && startAt.Minute % 15 == 0 && startAt.Second == 0;
}
