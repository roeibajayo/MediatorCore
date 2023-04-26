using MediatorCore.RequestTypes.AccumulatorQueue;
using MediatorCore.RequestTypes.Response;
using MediatorCore.WebapiTester;
using Microsoft.AspNetCore.Mvc;

namespace MediatorCore.WebapiTester.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IPublisher publisher;

        public WeatherForecastController(IPublisher publisher)
        {
            this.publisher = publisher;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            var request = new GetWeatherForecastRequest(1, 5);
            publisher.Publish(request);
            return await publisher.GetResponseAsync(request);
        }
    }
}

public record GetWeatherForecastRequest(int from, int to) : IResponseMessage<IEnumerable<WeatherForecast>>, IAccumulatorQueueMessage;
public class GetWeatherForecastHandler : IResponseHandler<GetWeatherForecastRequest, IEnumerable<WeatherForecast>>
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public Task<IEnumerable<WeatherForecast>> HandleAsync(GetWeatherForecastRequest message, CancellationToken cancellationToken)
    {
        return Task.FromResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }));
    }
}

public class GetWeatherForecast2HandlerOptions : IAccumulatorQueueOptions
{
    public int MsInterval => 100;

    public int? MaxItemsOnDequeue => default;

    public int? MaxItemsStored => default;

    public MaxItemsStoredBehaviors? MaxItemsBehavior => default;
}
public class GetWeatherForecast2Handler : IAccumulatorQueueHandler<GetWeatherForecastRequest, GetWeatherForecast2HandlerOptions>
{
    private readonly ILogger<GetWeatherForecast2Handler> logger;

    public GetWeatherForecast2Handler(ILogger<GetWeatherForecast2Handler> logger)
    {
        this.logger = logger;
    }

    public Task HandleAsync(IEnumerable<GetWeatherForecastRequest> items)
    {
        logger.LogInformation("GetWeatherForecastRequest just logged from queue");
        return Task.CompletedTask;
    }

    public Task? HandleExceptionAsync(IEnumerable<GetWeatherForecastRequest> items, Exception exception, int retries, Func<Task> retry)
    {
        return default;
    }
}