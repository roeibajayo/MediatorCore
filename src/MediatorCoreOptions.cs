using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore;

public class MediatorCoreOptions
{
    /// <summary>
    /// Handlers lifetime. Default value is <see cref="ServiceLifetime.Scoped"/>.
    /// </summary>
    public ServiceLifetime HandlersLifetime { get; set; } = ServiceLifetime.Scoped;
}
