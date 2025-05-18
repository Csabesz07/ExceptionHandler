using Microsoft.AspNetCore.Mvc;

namespace Transactions.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];        

        [HttpGet("GetWeatherForecast/{count:int}")]
        [ProducesResponseType(typeof(WeatherForecast[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ArgumentException), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BarrierPostPhaseException), StatusCodes.Status403Forbidden)]
        public ActionResult GetWeatherForecast([FromRoute] int count) =>
            new ExceptionHandler<IEnumerable<WeatherForecast>>(() =>
            {
                if (count == 10)
                    throw new BarrierPostPhaseException();

                if(count == 20)
                    throw new ArgumentException();

                return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[count]
                })
                .ToArray();
            },
            (typeof(ArgumentException), StatusCodes.Status400BadRequest),
            (typeof(BarrierPostPhaseException), StatusCodes.Status403Forbidden));

        [HttpGet("GetRandomNumbers/{count:int}")]
        [ProducesResponseType(typeof(int[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ArgumentException), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BarrierPostPhaseException), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> GetRandomNumbersAsync([FromRoute] int count) =>
            await new ExceptionHandler<IEnumerable<int>>()
            .ExecuteAsync(async () =>
            {
                if (count < 1 || count > 100)
                    throw new ArgumentException();

                if (count == 42)
                    throw new BarrierPostPhaseException();

                await Task.Delay(100);

                var randomNumbers = Enumerable.Range(0, count)
                    .Select(_ => Random.Shared.Next(1, 1000))
                    .ToArray();

                var randomWeather = Summaries[count];

                return randomNumbers.AsEnumerable();
            },
            (typeof(ArgumentException), StatusCodes.Status400BadRequest),
            (typeof(BarrierPostPhaseException), StatusCodes.Status403Forbidden));
    }
}
