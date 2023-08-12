using Amazon.DynamoDBv2.DataModel;

namespace dynamodb_querying;

[DynamoDBTable("WeatherForecast")]
public class WeatherForecastProjectionItem
{
    public string CityName { get; set; }
    public DateTime Date { get; set; }
    public int TemperatureC { get; set; }
}