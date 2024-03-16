﻿using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MediatorCore.RequestTypes.Notification;

internal static class DependencyInjection
{
    internal static void AddNotificationHandlers(this IServiceCollection services,
        MediatorCoreOptions options, Assembly[] assemblies)
    {
        var handlerType = typeof(INotificationHandler<>);
        var handlers = AssemblyExtentions.GetAllInherits(assemblies, handlerType);
        foreach (var handler in handlers)
        {
            var handlerInterfaces = handler.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == handlerType);

            foreach (var handlerInterface in handlerInterfaces)
            {
                if (services.Any(x => x.ServiceType == handlerInterface && x.ImplementationType == handler))
                    continue;

                services.Add(new ServiceDescriptor(handlerInterface,
                    handler,
                    options.HandlersLifetime));
            }
        }
    }
}