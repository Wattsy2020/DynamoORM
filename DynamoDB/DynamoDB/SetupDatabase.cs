using System.Security.Authentication;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

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
    /// <returns>A Boolean value indicating the success of the operation.</returns>
    public static async Task CreateJobTableAsync(this AmazonDynamoDBClient client)
    {
        if (await client.DoesTableExistAsync("JobStatus"))
        {
            Console.WriteLine("Not creating table as it already exists");
            return;
        }

        var response = await client.CreateTableAsync(new CreateTableRequest
        {
            TableName = "JobStatus",
            AttributeDefinitions =
            [
                new AttributeDefinition
                {
                    AttributeName = "UserID",
                    AttributeType = ScalarAttributeType.S,
                },
            ],
            KeySchema =
            [
                new KeySchemaElement
                {
                    AttributeName = "UserID",
                    KeyType = KeyType.HASH,
                },
            ],
            ProvisionedThroughput = new ProvisionedThroughput
            {
                ReadCapacityUnits = 5,
                WriteCapacityUnits = 5,
            },
        });

        // Wait until the table is ACTIVE and then report success.
        Console.WriteLine("Waiting for table to become active...");

        var request = new DescribeTableRequest
        {
            TableName = response.TableDescription.TableName,
        };

        TableStatus status;

        int sleepDuration = 2000;

        do
        {
            Thread.Sleep(sleepDuration);

            var describeTableResponse = await client.DescribeTableAsync(request);
            status = describeTableResponse.Table.TableStatus;
            var attrSchema = describeTableResponse.Table.AttributeDefinitions
                .Select(attr => $"{attr.AttributeName}: {attr.AttributeType}");
            Console.WriteLine($"Attribute: {string.Join('\n', attrSchema)}");
        }
        while (status != "ACTIVE");

        Console.WriteLine($"Finished creating table, status: {status}");
    }
}
