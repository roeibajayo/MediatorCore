# MediatorCore

[![NuGet](https://img.shields.io/nuget/dt/MediatorCore.svg)](https://www.nuget.org/packages/MediatorCore) 
[![NuGet](https://img.shields.io/nuget/vpre/MediatorCore.svg)](https://www.nuget.org/packages/MediatorCore)

High-performance yet easy to use mediator pattern and in-process message bus implementation in .NET.
NEW! we now allowing retries!

Supports these messages:
- Request/Response (IResponseHandler<TRequest, TResponse>)
- Queue (IQueueHandler<TMessage>)
- Stack (IStackHandler<TMessage>)
- DebounceQueue (IDebounceQueueMessage<TMessage, TDebounceQueueOptions>)
- AccumulatorQueue (IAccumulatorQueueHandler<TMessage, TAccumulatorQueueOptions>)
- ThrottlingQueue (IThrottlingQueueHandler<TMessage, TThrottlingQueueOptions>)
- FireAndForget (IFireAndForgetHandler<TMessage>)
- BubblingNotification (IBubblingNotificationHandler<TMessage, TBubblingNotificationOptions>)
- ParallelNotification (IParallelNotificationHandler<TMessage>)
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

easy as these few lines:
```csharp
public record SimpleResponse(bool Success);
public record SimpleRequest(int Id) : IResponseMessage<SimpleResponse>;
public class SimpleResponseMessageHandler : IResponseHandler<SimpleRequest, SimpleResponse>
{
    public Task<SimpleResponse> HandleAsync(SimpleRequest message, CancellationToken cancellationToken)
    {
        return Task.FromResult(new SimpleResponse(true));
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
        var response = await publisher.GetResponseAsync(
          new SimpleRequest(1));
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
- Registration validations
- More handlers types
- More unitests
- More examples of use

## Contribute
Please feel free to PR. I highly appreciate any contribution!
