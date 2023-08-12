using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var env = "test_";
var dynamoDbClient = new AmazonDynamoDBClient(FallbackCredentialsFactory.GetCredentials(), RegionEndpoint.APSoutheast2);
var context = new DynamoDBContext(dynamoDbClient, new DynamoDBContextConfig()
{
      // TableNamePrefix = env
});
builder.Services.AddSingleton<IAmazonDynamoDB>(dynamoDbClient);
builder.Services.AddSingleton<IDynamoDBContext>(context);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();