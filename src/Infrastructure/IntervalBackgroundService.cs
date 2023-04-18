using Microsoft.Extensions.Hosting;

namespace MediatorCore.Infrastructure;

internal abstract class IntervalBackgroundService : BackgroundService
{
    protected IntervalBackgroundService(int msInterval)
    {
        timer = new PeriodicTimer(TimeSpan.FromMilliseconds(msInterval));
        Interval = msInterval;
    }

    private readonly PeriodicTimer timer;

    internal int Interval { get; }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await timer.WaitForNextTickAsync(cancellationToken);
            await OnExecuteAsync(cancellationToken);
        }
    }

    protected abstract Task OnExecuteAsync(CancellationToken cancellationToken);
}
