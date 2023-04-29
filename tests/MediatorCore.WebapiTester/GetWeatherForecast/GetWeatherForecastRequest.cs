namespace MediatorCore.WebapiTester.GetWeatherForcast;

public record GetWeatherForecastRequest(int From, int To) : 
    IResponseMessage<IEnumerable<WeatherForecast>>;
