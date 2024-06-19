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
    private readonly PropertyInfo _propertyInfo;
    public readonly DynamoDataType DynamoType;

    public DynamoTableField(string propertyName)
    {
        _entityType = typeof(T);
        
        _propertyInfo = _entityType.GetProperty(propertyName) 
                        ?? throw new EntityModelException($"Property: {propertyName} not found on entityType: {_entityType}");
        if (!_propertyInfo.CanRead || _propertyInfo.GetMethod is null)
        {
            throw new EntityModelException($"Property: {propertyName} is not readable");
        }

        if (!_propertyInfo.CanWrite || _propertyInfo.SetMethod is null)
        {
            throw new EntityModelException($"Property: {propertyName} is not writeable");
        }
        
        DynamoType = _propertyInfo.PropertyType.GetDynamoType();
    }

    /// <summary>
    /// Extract the entityModel's field value and convert to an AttributeValue
    /// </summary>
    /// <param name="entityModel"></param>
    /// <returns></returns>
    public AttributeValue GetFieldValue(T entityModel)
    {
        var propertyValue = _propertyInfo.GetMethod!.Invoke(entityModel, null)
            ?? throw new EntityModelException($"Failed to invoke the Getter for {this}");
        return _propertyInfo.PropertyType.ToAttributeValue(propertyValue);
    }
    
    /// <summary>
    /// From a Dynamo DB response's attribute values, set the appropriate value on the entity model
    /// </summary>
    /// <param name="attributeValue"></param>
    /// <param name="entityModel"></param>
    public void SetFieldValue(AttributeValue attributeValue, T entityModel)
    {
        var value = _propertyInfo.PropertyType.ExtractAttributeValue(attributeValue);
        _propertyInfo.SetMethod!.Invoke(entityModel, [value]);
    }

    public override string ToString() => $"{_entityType}.{_propertyInfo.Name}";
}