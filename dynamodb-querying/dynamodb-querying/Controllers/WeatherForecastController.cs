using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Util;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace dynamodb_querying.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(IDynamoDBContext dynamoDbContext, IAmazonDynamoDB dynamoDbClient,
            ILogger<WeatherForecastController> logger)
        {
            _dynamoDbContext = dynamoDbContext;
            _dynamoDbClient = dynamoDbClient;
            _logger = logger;
        }

        [HttpGet("specific-date")]
        public async Task<WeatherForecast> GetAsync(string cityName, DateTime date)
        {
            return await _dynamoDbContext.LoadAsync<WeatherForecast>(cityName, date);
        }

        [HttpGet("specific-date-projection")]
        public async Task<WeatherForecastProjectionItem> GetProjectionAsync(string cityName, DateTime date)
        {
            return await _dynamoDbContext.LoadAsync<WeatherForecastProjectionItem>(cityName, date,
                new DynamoDBOperationConfig()
                {
                    // OverrideTableName = "dev_WeatherForecast"
                });
        }

        [HttpGet("city-all-projection")]
        public async Task<IEnumerable<WeatherForecastProjectionItem>> GetAllForCityAsProjection(string cityName)
        {
            return await _dynamoDbContext.QueryAsync<WeatherForecastProjectionItem>(cityName)
                .GetRemainingAsync();
        }

        [HttpGet("city-all")]
        public async Task<IEnumerable<WeatherForecast>> GetAllForCity(string cityName)
        {
            return await _dynamoDbContext.QueryAsync<WeatherForecast>(cityName).GetRemainingAsync();
        }

        [HttpGet("city-date-filter")]
        public async Task<IEnumerable<WeatherForecast>> GetAllForCityAndDate(string cityName, DateTime startDate)
        {
            return await _dynamoDbContext
                .QueryAsync<WeatherForecast>(cityName, QueryOperator.GreaterThan, new object[] {startDate})
                .GetRemainingAsync();
        }

        [HttpGet("city-date-filter2")]
        public async Task<IEnumerable<WeatherForecast>> GetAllForCityAndDate(string cityName, DateTime startDate,
            DateTime endDate)
        {
            return await _dynamoDbContext
                .QueryAsync<WeatherForecast>(cityName, QueryOperator.Between, new object[] {startDate, endDate})
                .GetRemainingAsync();
        }

        [HttpGet("city-name-low-level")]
        public async Task<IEnumerable<WeatherForecast>> GetAllForCityLowLevel(string cityName)
        {
            var queryRequest = new QueryRequest()
            {
                TableName = nameof(WeatherForecast),
                KeyConditions = new Dictionary<string, Condition>()
                {
                    {
                        nameof(WeatherForecast.CityName), new Condition()
                        {
                            ComparisonOperator = ComparisonOperator.EQ,
                            AttributeValueList = new List<AttributeValue>() {new AttributeValue(cityName)}
                        }
                    }
                }
            };

            var response = await _dynamoDbClient.QueryAsync(queryRequest);

            return response.Items.Select(a =>
            {
                var doc = Document.FromAttributeMap(a);
                return _dynamoDbContext.FromDocument<WeatherForecast>(doc);
            });
        }

        [HttpGet("city-name-low-level-expressions")]
        public async Task<IEnumerable<WeatherForecast>> GetAllForCityLowLevelExpressions(string cityName,
            DateTime startDate, DateTime endDate, int minTemp)
        {
            var queryRequest = new QueryRequest()
            {
                TableName = nameof(WeatherForecast),
                KeyConditionExpression = "CityName = :cityName and #startDate between :startDate and :endDate",
                FilterExpression = "TemperatureC >= :minTemp",
                ExpressionAttributeNames = new Dictionary<string, string>()
                {
                    {"#startDate", "Date"}
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":cityName", new AttributeValue(cityName)},
                    {":startDate", new AttributeValue(startDate.ToString(AWSSDKUtils.ISO8601DateFormat))},
                    {":endDate", new AttributeValue(endDate.ToString(AWSSDKUtils.ISO8601DateFormat))},
                    {":minTemp", new AttributeValue() {N = minTemp.ToString()}}
                }
            };

            var response = await _dynamoDbClient.QueryAsync(queryRequest);

            return response.Items.Select(a =>
            {
                var doc = Document.FromAttributeMap(a);
                return _dynamoDbContext.FromDocument<WeatherForecast>(doc);
            });
        }

        [HttpGet("weather-scan")]
        public async Task<IEnumerable<WeatherForecast>> GetScan()
        {
            return await _dynamoDbContext.ScanAsync<WeatherForecast>(new ScanCondition[]
            {
                new ScanCondition(nameof(WeatherForecast.TemperatureC), ScanOperator.GreaterThan, new object[] {25})
            }).GetRemainingAsync();
        }

        [HttpGet("city-name-paged")]
        public async Task<PagedResult<WeatherForecast>> GetAllForCityPaged(string cityName, string? pageKey)
        {
            var exlusiveStartKey = string.IsNullOrEmpty(pageKey)
                ? null
                : JsonSerializer.Deserialize<Dictionary<string, AttributeValue>>(Convert.FromBase64String(pageKey));

            var queryRequest = new QueryRequest()
            {
                TableName = nameof(WeatherForecast),
                KeyConditionExpression = "CityName = :cityName",
                Limit = 5,
                ExclusiveStartKey = exlusiveStartKey,
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":cityName", new AttributeValue(cityName)},
                }
            };

            var response = await _dynamoDbClient.QueryAsync(queryRequest);

            var items = response.Items.Select(a =>
            {
                var doc = Document.FromAttributeMap(a);
                return _dynamoDbContext.FromDocument<WeatherForecast>(doc);
            }).ToList();

            var nextPageKey = response.LastEvaluatedKey.Count == 0
                ? null
                : Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(
                    response.LastEvaluatedKey,
                    new JsonSerializerOptions()
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
                    }));

            return new PagedResult<WeatherForecast>()
            {
                Items = items,
                NextPageKey = nextPageKey
            };
        }

        [HttpGet("city-name-paged-range-key")]
        public async Task<PagedResult<WeatherForecast>> GetAllForCityPagedWithRangeKey(string cityName,
            DateTime? fromDateTime, bool isForward = true)
        {
            Dictionary<string, AttributeValue> exlusiveStartKey = null;
            if (fromDateTime.HasValue)
            {
                var doc = new Document();
                doc[nameof(WeatherForecast.CityName)] = cityName;
                doc[nameof(WeatherForecast.Date)] = fromDateTime.Value.ToString(AWSSDKUtils.ISO8601DateFormat);
                exlusiveStartKey = doc.ToAttributeMap();
            }

            var queryRequest = new QueryRequest()
            {
                TableName = nameof(WeatherForecast),
                KeyConditionExpression = "CityName = :cityName",
                Limit = 5,
                ScanIndexForward = isForward,
                ExclusiveStartKey = exlusiveStartKey,
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":cityName", new AttributeValue(cityName)},
                }
            };

            var response = await _dynamoDbClient.QueryAsync(queryRequest);

            var items = response.Items.Select(a =>
            {
                var doc = Document.FromAttributeMap(a);
                return _dynamoDbContext.FromDocument<WeatherForecast>(doc);
            }).ToList();

            var nextPageKey = !response.LastEvaluatedKey.Any()
                ? null
                : isForward
                    ? items.MaxBy(a => a.Date)?.Date.ToString(AWSSDKUtils.ISO8601DateFormat)
                    : items.MinBy(a => a.Date)?.Date.ToString(AWSSDKUtils.ISO8601DateFormat);

            return new PagedResult<WeatherForecast>()
            {
                Items = items,
                NextPageKey = nextPageKey
            };
        }

        [HttpGet("city-name-paged-range-key-condition")]
        public async Task<PagedResult<WeatherForecast>> GetAllForCityPagedWithKeyCondition(string cityName,
            DateTime? fromDateTime)
        {
            var queryRequest = new QueryRequest()
            {
                TableName = nameof(WeatherForecast),
                KeyConditionExpression = "CityName = :cityName and #date > :fromDateTime",
                Limit = 5,
                ExpressionAttributeNames = new Dictionary<string, string>()
                {
                    {"#date", "Date"}
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":cityName", new AttributeValue(cityName)},
                    {":fromDateTime", new AttributeValue(fromDateTime.Value.ToString(AWSSDKUtils.ISO8601DateFormat))},
                }
            };

            var response = await _dynamoDbClient.QueryAsync(queryRequest);

            var items = response.Items.Select(a =>
            {
                var doc = Document.FromAttributeMap(a);
                return _dynamoDbContext.FromDocument<WeatherForecast>(doc);
            }).ToList();

            var nextPageKey = !response.LastEvaluatedKey.Any()
                ? null
                : items.MaxBy(a => a.Date)?.Date.ToString(AWSSDKUtils.ISO8601DateFormat);

            return new PagedResult<WeatherForecast>()
            {
                Items = items,
                NextPageKey = nextPageKey
            };
        }

        [HttpGet("specific-date-projection-expression")]
        public async Task<WeatherForecastListItem> GetWithProjectionAsync(
            string cityName, DateTime date)
        {
            return await _dynamoDbContext.LoadAsync<WeatherForecastListItem>(
                cityName,
                date, new DynamoDBOperationConfig()
                {
                    OverrideTableName = nameof(WeatherForecast)
                });
        }

        [HttpGet("city-name-paged-range-key-condition-projection-expression")]
        public async Task<PagedResult<WeatherForecastListItem>> GetAllForCityPagedWithKeyConditionProjectionExpression(
            string cityName, DateTime? fromDateTime)
        {
            var queryRequest = new QueryRequest()
            {
                TableName = nameof(WeatherForecast),
                KeyConditionExpression = "CityName = :cityName and #date > :fromDateTime",
                Limit = 5,
                ProjectionExpression = $"{nameof(WeatherForecast.CityName)},#date,TemperatureC,WeatherDetails[0],WeatherDetails[1]",
                ExpressionAttributeNames = new Dictionary<string, string>()
                {
                    {"#date", "Date"}
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":cityName", new AttributeValue(cityName)},
                    {":fromDateTime", new AttributeValue(fromDateTime.Value.ToString(AWSSDKUtils.ISO8601DateFormat))},
                }
            };

            var response = await _dynamoDbClient.QueryAsync(queryRequest);

            var items = response.Items.Select(a =>
            {
                var doc = Document.FromAttributeMap(a);
                return _dynamoDbContext.FromDocument<WeatherForecastListItem>(doc);
            }).ToList();

            var nextPageKey = !response.LastEvaluatedKey.Any()
                ? null
                : items.MaxBy(a => a.Date)?.Date.ToString(AWSSDKUtils.ISO8601DateFormat);

            return new PagedResult<WeatherForecastListItem>()
            {
                Items = items,
                NextPageKey = nextPageKey
            };
        }

        [HttpGet("gsi-query")]
        public async Task<List<WeatherForecastGSIItem>> GetUsingGSIQuery(DateTime dateTime)
        {
            var queryRequest = new QueryRequest()
            {
                TableName = nameof(WeatherForecast),
                IndexName = "Date-CityName-with-Temp-index",
                KeyConditionExpression = "#date = :dateTime",
                ExpressionAttributeNames = new Dictionary<string, string>()
                {
                    {"#date", "Date"}
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":dateTime", new AttributeValue(dateTime.ToString(AWSSDKUtils.ISO8601DateFormat))},
                }
            };

            var response = await _dynamoDbClient.QueryAsync(queryRequest);

            return response.Items.Select(a =>
            {
                var doc = Document.FromAttributeMap(a);
                return _dynamoDbContext.FromDocument<WeatherForecastGSIItem>(doc);
            }).ToList();
        }

        [HttpGet("lsi-query")]
        public async Task<List<WeatherForecastLSIItem>> GetUsingLSIQuery(string cityName)
        {
            var queryRequest = new QueryRequest()
            {
                TableName = nameof(WeatherForecastTable),
                IndexName = "TemperatureC-index-Include",
                KeyConditionExpression = "CityName = :cityName",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":cityName", new AttributeValue(cityName)},
                },

                // Use Projection Expression to fetch additional attributes that
                // are not specified on the Index

                // ProjectionExpression = "CityName,TemperatureC,#Date,Summary",
                // ExpressionAttributeNames = new Dictionary<string, string>()
                // {
                //     {"#Date", "Date"}
                // }
            };

            var response = await _dynamoDbClient.QueryAsync(queryRequest);

            return response.Items.Select(a =>
            {
                var doc = Document.FromAttributeMap(a);
                return _dynamoDbContext.FromDocument<WeatherForecastLSIItem>(doc);
            }).ToList();
        }

        [HttpGet("sparse-index-query")]
        public async Task<List<WeatherForecastSparseIndexItem>> GetUsingSparseIndexQuery(DateTime dateTime)
        {
            var queryRequest = new QueryRequest()
            {
                TableName = nameof(WeatherForecast),
                IndexName = "Date-IsExtremeWeatherConditions-index",
                KeyConditionExpression = "#date = :dateTime",
                ExpressionAttributeNames = new Dictionary<string, string>()
                {
                    {"#date", "Date"}
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":dateTime", new AttributeValue(dateTime.ToString(AWSSDKUtils.ISO8601DateFormat))},
                }
            };

            var response = await _dynamoDbClient.QueryAsync(queryRequest);

            return response.Items.Select(a =>
            {
                var doc = Document.FromAttributeMap(a);
                return _dynamoDbContext.FromDocument<WeatherForecastSparseIndexItem>(doc);
            }).ToList();
        }

        [HttpGet("dynamodb-context-gsi-query")]
        public async Task<List<WeatherForecastGSIItem>> GetUsingDynamoDBContextGSIQuery(DateTime dateTime)
        {
            var items = await _dynamoDbContext.QueryAsync<WeatherForecastGSIItem>(dateTime, new DynamoDBOperationConfig()
            {
                IndexName = "Date-CityName-with-Temp-index",
                OverrideTableName = nameof(WeatherForecast)
            }).GetRemainingAsync();

            return items.ToList();
        }

        [HttpPost("get-batch-dynamodbcontext-one-table")]
        public async Task<List<WeatherForecast>> GetBatch(Dictionary<string, DateTime> keys)
        {
            var batchGet = _dynamoDbContext.CreateBatchGet<WeatherForecast>();
            foreach (var key in keys)
                batchGet.AddKey(key.Key, key.Value);

            await batchGet.ExecuteAsync();

            return batchGet.Results;
        }

        [HttpPost("get-batch-dynamodbcontext-multiple-table")]
        public async Task<object> GetBatchMultipleTable(Dictionary<string, DateTime> keys)
        {
            var batchGet1 = _dynamoDbContext.CreateBatchGet<WeatherForecast>();
            var batchGet2 = _dynamoDbContext.CreateBatchGet<WeatherForecastTable>();
            foreach (var key in keys)
            {
                batchGet1.AddKey(key.Key, key.Value);
                batchGet2.AddKey(key.Key, key.Value);
            }

           // var batches = batchGet1.Combine(batchGet2);

            var batches = _dynamoDbContext.CreateMultiTableBatchGet(batchGet1, batchGet2);

            await batches.ExecuteAsync();

            return new {WeatherForecast = batchGet1.Results, WeatherForecastTable = batchGet2.Results};
        }
        
        [HttpPost("get-batch-dynamodbclient-one-table")]
        public async Task<List<WeatherForecast>> GetBatchDynamoDbClient(Dictionary<string, DateTime> keys)
        {
            var batchGetItemRequest = new BatchGetItemRequest();
            batchGetItemRequest.RequestItems = new Dictionary<string, KeysAndAttributes>()
            {
                {
                    nameof(WeatherForecast), new KeysAndAttributes()
                    {
                        Keys = keys.Select(key => new Dictionary<string, AttributeValue>()
                        {
                            {nameof(WeatherForecast.CityName), new AttributeValue(key.Key)},
                            {nameof(WeatherForecast.Date), new AttributeValue(key.Value.ToString(AWSSDKUtils.ISO8601DateFormat))}
                        }).ToList()
                    }
                }
            };

            var response = await _dynamoDbClient.BatchGetItemAsync(batchGetItemRequest);

            return response.Responses
                .SelectMany(t => t.Value.Select(Document.FromAttributeMap)
                .Select(_dynamoDbContext.FromDocument<WeatherForecast>))
                .ToList();
        }
        
        [HttpPost("get-batch-dynamodbclient-multi-table")]
        public async Task<List<WeatherForecast>> GetBatchDynamoDbClientMulti(Dictionary<string, DateTime> keys)
        {
            var batchGetItemRequest = new BatchGetItemRequest();
            batchGetItemRequest.RequestItems = new Dictionary<string, KeysAndAttributes>()
            {
                {
                    nameof(WeatherForecast), new KeysAndAttributes()
                    {
                        Keys = keys.Select(key => new Dictionary<string, AttributeValue>()
                        {
                            {nameof(WeatherForecast.CityName), new AttributeValue(key.Key)},
                            {nameof(WeatherForecast.Date), new AttributeValue(key.Value.ToString(AWSSDKUtils.ISO8601DateFormat))}
                        }).ToList()
                    }
                },
                {
                    nameof(WeatherForecastTable), new KeysAndAttributes()
                    {
                        Keys = keys.Select(key => new Dictionary<string, AttributeValue>()
                        {
                            {nameof(WeatherForecast.CityName), new AttributeValue(key.Key)},
                            {nameof(WeatherForecast.Date), new AttributeValue(key.Value.ToString(AWSSDKUtils.ISO8601DateFormat))}
                        }).ToList()
                    }
                }
            };

            var response = await _dynamoDbClient.BatchGetItemAsync(batchGetItemRequest);

            return response.Responses
                .SelectMany(t => t.Value.Select(Document.FromAttributeMap)
                    .Select(_dynamoDbContext.FromDocument<WeatherForecast>))
                .ToList();
        }

        [HttpPost]
        public async Task Post(WeatherForecast data)
        {
            data.Date = data.Date.Date;
            await _dynamoDbContext.SaveAsync(data);
        }

        [HttpPost("post-weather-forecast-table-lsi")]
        public async Task PostWeatherForecastLisi(WeatherForecastTable data)
        {
            data.Date = data.Date.Date;
            await _dynamoDbContext.SaveAsync(data);
        }

        [HttpPost("post-skip-versioning")]
        public async Task PostSkipVersioning(WeatherForecast data)
        {
            data.Date = data.Date.Date;
            await _dynamoDbContext.SaveAsync(data, new DynamoDBOperationConfig() {SkipVersionCheck = true});
        }

        [HttpDelete("delete-with-version")]
        public async Task DeleteWithVersion(WeatherForecast data)
        {
            data.Date = data.Date.Date;
            await _dynamoDbContext.DeleteAsync(data);
        }

        [HttpPost("post-if-latest")]
        public async Task PostIfLatest(WeatherForecast data)
        {
            data.Date = data.Date.Date;
            var item = Document.FromJson(JsonSerializer.Serialize(data));
            await _dynamoDbClient.PutItemAsync(new PutItemRequest()
            {
                TableName = nameof(WeatherForecast),
                Item = item.ToAttributeMap(),
                ConditionExpression = "attribute_not_exists(LastUpdated) OR (LastUpdated < :LastUpdated)",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {
                        ":LastUpdated", new AttributeValue(data.LastUpdated.ToString(AWSSDKUtils.ISO8601DateFormat))
                    }
                }
            });
        }

        [HttpPost("post-if-not-exists")]
        public async Task PostIfNotExists(WeatherForecast data)
        {
            data.Date = data.Date.Date;
            var item = Document.FromJson(JsonSerializer.Serialize(data));
            await _dynamoDbClient.PutItemAsync(new PutItemRequest()
            {
                TableName = nameof(WeatherForecast),
                Item = item.ToAttributeMap(),
                ConditionExpression = "attribute_not_exists(CityName) AND attribute_not_exists(#Date)",
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    {"#Date", "Date"}
                }
            });
        }

        [HttpPost("put-item-request")]
        public async Task PutItemRequest(WeatherForecast data)
        {
            data.Date = data.Date.Date;
            var item = Document.FromJson(JsonSerializer.Serialize(data));
            await _dynamoDbClient.PutItemAsync(new PutItemRequest()
            {
                TableName = nameof(WeatherForecast),
                Item = item.ToAttributeMap(),
            });
        }

        [HttpPost("update-item-request")]
        public async Task UpdateItemRequest(string cityName, string date, string summary)
        {
            var request = new UpdateItemRequest()
            {
                TableName = nameof(WeatherForecast),
                Key = new Dictionary<string, AttributeValue>
                {
                    {nameof(WeatherForecast.CityName), new AttributeValue(cityName)},
                    {nameof(WeatherForecast.Date), new AttributeValue(date)}
                },
                UpdateExpression = "SET Summary = :summary",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":summary", new AttributeValue(summary)}
                },
            };

            await _dynamoDbClient.UpdateItemAsync(request);
        }

        [HttpDelete]
        public async Task DeleteIfGt20(string cityName, string date)
        {
            await _dynamoDbClient.DeleteItemAsync(new DeleteItemRequest()
            {
                TableName = nameof(WeatherForecast),
                Key = new Dictionary<string, AttributeValue>
                {
                    {"CityName", new AttributeValue(cityName)},
                    {"Date", new AttributeValue(date)}
                },
                ConditionExpression = "TemperatureC > :limit",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":limit", new AttributeValue() {N = "20"}}
                }
            });
        }
    }
}