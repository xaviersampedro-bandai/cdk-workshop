using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Lambda;
using Cdklabs.DynamoTableViewer;
using Constructs;
using AssetOptions = Amazon.CDK.AWS.S3.Assets.AssetOptions;

namespace CdkWorkshop
{
    public class CdkWorkshopStack : Stack
    {
        internal CdkWorkshopStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // var hello = new Function(this, "HelloHandler", new FunctionProps
            // {
            //     Runtime = Runtime.NODEJS_14_X, // execution environment
            //     Code = Code.FromAsset("lambda"), // Code loaded from the "lambda" directory
            //     Handler = "hello.handler" // file is "hello", function is "handler"
            // });
            
            IEnumerable<string?> commands = new[]
            {
                "cd /asset-input",
                "export DOTNET_CLI_HOME=\"/tmp/DOTNET_CLI_HOME\"",
                "export PATH=\"$PATH:/tmp/DOTNET_CLI_HOME/.dotnet/tools\"",
                "dotnet tool install -g Amazon.Lambda.Tools",
                "dotnet lambda package -o output.zip",
                "unzip -o -d /asset-output output.zip"
            };


            var hello = new Function(this,
                "HelloHandler",
                new FunctionProps
                {
                    Runtime = Runtime.DOTNET_CORE_3_1,
                    Code = Code.FromAsset("src-lambda/Lambda", new AssetOptions
                    {
                        Bundling = new BundlingOptions
                        {
                            Image  = Runtime.DOTNET_CORE_3_1.BundlingImage,
                            Command = new []
                            {
                                "bash", "-c", string.Join(" && ", commands)
                            }
                        }
                    }),
                    Handler = "Lambda::Lambda.Hello::FunctionHandler"
                });
            
            var helloWithCounter = new HitCounter(this, "HelloHitCounter", new HitCounterProps
            {
                Downstream = hello
            });


            _ = new LambdaRestApi(this, "Endpoint", new LambdaRestApiProps
            {
                Handler = helloWithCounter.Handler
            });
            
            
            // Defines a new TableViewer resource
            _ = new TableViewer(this, "ViewerHitCount", new TableViewerProps
            {
                Title = "Hello Hits",
                Table = helloWithCounter.Table,
                SortBy = "-hits"
            });
        }
    }
}
