using System.Net;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace serverless_app_with_dynamo_example;

public class Functions
{
    public Functions()
    {
    }


    /// <summary>
    /// A Lambda function to respond to HTTP Get methods from API Gateway
    /// </summary>
    /// <remarks>
    /// This uses the <see href="https://github.com/aws/aws-lambda-dotnet/blob/master/Libraries/src/Amazon.Lambda.Annotations/README.md">Lambda Annotations</see> 
    /// programming model to bridge the gap between the Lambda programming model and a more idiomatic .NET model.
    /// 
    /// This automatically handles reading parameters from an <see cref="APIGatewayProxyRequest"/>
    /// as well as syncing the function definitions to serverless.template each time you build.
    /// 
    /// If you do not wish to use this model and need to manipulate the API Gateway 
    /// objects directly, see the accompanying Readme.md for instructions.
    /// </remarks>
    /// <param name="context">Information about the invocation, function, and execution environment</param>
    /// <returns>The response as an implicit <see cref="APIGatewayProxyResponse"/></returns>
    [LambdaFunction(
        PackageType = LambdaPackageType.Image,
        MemorySize = 256, Timeout = 30)]
    [RestApi(LambdaHttpMethod.Get, "/")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> GetAsync(
        APIGatewayProxyRequest request, 
        ILambdaContext context)
    {
        context.Logger.LogInformation("Get Request\n");

        var response = new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = await ScanReadingListAsync(),
            Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
        };

        return response;
    }

    private async Task<string> ScanReadingListAsync()
    {
        using var client = new AmazonDynamoDBClient(Amazon.RegionEndpoint.USEast1);

        var response = await client.ScanAsync(new ScanRequest("readingList"));

        return JsonConvert.SerializeObject(response.Items);
    }
}
