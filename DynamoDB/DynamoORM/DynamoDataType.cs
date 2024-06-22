using Amazon.DynamoDBv2.Model;

namespace DynamoORM;

public enum DynamoDataType
{
    String,
    Number
    // not supporting other types for now
}

public static class DynamoDataTypeExtensions
{
    private static readonly Dictionary<Type, DynamoDataType> TypeMap = new()
    {
        { typeof(string), DynamoDataType.String },
        { typeof(char), DynamoDataType.String },
        { typeof(Enum), DynamoDataType.String },
        { typeof(int), DynamoDataType.Number },
        { typeof(short), DynamoDataType.Number },
        { typeof(long), DynamoDataType.Number },
        { typeof(DateTime), DynamoDataType.Number },
    };

    public static DynamoDataType GetDynamoType(this Type type) => TypeMap[type];

    private static readonly Dictionary<Type, Func<object, AttributeValue>> SerialisationFunctions = new()
    {
        { typeof(string), obj => new AttributeValue { S = (string)obj } },
        { typeof(char), obj => new AttributeValue { S = ((char)obj).ToString() } },
        { typeof(int), obj => new AttributeValue { N = ((int)obj).ToString() } },
        { typeof(short), obj => new AttributeValue { N = ((short)obj).ToString() } },
        { typeof(long), obj => new AttributeValue { N = ((long)obj).ToString() } },
        { typeof(DateTime), obj => new AttributeValue { N = ((DateTime)obj).Ticks.ToString() } },
        // todo: use typeof(Enum) to handle enums (convert them to a string)
    };

    public static AttributeValue ToAttributeValue(this Type type, object value)
        => SerialisationFunctions[type](value);

    private static string ParseString(AttributeValue value)
        => value.S ?? throw DynamoSchemaException.ExpectedTypeException(DynamoDataType.String);

    private static char ParseChar(AttributeValue value)
        => char.TryParse(ParseString(value), out var charResult)
            ? charResult
            : throw new DynamoSchemaException("Expected String Attribute to be convertable to char");
    
    private static int ParseInt(AttributeValue value)
        => int.TryParse(value.N ?? throw DynamoSchemaException.ExpectedTypeException(DynamoDataType.Number), out var parsedInt)
            ? parsedInt
            : throw new DynamoSchemaException("Expected Numeric Attribute to be convertable to int");
    
    private static short ParseShort(AttributeValue value)
        => short.TryParse(value.N ?? throw DynamoSchemaException.ExpectedTypeException(DynamoDataType.Number), out var parsedShort)
            ? parsedShort
            : throw new DynamoSchemaException("Expected Numeric Attribute to be convertable to short");
    
    private static long ParseLong(AttributeValue value)
        => long.TryParse(value.N ?? throw DynamoSchemaException.ExpectedTypeException(DynamoDataType.Number), out var parsedLong)
            ? parsedLong
            : throw new DynamoSchemaException("Expected Numeric Attribute to be convertable to long");

    private static DateTime ParseDateTime(AttributeValue value)
        => new (ticks: ParseLong(value));

    private static readonly Dictionary<Type, Func<AttributeValue, object>> DeserializationFunctions = new()
    {
        { typeof(string), value => ParseString(value) },
        { typeof(char), value => ParseChar(value) },
        { typeof(int), value => ParseInt(value) },
        { typeof(short), value => ParseShort(value) },
        { typeof(long), value => ParseLong(value) },
        { typeof(DateTime), value => ParseDateTime(value) },
        // todo: use typeof(Enum) to handle enums (read from a string)
    };

    public static object ExtractAttributeValue(this Type type, AttributeValue value)
        => DeserializationFunctions[type](value);
}