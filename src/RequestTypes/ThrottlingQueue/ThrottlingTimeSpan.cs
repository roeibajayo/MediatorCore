namespace MediatorCore.RequestTypes.ThrottlingQueue;

/// <summary>
/// Represents the throttling info
/// </summary>
/// <param name="TimeSpan">Duration of throttling</param>
/// <param name="Executes">The number of times the action was executed during the throttling</param>
/// <param name="Fixed">Determines whether the throttling is fixed or sliding</param>
public sealed record ThrottlingTimeSpan(TimeSpan TimeSpan, int Executes, bool Fixed = false)
{
    public DateTimeOffset GetLastStart() => GetLastStart(DateTimeOffset.Now);
    public DateTimeOffset GetLastStart(DateTimeOffset relativeTo)
    {
        if (Fixed)
        {
            if (TimeSpan.TotalHours < 1)
                return new DateTime(relativeTo.Year, relativeTo.Month, relativeTo.Day, relativeTo.Hour, relativeTo.Minute, 0);

            if (TimeSpan.TotalDays < 1)
                return new DateTime(relativeTo.Year, relativeTo.Month, relativeTo.Day, relativeTo.Hour, 0, 0);

            if (TimeSpan.TotalDays < 7)
                return new DateTime(relativeTo.Year, relativeTo.Month, relativeTo.Day);

            if (TimeSpan.TotalDays >= 7)
            {
                var result = new DateTime(relativeTo.Year, relativeTo.Month, relativeTo.Day);
                while (result.DayOfWeek != DayOfWeek.Sunday)
                {
                    result = result.AddDays(-1);
                }
                return result;
            }
        }
        return relativeTo - TimeSpan;
    }
    public DateTimeOffset GetLastEnd() => GetLastEnd(DateTimeOffset.Now);
    public DateTimeOffset GetLastEnd(DateTimeOffset relativeTo)
    {
        return !Fixed ? relativeTo : GetLastStart(relativeTo) + TimeSpan;
    }
}