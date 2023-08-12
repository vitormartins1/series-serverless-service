using Amazon.DynamoDBv2.DataModel;
using System.Text.Json.Serialization;

namespace dynamodb_querying
{
    public class WeatherForecast
    {
        public string CityName { get; set; }
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        public string? Summary { get; set; }
        public WeatherType WeatherType { get; set; }
        public Wind Wind { get; set; }
        public List<WeatherDetails>? WeatherDetails { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool? IsExtremeWeatherConditions { get; set; }

        // [DynamoDBVersion]
        public int? VersionNumber { get; set; }
    }
    public class WeatherDetails
    {
        public string TimeOfDay { get; set; }
        public int Temperature { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WeatherType
    {
        None,
        Sunny,
        Cloudy,
        Windy,
        Rainy,
        Stormy
    }

    public class Wind
    {
        public decimal Speed { get; set; }
        public string Direction { get; set; }
        public WeatherType WeatherType { get; set; }
    }
}