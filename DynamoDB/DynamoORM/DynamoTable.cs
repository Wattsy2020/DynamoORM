using System.Linq;
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
        var entityType = typeof(T);
        _fields = entityType.GetProperties()
            .Select(prop =>
                new KeyValuePair<string, DynamoTableField<T>>(prop.Name, new DynamoTableField<T>(prop.Name)))
            .ToDictionary();
    }

    // Function to initialise table in dynamo db, takes in limits and other appropriate parameters
    // This function needs to handle the indexes, might also need to store them in this class as well

    // Function to a record

    // Function to scan the table

    // Function to query the table, taking the indexes and querying keys
}