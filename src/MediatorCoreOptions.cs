using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.Publisher;

public class MediatorCoreOptions
{
    internal static MediatorCoreOptions instance;

    /// <summary>
    /// Handlers lifetime. Default value is <see cref="ServiceLifetime.Transient"/>
    /// </summary>
    public ServiceLifetime HandlersLifetime { get; set; } = ServiceLifetime.Transient;
}
