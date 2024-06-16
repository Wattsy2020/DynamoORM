using System.Security.Authentication;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Common;

namespace DynamoDB;

public static class SetupDatabase
{
    public static AmazonDynamoDBClient InitializeClient()
    {
        var accessKey = Environment.GetEnvironmentVariable("DynamoAccessKey") 
                        ?? throw new InvalidCredentialException("Could not get the Access Key");
        var secretKey = Environment.GetEnvironmentVariable("DynamoSecretKey") 
                        ?? throw new InvalidCredentialException("Could not get the Secret Key");
        var config = new AmazonDynamoDBConfig
        {
            RegionEndpoint = RegionEndpoint.APSoutheast2
        };
        return new AmazonDynamoDBClient(accessKey, secretKey, config);
    }

    public static async Task<bool> DoesTableExistAsync(this AmazonDynamoDBClient client, string tableName)
    {
        try
        {
            _ = await client.DescribeTableAsync(tableName);
        }
        catch (ResourceNotFoundException)
        {
            return false;
        }

        return true;
    }

    public static async Task<bool> IsTableEmpty(this AmazonDynamoDBClient client, string tableName)
    {
        var response = await client.ScanAsync(new ScanRequest
        {
            TableName = tableName, 
            Limit = 1
        });
        return response.Items.Count == 0;
    }

    /// <summary>
    /// Wait until the table is in the desired status
    /// </summary>
    public static async Task WaitForStatusAsync(this AmazonDynamoDBClient client, string tableName, TableStatus status )
    {
        var request = new DescribeTableRequest { TableName = tableName };
        await Functools.WaitForPredicateAsync(async () => 
            (await client.DescribeTableAsync(request)).Table.TableStatus == status);
    }
    
    private static readonly CreateTableRequest TableSchema = new()
    {
        TableName = "JobStatus",
        AttributeDefinitions =
        [
            new AttributeDefinition
            {
                AttributeName = "UserId",
                AttributeType = ScalarAttributeType.S,
            },
            new AttributeDefinition
            {
                AttributeName = "JobCreationTimestamp",
                AttributeType = ScalarAttributeType.N,
            },
            new AttributeDefinition
            {
                AttributeName = "JobStatus",
                AttributeType = ScalarAttributeType.S,
            }
        ],
        KeySchema =
        [
            new KeySchemaElement
            {
                AttributeName = "UserId",
                KeyType = KeyType.HASH,
            },
            new KeySchemaElement
            {
                AttributeName = "JobCreationTimestamp",
                KeyType = KeyType.RANGE
            }
        ],
        GlobalSecondaryIndexes =
        [
            // this is for processing jobs, we need the filtering info
            // we can also query for expired jobs by querying for JobStatus == "Processed" and "JobCreationTimestamp" > 8 days ago
            // in which case we need the S3 Info
            new GlobalSecondaryIndex
            {
                IndexName = "JobStatusIndex",
                KeySchema =
                [
                    new KeySchemaElement
                    {
                        AttributeName = "JobStatus",
                        KeyType = KeyType.HASH
                    },
                    new KeySchemaElement
                    {
                        AttributeName = "JobCreationTimestamp",
                        KeyType = KeyType.RANGE,
                    }
                ],
                Projection = new Projection
                {
                    ProjectionType = ProjectionType.ALL
                },
                // Each GSI has its own provisioned throughput that it consumes
                // Whenever a write occurs in the base table, it is replicated to the GSI, costing 1/2 a write unit
                // Reads are similar
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 5,
                    WriteCapacityUnits = 5,
                }
            }
        ],
        ProvisionedThroughput = new ProvisionedThroughput
        {
            ReadCapacityUnits = 5,
            WriteCapacityUnits = 5,
        },
    };

    /// <summary>
    /// Creates a new Amazon DynamoDB table and then waits for the new
    /// table to become active.
    /// 
    /// Schema: User Id primary key, maps to list of Jobs:
    ///   - Job Creation Date Ticks: number
    ///   - Job Status: integer enum
    ///   - Report Creation Date Ticks (optional): number
    ///   - Report S3 Link (optional): string
    /// GSI on Job Status for querying unused data
    /// </summary>
    /// <param name="client">An initialized Amazon DynamoDB client object.</param>
    /// <param name="tableName"></param>
    /// <returns>A Boolean value indicating the success of the operation.</returns>
    public static async Task CreateJobTableAsync(this AmazonDynamoDBClient client, string tableName)
    {
        if (await client.DoesTableExistAsync(tableName))
        {
            if (!await client.IsTableEmpty(tableName))
            {
                Console.WriteLine("Not creating table as it already exists and has data");
                return;
            }
            
            Console.WriteLine("Deleting Empty Table");
            await client.DeleteTableAndWaitAsync(tableName);
        }

        await client.CreateTableAsync(TableSchema);
        Console.WriteLine("Waiting for table to become active...");
        await client.WaitForStatusAsync(tableName, TableStatus.ACTIVE);
        Console.WriteLine("Table Created!");
    }

    public static async Task DeleteTableAndWaitAsync(this AmazonDynamoDBClient client, string tableName)
    {
        try
        {
            await client.DeleteTableAsync(tableName);
        }
        catch (ResourceNotFoundException)
        {
            return;
        }
        await Functools.WaitForPredicateAsync(async () => !await client.DoesTableExistAsync(tableName));
    }
}
