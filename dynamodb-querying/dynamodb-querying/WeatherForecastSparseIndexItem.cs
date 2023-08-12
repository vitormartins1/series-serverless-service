namespace dynamodb_querying;

public class WeatherForecastSparseIndexItem
{
    public string CityName { get; set; }
    public DateTime Date { get; set; }
    public bool? IsExtremeWeatherConditions { get; set; }
}