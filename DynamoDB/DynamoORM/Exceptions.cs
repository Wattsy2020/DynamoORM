namespace DynamoORM;

public class DynamoSchemaException(string message) : Exception(message)
{
    public static DynamoSchemaException ExpectedTypeException(DynamoDataType type)
        => new ($"Expected Attribute Value to have type {type}");
}

public class EntityModelException(string message) : Exception(message);
