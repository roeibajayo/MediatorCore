using MediatR;

namespace MediatorCore.Benchmarks.RequestTypes;

public record SimpleResponse(bool Success);
public record SimpleResponseMessage(int Id) :
    IRequest<SimpleResponse>,
    IResponseMessage<SimpleResponse>;
public class SimpleResponseMessageHandler :
    IRequestHandler<SimpleResponseMessage, SimpleResponse>,
    IResponseHandler<SimpleResponseMessage, SimpleResponse>
{
    public Task<SimpleResponse> Handle(SimpleResponseMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new SimpleResponse(true));
    }

    public Task<SimpleResponse> HandleAsync(SimpleResponseMessage message, CancellationToken cancellationToken)
    {
        return Task.FromResult(new SimpleResponse(true));
    }
}
