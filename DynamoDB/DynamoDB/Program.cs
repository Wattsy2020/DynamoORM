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
