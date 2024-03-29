using MediatorCore.RequestTypes.Response;
using MediatorCore.WebapiTester.LogsHandler;

namespace MediatorCore.WebapiTester.GetWeatherForecast;

public class GetWeatherForecastHandler : IResponseHandler<GetWeatherForecastRequest, IEnumerable<WeatherForecast>>
{
    private readonly IPublisher publisher;

    public GetWeatherForecastHandler(IPublisher publisher)
    {
        this.publisher = publisher;
    }

    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public Task<IEnumerable<WeatherForecast>> HandleAsync(GetWeatherForecastRequest message, CancellationToken cancellationToken)
    {
        publisher.Publish(new LogMessage("GetWeatherForecastHandler executed"));

        return Task.FromResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }));
    }
}
