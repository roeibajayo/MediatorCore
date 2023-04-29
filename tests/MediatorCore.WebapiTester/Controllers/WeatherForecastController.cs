using MediatorCore.WebapiTester.GetWeatherForcast;
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