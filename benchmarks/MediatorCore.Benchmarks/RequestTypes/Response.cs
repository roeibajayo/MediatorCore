using MediatorCore.RequestTypes.Response;
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
    public async Task<SimpleResponse> Handle(SimpleResponseMessage request, CancellationToken cancellationToken)
    {
        await Task.Delay(30);
        return new SimpleResponse(true);
    }

    public async Task<SimpleResponse> HandleAsync(SimpleResponseMessage message, CancellationToken cancellationToken)
    {
        await Task.Delay(30);
        return new SimpleResponse(true);
    }
}
