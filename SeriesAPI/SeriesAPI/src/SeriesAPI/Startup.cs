using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace SeriesAPI
{
    [LambdaStartup]
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ICalculatorService>(new CalculatorService());

            services.AddAWSService<IAmazonDynamoDB>();
            services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
            services.AddScoped<IDynamoDBContext, DynamoDBContext>();
        }
    }
}