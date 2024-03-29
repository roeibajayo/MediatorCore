using Microsoft.Extensions.Hosting;

namespace MediatorCore.Infrastructure;

internal abstract class IntervalBackgroundService(int msInterval) : IHostedService
{
    internal int Interval { get; } = msInterval;
    internal bool IsRunning { get; private set; }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        RunAsync(cancellationToken);
        return Task.CompletedTask;
    }

    private async void RunAsync(CancellationToken cancellationToken)
    {
        IsRunning = true;
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(Interval));

        while (IsRunning && !cancellationToken.IsCancellationRequested)
        {
            await timer.WaitForNextTickAsync(cancellationToken);
            await OnExecuteAsync(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        IsRunning = false;
        return Task.CompletedTask;
    }

    protected abstract Task OnExecuteAsync(CancellationToken cancellationToken);

}
