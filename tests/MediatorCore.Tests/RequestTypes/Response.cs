using MediatorCore.RequestTypes.Response;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorCore.Tests.RequestTypes;

public class Response : BaseUnitTest
{
    [Fact]
    public async Task BasicResponse_ReturnSuccess()
    {
        //Arrange
        var publisher = ServiceProvider.GetService<IPublisher>()!;

        //Act
        var response = await publisher.GetResponseAsync(new SimpleResponseMessage(1));

        //Assert
        Assert.NotNull(response);
        Assert.True(response.Success);
    }
}

public record SimpleResponse(bool Success);
public record SimpleResponseMessage(int Id) : IResponseMessage<SimpleResponse>;
public class SimpleResponseMessageHandler : IResponseHandler<SimpleResponseMessage, SimpleResponse>
{
    public Task<SimpleResponse> HandleAsync(SimpleResponseMessage message, CancellationToken cancellationToken)
    {
        return Task.FromResult(new SimpleResponse(true));
    }
}