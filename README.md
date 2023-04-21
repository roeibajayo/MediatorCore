# MediatorCore

[![NuGet](https://img.shields.io/nuget/dt/MediatorCore.svg)](https://www.nuget.org/packages/MediatorCore) 
[![NuGet](https://img.shields.io/nuget/vpre/MediatorCore.svg)](https://www.nuget.org/packages/MediatorCore)

High-performance yet easy to use mediator pattern and in-process message bus implementation in .NET.

Supports these messages:
- Request/Response (IResponseHandler<TRequest, TResponse>)
- Queue (IQueueHandler<TMessage>)
- Stack (IStackHandler<TMessage>)
- DebounceQueue (IDebounceQueueMessage<TMessage, TDebounceQueueOptions>)
- AccumulatorQueue (IAccumulatorQueueHandler<TMessage, TAccumulatorQueueOptions>)
- FireAndForget (IFireAndForgetHandler<TMessage>)
- BubblingNotification (IBubblingNotificationHandler<TMessage, TBubblingNotificationOptions>)
- ParallelNotification (IParallelNotificationHandler<TMessage>)
- More coming soon..

## Install & Registering:

Install [MediatorCore with NuGet](https://www.nuget.org/packages/MediatorCore):

    Install-Package MediatorCore
    
Or via the .NET Core command line interface:

    dotnet add package MediatR

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
|                            Method |     Mean |   Error |  StdDev | Allocated |
|---------------------------------- |---------:|--------:|--------:|----------:|
|             Response_MediatorCore | 224.4 ns | 3.92 ns | 3.48 ns |     384 B |
|                  Response_MediatR | 234.7 ns | 3.92 ns | 3.67 ns |     464 B |
| ParallelNotification_MediatorCore | 234.0 ns | 4.57 ns | 4.49 ns |     182 B |
|      ParallelNotification_MediatR | 254.2 ns | 3.90 ns | 3.45 ns |     496 B |

## Roadmap:
- Regisaration validations
- More handlers types
- More unitests
- More examples of use

## Contribute
Please feel free to PR. I highly appreciate any contribution!
