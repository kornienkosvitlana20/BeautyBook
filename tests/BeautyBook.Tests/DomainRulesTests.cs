
using Xunit;

namespace BeautyBook.Tests;

public class DomainRulesTests
{
    [Fact]
    public void OverlappingIntervals_ReturnTrue()
    {
        Assert.True(DomainRules.IntervalsOverlap(
            new(2026, 12, 1, 10, 0, 0),
            new(2026, 12, 1, 11, 0, 0),
            new(2026, 12, 1, 10, 30, 0),
            new(2026, 12, 1, 11, 30, 0)));
    }

    [Fact]
    public void AdjacentIntervals_DoNotOverlap()
    {
        Assert.False(DomainRules.IntervalsOverlap(
            new(2026, 12, 1, 10, 0, 0),
            new(2026, 12, 1, 11, 0, 0),
            new(2026, 12, 1, 11, 0, 0),
            new(2026, 12, 1, 12, 0, 0)));
    }

    [Fact]
    public void InvalidInterval_ReturnsFalse()
    {
        Assert.False(DomainRules.IsValidInterval(
            new(2026, 12, 1, 12, 0, 0),
            new(2026, 12, 1, 11, 0, 0)));
    }

    [Fact]
    public void ZeroLengthInterval_ReturnsFalse()
    {
        var time = new DateTime(2026, 12, 1, 10, 0, 0);

        Assert.False(DomainRules.IsValidInterval(time, time));
    }

    [Fact]
    public void AppointmentInPast_IsInvalid()
    {
        Assert.False(DomainRules.IsValidAppointmentTime(
            new(2026, 1, 1, 10, 0, 0),
            new(2026, 1, 1, 11, 0, 0)));
    }

    [Fact]
    public void AppointmentOnFifteenMinuteBoundary_IsValid()
    {
        Assert.True(DomainRules.IsValidAppointmentTime(
            new(2026, 12, 1, 10, 15, 0),
            new(2026, 12, 1, 9, 0, 0)));
    }

    [Fact]
    public void AppointmentWithOddMinute_IsInvalid()
    {
        Assert.False(DomainRules.IsValidAppointmentTime(
            new(2026, 12, 1, 10, 17, 0),
            new(2026, 12, 1, 9, 0, 0)));
    }

    [Fact]
    public void AppointmentWithSeconds_IsInvalid()
    {
        Assert.False(DomainRules.IsValidAppointmentTime(
            new(2026, 12, 1, 10, 15, 30),
            new(2026, 12, 1, 9, 0, 0)));
    }

    [Fact]
    public void AppointmentAtCurrentTime_IsInvalid()
    {
        var time = new DateTime(2026, 12, 1, 10, 15, 0);

        Assert.False(DomainRules.IsValidAppointmentTime(time, time));
    }

    [Fact]
    public void InvalidIntervals_DoNotOverlap()
    {
        Assert.False(DomainRules.IntervalsOverlap(
            new(2026, 12, 1, 12, 0, 0),
            new(2026, 12, 1, 11, 0, 0),
            new(2026, 12, 1, 10, 0, 0),
            new(2026, 12, 1, 13, 0, 0)));
    }
}
