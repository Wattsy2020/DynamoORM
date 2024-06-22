using Amazon.DynamoDBv2.Model;
using FluentAssertions;
using FluentAssertions.Primitives;

namespace DynamoORM.UT;

public class AttributeValueComparer : IEqualityComparer<AttributeValue>
{
    public bool Equals(AttributeValue? x, AttributeValue? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.BOOL == y.BOOL 
               && x.N == y.N  
               && x.NULL == y.NULL 
               && x.S == y.S;
    }

    public int GetHashCode(AttributeValue obj)
    {
        var hashCode = new HashCode();
        hashCode.Add(obj.BOOL);
        hashCode.Add(obj.N);
        hashCode.Add(obj.NULL);
        hashCode.Add(obj.S);
        return hashCode.ToHashCode();
    }
}

public static class AssertionExtensions
{
    public static AndConstraint<ObjectAssertions> BeAttributeValue(
        this ObjectAssertions objectAssertions,
        AttributeValue expected,
        string because = "",
        params object[] becauseArgs)
        => objectAssertions.Be(expected, new AttributeValueComparer(), because, becauseArgs);
}
