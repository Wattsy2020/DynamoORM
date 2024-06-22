using System.Reflection;
using Amazon.DynamoDBv2.Model;

namespace DynamoORM;

/// <summary>
/// Represents a field of an entity model
/// </summary>
/// <typeparam name="T">The entity model type</typeparam>
public class DynamoTableField<T>
{
    private readonly Type _entityType;
    private readonly MethodInfo _getMethod;
    private readonly MethodInfo _setMethod;
    private readonly Type _fieldType;
    private readonly string _propertyName;
    
    public readonly DynamoDataType DynamoType;

    public DynamoTableField(string propertyName)
    {
        _entityType = typeof(T);
        _propertyName = propertyName;
        
        var propertyInfo = _entityType.GetProperty(propertyName) 
                        ?? throw new EntityModelException($"Property: {propertyName} not found on entityType: {_entityType}");
        if (!propertyInfo.CanRead || propertyInfo.GetMethod is null)
        {
            throw new EntityModelException($"Property: {propertyName} is not readable");
        }

        if (!propertyInfo.CanWrite || propertyInfo.SetMethod is null)
        {
            throw new EntityModelException($"Property: {propertyName} is not writeable");
        }

        _getMethod = propertyInfo.GetMethod;
        _setMethod = propertyInfo.SetMethod;
        _fieldType = propertyInfo.PropertyType;
        DynamoType = _fieldType.GetDynamoType();
    }

    private AttributeValue ToAttributeValue(object value)
        => _fieldType.IsEnum
            ? new AttributeValue { S = Enum.GetName(_fieldType, value) }
            : _fieldType.ToAttributeValue(value);

    /// <summary>
    /// Extract the entityModel's field value and convert to an AttributeValue
    /// </summary>
    /// <param name="entityModel"></param>
    /// <returns></returns>
    public AttributeValue GetFieldValue(T entityModel)
    {
        var propertyValue = _getMethod.Invoke(entityModel, null)
            ?? throw new EntityModelException($"Failed to invoke the Getter for {this}");
        return ToAttributeValue(propertyValue);
    }

    private object ExtractAttributeValue(AttributeValue attributeValue)
        => _fieldType.IsEnum
            ? Enum.TryParse(_fieldType,
                attributeValue.S ?? throw DynamoSchemaException.ExpectedTypeException(DynamoDataType.String),
                out var enumValue)
                ? enumValue
                : throw new DynamoSchemaException($"Expected Attribute Value to be parsable as enum type: {_fieldType}")
            : _fieldType.ExtractAttributeValue(attributeValue);
    
    /// <summary>
    /// From a Dynamo DB response's attribute values, set the appropriate value on the entity model
    /// </summary>
    /// <param name="attributeValue"></param>
    /// <param name="entityModel"></param>
    public void SetFieldValue(AttributeValue attributeValue, T entityModel)
    {
        _setMethod.Invoke(entityModel, [ExtractAttributeValue(attributeValue)]);
    }

    public override string ToString() => $"{_entityType}.{_propertyName}";
}