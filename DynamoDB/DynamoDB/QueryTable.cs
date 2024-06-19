using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace DynamoDB;

public static class QueryTable
{
    public static async Task<IEnumerable<JobReport>> ScanAsync(this AmazonDynamoDBClient client, string tableName)
    {
        var result = await client.ScanAsync(new ScanRequest(tableName));
        return result.Items.Select(JobReport.FromAttributes);
    }

    public static async Task<IEnumerable<JobReport>> QueryJobsAsync(this AmazonDynamoDBClient client, string tableName, string userId)
    {
        var response = await client.QueryAsync(new QueryRequest
        {
            TableName = tableName,
            KeyConditionExpression = "UserId = :UserIdVal AND JobCreationTimestamp >= :JobCreationTimestampVal",
            ExpressionAttributeValues =
            {
                { ":UserIdVal", new AttributeValue { S = userId } },
                { ":JobCreationTimestampVal", new AttributeValue { N = new DateTime(2024, 06, 04).Ticks.ToString() }}
            }
        });
        return response.Items.Select(JobReport.FromAttributes);
    }

    public static async Task<IEnumerable<JobReport>> QueryJobsAsync(this AmazonDynamoDBClient client, string tableName, JobStatus status)
    {
        var response = await client.QueryAsync(new QueryRequest
        {
            TableName = tableName,
            IndexName = "JobStatusIndex",
            KeyConditionExpression = "JobStatus = :JobStatusVal AND JobCreationTimestamp >= :JobCreationTimestampVal",
            ExpressionAttributeValues =
            {
                { ":JobStatusVal", new AttributeValue { S = status.ToString() } },
                { ":JobCreationTimestampVal", new AttributeValue { N = new DateTime(2024, 06, 04).Ticks.ToString() }}
            }
        });
        return response.Items.Select(JobReport.FromAttributes);
    }
}