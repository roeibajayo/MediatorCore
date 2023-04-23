using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.RequestTypes.FireAndForget;

internal static class DependencyInjection
{
    internal static void AddFireAndForgetHandlers<TMarker>(this IServiceCollection services)
    {
        var handlers = AssemblyExtentions.GetAllInheritsFromMarker(typeof(IFireAndForgetHandler<>), typeof(TMarker));
        foreach (var handler in handlers)
        {
            var messageType = handler.GetInterfaces()
                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IFireAndForgetHandler<>))
                .GetGenericArguments()
                .First();

            var handlerInterface = typeof(IFireAndForgetHandler<>).MakeGenericType(messageType);
            services.Add(new ServiceDescriptor(handlerInterface,
                handler,
                MediatorCoreOptions.instance.HandlersLifetime));
        }
    }
}