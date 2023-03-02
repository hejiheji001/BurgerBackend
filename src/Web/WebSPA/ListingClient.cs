using System.Text.Json;

namespace WebSPA;

public class ListingClient
{
    private readonly JsonSerializerOptions? options = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly HttpClient client;
    private readonly ILogger<ListingClient> _logger;

    public ListingClient(HttpClient client, ILogger<ListingClient> logger)
    {
        this.client = client;
        this._logger = logger;
    }

    public async Task<WeatherForecast[]?> GetWeatherForecastAsync()
    {
        try {
            var responseMessage = await this.client.GetAsync("/backendUrl");
            Console.WriteLine(responseMessage);
            if(responseMessage!=null)
            {
                var stream = await responseMessage.Content.ReadAsStreamAsync();
                return await JsonSerializer.DeserializeAsync<WeatherForecast[]>(stream, options);
            }
        }
        catch(HttpRequestException ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }
        return new WeatherForecast[] {};
    }
}