using System.Reflection;
using Amazon.DynamoDBv2.Model;

namespace DynamoORM;

/// <summary>
/// Represents a field of an entity model
/// </summary>
/// <typeparam name="T">The entity model type</typeparam>
public class DynamoTableField<T>
{
    private readonly string _propertyName;
    private readonly Type _entityType;
    private readonly PropertyInfo _propertyInfo;
    public readonly DynamoDataType DynamoType;

    public DynamoTableField(string propertyName)
    {
        _propertyName = propertyName;
        _entityType = typeof(T);
        
        _propertyInfo = _entityType.GetProperty(propertyName) 
                        ?? throw new EntityModelException($"Property: {propertyName} not found on entityType: {_entityType}");
        if (!_propertyInfo.CanRead)
        {
            throw new EntityModelException($"Property: {propertyName} is not readable");
        }

        if (!_propertyInfo.CanWrite)
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
        return null;
    }
    
    /// <summary>
    /// From a Dynamo DB response's attribute values, set the appropriate value on the entity model
    /// </summary>
    /// <param name="attributeValue"></param>
    /// <param name="entityModel"></param>
    public void SetFieldValue(AttributeValue attributeValue, T entityModel)
    {
        
    }
    
    // todo: handle indexes later, maybe those can be handled at the table level?
}