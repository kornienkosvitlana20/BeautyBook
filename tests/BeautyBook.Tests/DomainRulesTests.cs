using Xunit;
namespace BeautyBook.Tests;
public class DomainRulesTests
{
    [Fact] public void OverlappingIntervals_ReturnTrue() => Assert.True(DomainRules.IntervalsOverlap(new(2026,12,1,10,0,0),new(2026,12,1,11,0,0),new(2026,12,1,10,30,0),new(2026,12,1,11,30,0)));
    [Fact] public void AdjacentIntervals_DoNotOverlap() => Assert.False(DomainRules.IntervalsOverlap(new(2026,12,1,10,0,0),new(2026,12,1,11,0,0),new(2026,12,1,11,0,0),new(2026,12,1,12,0,0)));
    [Fact] public void AppointmentInPast_IsInvalid() => Assert.False(DomainRules.IsValidAppointmentTime(new(2026,1,1,10,0,0),new(2026,1,1,11,0,0)));
    [Fact] public void AppointmentOnFifteenMinuteBoundary_IsValid() => Assert.True(DomainRules.IsValidAppointmentTime(new(2026,12,1,10,15,0),new(2026,12,1,9,0,0)));
    [Fact] public void AppointmentWithOddMinute_IsInvalid() => Assert.False(DomainRules.IsValidAppointmentTime(new(2026,12,1,10,17,0),new(2026,12,1,9,0,0)));
}
