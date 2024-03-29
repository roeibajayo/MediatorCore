using MediatorCore.RequestTypes.Response;

namespace MediatorCore.WebapiTester.GetWeatherForecast;

public record GetWeatherForecastRequest(int From, int To) :
    IResponseMessage<IEnumerable<WeatherForecast>>;
