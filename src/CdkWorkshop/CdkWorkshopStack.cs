using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Lambda;
using Cdklabs.DynamoTableViewer;
using Constructs;

namespace CdkWorkshop
{
    public class CdkWorkshopStack : Stack
    {
        internal CdkWorkshopStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var hello = new Function(this, "HelloHandler", new FunctionProps
            {
                Runtime = Runtime.NODEJS_14_X, // execution environment
                Code = Code.FromAsset("lambda"), // Code loaded from the "lambda" directory
                Handler = "hello.handler" // file is "hello", function is "handler"
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
                Table = helloWithCounter.Table
            });
        }
    }
}
