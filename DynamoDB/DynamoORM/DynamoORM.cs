using Amazon.DynamoDBv2;

namespace DynamoORM;

public class DynamoORM
{
    private readonly AmazonDynamoDBClient _dynamoDbClient;

    public DynamoORM(AmazonDynamoDBClient client)
    {
        _dynamoDbClient = client;
    }

    /// <summary>
    /// Create a table for the given entity type
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <returns>A table for the entity</returns>
    public DynamoTable<T> Table<T>()
    {
        return null;
    }
}