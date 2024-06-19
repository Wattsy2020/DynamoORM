using DynamoDB;

const string tableName = "JobStatus";
var client = SetupDatabase.InitializeClient();
await client.DeleteTableAndWaitAsync(tableName);
await client.CreateJobTableAsync(tableName);

Console.WriteLine("Populating Table Data...");
await TableData.PopulateTable(client, tableName);

Console.WriteLine("Scanning Table...");
foreach (var item in await client.ScanAsync(tableName))
{
    Console.WriteLine(item);
}

Console.WriteLine("Querying Table for FirstUser's jobs after 2024/06/04");
foreach (var item in await client.QueryJobsAsync(tableName, "FirstUser"))
{
    Console.WriteLine(item);
}

Console.WriteLine("Querying Table for unprocessed jobs");
await Task.Delay(TimeSpan.FromSeconds(10)); // wait for written records to sync with the GSI
foreach (var item in await client.QueryJobsAsync(tableName, JobStatus.Unprocessed))
{
    Console.WriteLine(item);
}
