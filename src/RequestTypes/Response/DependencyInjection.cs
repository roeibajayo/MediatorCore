using MediatorCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MediatorCore.RequestTypes.Response;

internal static class DependencyInjection
{
    internal static IDictionary<Type, object> responseHandlers =
        new Dictionary<Type, object>();

    internal static void AddResponseHandlers(this IServiceCollection services,
        MediatorCoreOptions options, Assembly[] assemblies)
    {
        var handlerType = typeof(IResponseHandler<,>);
        var handlers = AssemblyExtentions.GetAllInherits(assemblies, handlerType);
        foreach (var handler in handlers)
        {
            var handlerInterfaces = handler.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == handlerType);

            foreach (var item in handlerInterfaces)
            {
                var types = item.GetGenericArguments();
                var message = types[0];
                var response = types[1];
                var handlerInterface = handlerType.MakeGenericType(types);

                if (services.Any(x => x.ServiceType == handlerInterface && x.ImplementationType == handler))
                    continue;

                services.Add(new ServiceDescriptor(handlerInterface,
                    handler,
                    options.HandlersLifetime));

                var wrapperType = typeof(ResponseHandlerWrapper<,>).MakeGenericType(message, response);
                var wrapper = Activator.CreateInstance(wrapperType);
                responseHandlers!.TryAdd(message, wrapper);
            }
        }
    }
}

internal abstract class BaseResponseHandlerWrapper<TResponse>
{
    internal abstract Task<TResponse> HandleAsync(IResponseMessage<TResponse> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

internal class ResponseHandlerWrapper<TRequest, TResponse> :
    BaseResponseHandlerWrapper<TResponse>
    where TRequest : IResponseMessage<TResponse>
{
    internal override Task<TResponse> HandleAsync(IResponseMessage<TResponse> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        return serviceProvider.GetService<IResponseHandler<TRequest, TResponse>>()!
            .HandleAsync((TRequest)request, cancellationToken);
    }
}