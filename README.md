# MediatorCore

[![NuGet](https://img.shields.io/nuget/dt/MediatorCore.svg)](https://www.nuget.org/packages/MediatorCore) 
[![NuGet](https://img.shields.io/nuget/vpre/MediatorCore.svg)](https://www.nuget.org/packages/MediatorCore)

High-performance yet easy to use mediator pattern and in-process message bus implementation in .NET.


NEW! we now allowing retries!

Supports these messages:
- Request without response `IRequestHandler<TMessage>`
- Request with response `IResponseHandler<TRequest, TResponse>`
- Queue `IQueueHandler<TMessage>`
- Stack `IStackHandler<TMessage>`
- Debounce queue `IDebounceQueueMessage<TMessage, TDebounceQueueOptions>`
- Accumulator queue `IAccumulatorQueueHandler<TMessage, TAccumulatorQueueOptions>`
- Throttling queue `IThrottlingQueueHandler<TMessage, TThrottlingQueueOptions>`
- Bubbling notification `IBubblingNotificationHandler<TMessage, TBubblingNotificationOptions>`
- Parallel notification `IParallelNotificationHandler<TMessage>`
- More coming soon..

## Install & Registering:

Install [MediatorCore with NuGet](https://www.nuget.org/packages/MediatorCore):

    Install-Package MediatorCore
    
Or via the .NET Core command line interface:

    dotnet add package MediatorCore

then register the required services easly:

```csharp
services.AddMediatorCore<Startup>();
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

    public async Task GetResponse()
    {
        var request = new SimpleRequest(1);
        var response = await publisher.GetResponseAsync(request);
        // ... do something with the response
    }
}
```

## Example of creating a Request witout response:

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

    public async Task GetResponse()
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

    public async Task GetResponse()
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

    public Task? HandleException(IEnumerable<LogMessage> items, Exception exception, int reties, Func<Task> retry)
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


## Benchmarks vs MediatR:
|                                        Method |          Mean |       Error |      StdDev | Allocated |
|---------------------------------------------- |--------------:|------------:|------------:|----------:|
|                         Response_MediatorCore | 32,394.786 us | 641.2623 us | 763.3772 us |     996 B |
|                              Response_MediatR | 32,578.816 us | 611.8009 us | 572.2789 us |     812 B |
|                                                                                                       |
|      ParallelNotification_Simple_MediatorCore |      1.638 us |   0.0327 us |   0.0751 us |     848 B |
|           ParallelNotification_Simple_MediatR |      2.470 us |   0.0489 us |   0.1084 us |     872 B |
|                                                                                                       |
| ParallelNotification_LongRunning_MediatorCore |      2.848 us |   0.0569 us |   0.0919 us |    1128 B |
|      ParallelNotification_LongRunning_MediatR |      3.205 us |   0.0634 us |   0.0846 us |    1160 B |

## Roadmap:
- Documentation
- Registration validations
- More handlers types
- More unitests
- More examples of use (check out the Unitests for now))

## Contribute
Please feel free to PR. I highly appreciate any contribution!
