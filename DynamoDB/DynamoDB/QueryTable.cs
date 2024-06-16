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
}