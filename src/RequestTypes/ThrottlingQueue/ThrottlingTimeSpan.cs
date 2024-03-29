namespace MediatorCore;

/// <summary>
/// Represents the throttling window info.
/// </summary>
/// <param name="Duration">Duration of throttling window.</param>
/// <param name="PermitLimit">Maximum number of permit counters that can be allowed in a window.</param>
/// <param name="Fixed">Determines whether the throttling is fixed to round time (eg. 12:00) or not.</param>
public sealed record ThrottlingWindow(TimeSpan Duration, int PermitLimit, bool Fixed = false)
{
    internal int PermitLimit { get; } = PermitLimit >= 0 ?
        PermitLimit :
        throw new ArgumentException("PermitLimit cannot be negative.", nameof(PermitLimit));

    internal DateTimeOffset GetLastStart() => GetLastStart(DateTimeOffset.Now);
    internal DateTimeOffset GetLastStart(DateTimeOffset relativeTo)
    {
        if (Fixed)
        {
            if (Duration.TotalHours < 1)
                return new DateTime(relativeTo.Year, relativeTo.Month, relativeTo.Day, relativeTo.Hour, relativeTo.Minute, 0);

            if (Duration.TotalDays < 1)
                return new DateTime(relativeTo.Year, relativeTo.Month, relativeTo.Day, relativeTo.Hour, 0, 0);

            if (Duration.TotalDays < 7)
                return new DateTime(relativeTo.Year, relativeTo.Month, relativeTo.Day);

            if (Duration.TotalDays >= 7)
            {
                var result = new DateTime(relativeTo.Year, relativeTo.Month, relativeTo.Day);
                while (result.DayOfWeek != DayOfWeek.Sunday)
                {
                    result = result.AddDays(-1);
                }
                return result;
            }
        }
        return relativeTo - Duration;
    }
    internal DateTimeOffset GetLastEnd() => GetLastEnd(DateTimeOffset.Now);
    internal DateTimeOffset GetLastEnd(DateTimeOffset relativeTo)
    {
        return !Fixed ? relativeTo : GetLastStart(relativeTo) + Duration;
    }
}