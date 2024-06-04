using DynamoDB;

var client = SetupDatabase.InitializeClient();
await client.CreateJobTableAsync();
