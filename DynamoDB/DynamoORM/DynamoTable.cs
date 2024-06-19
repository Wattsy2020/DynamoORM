namespace DynamoORM;

/// <summary>
/// Represents a Dynamo DB Table for an entity model
/// </summary>
/// <typeparam name="T">The entity model type</typeparam>
public class DynamoTable<T>
{
    private readonly Dictionary<string, DynamoTableField<T>> _fields;

    public DynamoTable()
    {
        // initialise fields using reflection on the type
        _fields = null;
    }

    // Function to initialise table in dynamo db, takes in limits and other appropriate parameters

    // Function to a record

    // Function to scan the table

    // Function to query the table, taking the indexes and querying keys
}