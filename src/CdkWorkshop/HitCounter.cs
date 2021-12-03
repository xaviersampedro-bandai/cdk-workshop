using System.Collections.Generic;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;
using Constructs;

namespace CdkWorkshop
{
    public class HitCounterProps
    {
        // The function for which we want to count url hits
        public IFunction Downstream { get; set; }
    }

    public class HitCounter : Construct
    {
        public Function Handler { get; }
        public Table Table { get; }

        public HitCounter(Construct scope, string id, HitCounterProps props) : base(scope, id)
        {
            Table = new Table(this, "Hits", new TableProps
            {
                PartitionKey = new Attribute
                {
                    Name = "path",
                    Type = AttributeType.STRING
                }
            });
            

            Handler = new Function(this, "HitCounterHandler", new FunctionProps
            {
                Runtime = Runtime.NODEJS_14_X,
                Handler = "hitcounter.handler",
                Code = Code.FromAsset("lambda"),
                Environment = new Dictionary<string, string>
                {
                    ["DOWNSTREAM_FUNCTION_NAME"] = props.Downstream.FunctionName,
                    ["HITS_TABLE_NAME"] = Table.TableName
                }
            });
            
            Table.GrantReadWriteData(Handler);
            props.Downstream.GrantInvoke(Handler);
        }
    }}