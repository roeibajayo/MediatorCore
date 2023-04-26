using Microsoft.Extensions.Hosting;

namespace MediatorCore.Infrastructure;

internal abstract class IntervalBackgroundService : IHostedService
{
    protected IntervalBackgroundService(int msInterval)
    {
        timer = new PeriodicTimer(TimeSpan.FromMilliseconds(msInterval));
        Interval = msInterval;
    }

    private readonly PeriodicTimer timer;

    internal int Interval { get; }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        RunAsync(cancellationToken);
        return Task.CompletedTask;
    }

    private async void RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await timer.WaitForNextTickAsync(cancellationToken);
            await OnExecuteAsync(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        timer.Dispose();
        return Task.CompletedTask;
    }

    protected abstract Task OnExecuteAsync(CancellationToken cancellationToken);

}
