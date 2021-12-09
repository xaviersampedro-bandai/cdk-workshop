using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Model;
using Amazon.Lambda.Serialization.SystemTextJson;
using Environment = System.Environment;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace Lambda
{
    public class HitCounter
    {
       public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var dynamoDbClient = new AmazonDynamoDBClient(); 
            var lambdaClient = new AmazonLambdaClient(); 
            
            await dynamoDbClient.UpdateItemAsync(new UpdateItemRequest
            {
                TableName = Environment.GetEnvironmentVariable("HITS_TABLE_NAME"),
                Key = new Dictionary<string, AttributeValue>
                {
                    ["path"] = new AttributeValue
                    {
                        S = request.Path
                    }
                },
                UpdateExpression = "ADD hits :incr",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":incr"] = new AttributeValue
                    {
                        N = "1"
                    }
                }
            });

            var response = await lambdaClient.InvokeAsync(new InvokeRequest
            {
                FunctionName = Environment.GetEnvironmentVariable("DOWNSTREAM_FUNCTION_NAME"),
                Payload = JsonStringify(request)
            });

            return new DefaultLambdaJsonSerializer().Deserialize<APIGatewayProxyResponse>(response.Payload);
        }

        private static string JsonStringify(object obj)
        {
            var stream = new MemoryStream();
            new DefaultLambdaJsonSerializer().Serialize(obj, stream);
            var json = new StreamReader(stream).ReadToEnd();
            stream.Dispose();
            return json;
        }
    }
}