namespace dynamodb_querying;

public class WeatherForecastTable
{
    public string CityName { get; set; }
    public DateTime Date { get; set; }
    public string TemperatureC { get; set; }
    public string? Summary { get; set; }
}