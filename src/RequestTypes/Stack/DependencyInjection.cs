﻿using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace MediatorCore.RequestTypes.Stack;

internal static class DependencyInjection
{
    internal static void AddStackHandlers(this IServiceCollection services,
        MediatorCoreOptions options, Assembly[] assemblies)
    {
        var handlerType = typeof(IStackHandler<,>);
        var handlers = AssemblyExtentions.GetAllInherits(assemblies, handlerType);
        foreach (var handler in handlers)
        {
            services.AddStackHandler(options, handler, handlerType);
        }
    }

    internal static void AddStackHandler(this IServiceCollection services,
        MediatorCoreOptions options, Type handler, Type? handlerType = null)
    {
        handlerType ??= typeof(IStackHandler<,>);
        var handlerInterfaces = handler.GetInterfaces()
            .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == handlerType);

        foreach (var item in handlerInterfaces)
        {
            var args = item.GetGenericArguments();
            var messageType = args[0];
            var optionsType = args[1];

            var handlerInterface = typeof(IBaseStackHandler<>)
                .MakeGenericType(messageType);

            if (services.Any(x => x.ServiceType == handlerInterface && x.ImplementationType == handler))
                continue;

            var serviceType = typeof(StackBackgroundService<,>)
                .MakeGenericType(messageType, optionsType);
            var serviceInterface = typeof(IStackBackgroundService<>)
                .MakeGenericType(messageType);

            services.AddSingleton(serviceInterface, serviceType);
            services.AddSingleton(s => (IHostedService)s.GetRequiredService(serviceInterface));
            services.Add(new ServiceDescriptor(handlerInterface,
                handler,
                options.HandlersLifetime));
        }
    }
}