using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace DynamoDB;

public static class TableData
{
    private static readonly JobReport[] ExampleReports =
    [
        new JobReport
        {
            UserId = "FirstUser",
            JobCreationTimestamp = new DateTime(2024, 06, 01),
            JobStatus = JobStatus.Processed,
            ReportS3Bucket = "bucket",
            ReportS3Key = "key1",
            ReportCreationTimestamp = new DateTime(2024, 06, 02),
            FilterParameters = "random data"
        },
        new JobReport
        {
            UserId = "FirstUser",
            JobCreationTimestamp = new DateTime(2024, 06, 05),
            JobStatus = JobStatus.Unprocessed,
            FilterParameters = "random data2"
        },
        new JobReport
        {
            UserId = "SecondUser",
            JobCreationTimestamp = new DateTime(2024, 06, 07),
            JobStatus = JobStatus.Unprocessed,
            FilterParameters = "random data3"
        },
        new JobReport
        {
            UserId = "ThirdUser",
            JobCreationTimestamp = new DateTime(2024, 06, 06),
            JobStatus = JobStatus.Unprocessed,
            FilterParameters = "random data4"
        },
        new JobReport
        {
            UserId = "FirstUser",
            JobCreationTimestamp = new DateTime(2024, 06, 04),
            JobStatus = JobStatus.Processing,
            FilterParameters = "random data3"
        }
    ];
    
    public static async Task PopulateTable(AmazonDynamoDBClient client, string tableName)
    {
        await Task.WhenAll(ExampleReports.Select(report =>
            client.PutItemAsync(new PutItemRequest(tableName, report.ToAttributes()))));
    }
}