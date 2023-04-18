using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MediatorCore.Infrastructure;

internal abstract class IntervalBackgroundService : BackgroundService
{
    protected IntervalBackgroundService(ILogger logger, int msInterval)
    {
        timer = new PeriodicTimer(TimeSpan.FromMilliseconds(msInterval));
        this.logger = logger;
        Interval = msInterval;
    }

    private readonly PeriodicTimer timer;
    private readonly ILogger logger;

    internal int Interval { get; }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                await OnExecuteAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Background service failed");
            }
        }
    }

    protected abstract Task OnExecuteAsync(CancellationToken cancellationToken);
}
