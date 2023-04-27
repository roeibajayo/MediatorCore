# MediatorCore

[![NuGet](https://img.shields.io/nuget/dt/MediatorCore.svg)](https://www.nuget.org/packages/MediatorCore) 
[![NuGet](https://img.shields.io/nuget/vpre/MediatorCore.svg)](https://www.nuget.org/packages/MediatorCore)

High-performance yet easy to use mediator pattern and in-process message bus implementation in .NET.


NEW! we now allowing retries!

Supports these messages:
- Request without response `IRequestHandler<TMessage>`
- Request with response `IResponseHandler<TRequest, TResponse>`
- Queue `IQueueHandler<TMessage, TQueueHandlerOptions>`
- Stack `IStackHandler<TMessage, TStackHandlerOptions>`
- Debounce queue `IDebounceQueueMessage<TMessage, TDebounceQueueOptions>`
- Accumulator queue `IAccumulatorQueueHandler<TMessage, TAccumulatorQueueOptions>`
- Throttling queue `IThrottlingQueueHandler<TMessage, TThrottlingQueueOptions>`
- Bubbling notification `IBubblingNotificationHandler<TMessage, TBubblingNotificationOptions>`
- Parallel notification `IParallelNotificationHandler<TMessage>`

## Install & Registering:

Install [MediatorCore with NuGet](https://www.nuget.org/packages/MediatorCore):

    Install-Package MediatorCore
    
Or via the .NET Core command line interface:

    dotnet add package MediatorCore

then register the required services easly:

```csharp
services.AddMediatorCore();
```

## Example of creating a Request/Response:

```csharp
// the response:
public record SimpleResponse(bool Success);

// the request (message):
public record SimpleRequest(int Id) : IResponseMessage<SimpleResponse>;

// the handler:
public class SimpleResponseMessageHandler : IResponseHandler<SimpleRequest, SimpleResponse>
{
    public async Task<SimpleResponse> HandleAsync(SimpleRequest message, CancellationToken cancellationToken)
    {
        var response = new SimpleResponse(true);
        await Task.Delay(200, cancellationToken); // simulate some work
        return response;
    }
}
```

then call the request:
```csharp
public class Example
{
    private readonly IPublisher publisher;

    public Example(IPublisher publisher)
    {
        this.publisher = publisher;
    }

    public async Task GetResponseFromHandlerAsync()
    {
        var request = new SimpleRequest(1);
        var response = await publisher.GetResponseAsync(request);
        // do something with the response ...
    }
}
```

## Example of creating a Request without response:

```csharp
// the message:
public record SimpleRequest(int Id) : IRequestMessage;

// the handler:
public class SimpleRequestMessageHandler : IRequestHandler<SimpleRequest>
{
    public async Task HandleAsync(SimpleRequest message, CancellationToken cancellationToken)
    {
        await Task.Delay(300, cancellationToken); // simulate some work
        throw new Exception("hello world");
    }
 
    public async Task HandleException(SimpleRequest message,
        Exception exception,
        int reties, Func<Task> retry,
        CancellationToken cancellationToken)
    {
        // you can just ignore the exception and continue:
        // return default;

        // handle the exception..

        if (reties == 3)
            return;

        await retry();
    }
}
```

then call the request:
```csharp
public class Example
{
    private readonly IPublisher publisher;

    public Example(IPublisher publisher)
    {
        this.publisher = publisher;
    }

    public async Task ExecuteRemoteCodeAsync()
    {
        var message = new SimpleRequest(1);
        await publisher.PublishAsync(message);

        // you can also use this as fire and forget request:
        // publisher.Publish(message);
    }
}
```

## Example of creating a Bubbling notification

```csharp
// the message:
public record SharedBubblingNotificationMessage(string Id, bool Bubble) : IBubblingNotificationMessage;

// first handler options:
public class BubblingNotification1Options
    : IBubblingNotificationOptions
{
    public int Sort => 1;
}

// first handler:
public class BubblingNotification1Handler : IBubblingNotificationHandler<SharedBubblingNotificationMessage, BubblingNotification1Options>
{
    public readonly ILogger logger;

    public BubblingNotification1Handler(ILogger logger)
    {
        this.logger = logger;
    }

    public Task<bool> HandleAsync(SharedBubblingNotificationMessage message, CancellationToken cancellationToken)
    {
        logger.LogDebug("BubblingNotification1Message: " + message.Id);
        return Task.FromResult(message.Bubble);
    }
}

// second handler options:
public class BubblingNotification2Options
    : IBubblingNotificationOptions
{
    public int Sort => 2;
}

// second handler:
public class BubblingNotification2Handler : IBubblingNotificationHandler<SharedBubblingNotificationMessage, BubblingNotification2Options>
{
    public readonly ILogger logger;

    public BubblingNotification2Handler(ILogger logger)
    {
        this.logger = logger;
    }

    public Task<bool> HandleAsync(SharedBubblingNotificationMessage message, CancellationToken cancellationToken)
    {
        logger.LogDebug("BubblingNotification2Message: " + message.Id);
        return Task.FromResult(true);
    }
}
```

then call the event:
```csharp
public class Example
{
    private readonly IPublisher publisher;

    public Example(IPublisher publisher)
    {
        this.publisher = publisher;
    }

    public async Task SomeAction()
    {
        var message = new SharedBubblingNotificationMessage(1, true);
        await publisher.PublishAsync(message);

        // you can also use this as fire and forget notification:
        // publisher.Publish(message);
    }
}
```

## Example of creating a Accumulator queue

```csharp
// the options of the queue:
public class LogsAccumulatorQueueOptions :
    IAccumulatorQueueOptions
{
    public int MsInterval => 60 * 1000;
    public int? MaxItemsOnDequeue => 100;
    public int? MaxItemsStored => 1000;
    public MaxItemsStoredBehaviors? MaxItemsBehavior => MaxItemsStoredBehaviors.ThrowExceptionOnAdd;
}

// the message:
public record LogMessage(DateTimeOffest Date, string Message) : IAccumulatorQueueMessage;

// the handler:
public class LogsAccumulatorQueueMessageHandler :
    IAccumulatorQueueHandler<LogMessage, LogsAccumulatorQueueOptions>
{
    public readonly ILogger logger;

    public LogsAccumulatorQueueMessageHandler(ILogger logger)
    {
        this.logger = logger;
    }

    public Task HandleAsync(IEnumerable<LogMessage> messages)
    {
        foreach (var message in messages)
        {
            logger.LogDebug("Log message: " + message.Message);
        }
        return Task.CompletedTask;
    }

    public Task? HandleException(IEnumerable<LogMessage> items, 
        Exception exception, 
        int reties, 
        Func<Task> retry)
    {
        return default;
    }
}
```

then enqueue a log message:
```csharp
public class Example
{
    private readonly IPublisher publisher;

    public Example(IPublisher publisher)
    {
        this.publisher = publisher;
    }
    
    public async Task SomeAction()
    {
        var log = new LogMessage(DateTimeOffest.UtcNow, "hello world from SomeAction");
        publisher.Publish(log);
    }
    
    public async Task AnotherAction()
    {
        var log = new LogMessage(DateTimeOffest.UtcNow, "hello world from AnotherAction");
        publisher.Publish(log);
    }
}
```


## Benchmarks MediatorCore (1.3.0) vs MediatR (12.0.1):
|                                        Method |       Mean |       Error |    StdDev | Allocated |
|---------------------------------------------- |-----------:|------------:|----------:|----------:|
|                         Response_MediatorCore |   151.4 ns |    68.35 ns |   3.75 ns |     336 B |
|                              Response_MediatR |   212.0 ns |   926.96 ns |  50.81 ns |     408 B |
|                                                                                                  |
|      ParallelNotification_Simple_MediatorCore | 1,680.1 ns | 2,093.40 ns | 114.75 ns |     872 B |
|           ParallelNotification_Simple_MediatR | 2,497.9 ns | 2,837.47 ns | 155.53 ns |     872 B |
|                                                                                                  |
| ParallelNotification_LongRunning_MediatorCore | 2,884.0 ns | 1,366.19 ns |  74.89 ns |    1136 B |
|      ParallelNotification_LongRunning_MediatR | 3,202.3 ns |   921.75 ns |  50.52 ns |    1160 B |
|                                                                                                  |
|                                 InsertToQueue |   119.7 ns |    99.76 ns |   5.47 ns |      48 B |
|                                                                                                  |
|                                 InsertToStack |   281.0 ns | 1,199.47 ns |  65.75 ns |      64 B |

## Roadmap:
- More handlers types
- More unitests
- More examples of use (check out the Unitests for now)

## Contribute
Please feel free to PR. I highly appreciate any contribution!
