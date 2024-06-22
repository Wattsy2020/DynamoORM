using Amazon.DynamoDBv2.Model;
using FluentAssertions;

namespace DynamoORM.UT;

public class DynamoTableFieldTests
{
    private readonly DynamoTableField<TestEntity> _stringField = new (nameof(TestEntity.TestString));
    private readonly DynamoTableField<TestEntity> _charField = new (nameof(TestEntity.TestChar));
    private readonly DynamoTableField<TestEntity> _intField = new (nameof(TestEntity.TestInt));
    private readonly DynamoTableField<TestEntity> _shortField = new (nameof(TestEntity.TestShort));
    private readonly DynamoTableField<TestEntity> _longField = new (nameof(TestEntity.TestLong));
    private readonly DynamoTableField<TestEntity> _dateTimeField = new (nameof(TestEntity.TestDateTime));
    private readonly DynamoTableField<TestEntity> _enumField = new(nameof(TestEntity.TestEnum));

    [Fact]
    public void TestEntityFieldsHaveCorrectDynamoDataType()
    {
        _stringField.DynamoType.Should().Be(DynamoDataType.String);
        _charField.DynamoType.Should().Be(DynamoDataType.String);
        _intField.DynamoType.Should().Be(DynamoDataType.Number);
        _shortField.DynamoType.Should().Be(DynamoDataType.Number);
        _longField.DynamoType.Should().Be(DynamoDataType.Number);
        _dateTimeField.DynamoType.Should().Be(DynamoDataType.Number);
        _enumField.DynamoType.Should().Be(DynamoDataType.String);
    }

    [Fact]
    public void TestGetFieldValue()
    {
        var exampleEntity = new TestEntity
        {
            TestString = "Test",
            TestChar = 'T',
            TestInt = 0,
            TestShort = 1,
            TestLong = 2,
            TestDateTime = new DateTime(2024, 1, 1),
            TestEnum = TestEnum.EnumValue2
        };

        _stringField.GetFieldValue(exampleEntity)
            .Should().BeAttributeValue(new AttributeValue { S = "Test" });
        _charField.GetFieldValue(exampleEntity)
            .Should().BeAttributeValue(new AttributeValue { S = "T" });
        _intField.GetFieldValue(exampleEntity)
            .Should().BeAttributeValue(new AttributeValue { N = "0" });
        _shortField.GetFieldValue(exampleEntity)
            .Should().BeAttributeValue(new AttributeValue { N = "1" });
        _longField.GetFieldValue(exampleEntity)
            .Should().BeAttributeValue(new AttributeValue { N = "2" });
        _dateTimeField.GetFieldValue(exampleEntity)
            .Should().BeAttributeValue(new AttributeValue { N = "638396640000000000" });
        _enumField.GetFieldValue(exampleEntity)
            .Should().BeAttributeValue(new AttributeValue { S = "EnumValue2" });
    }
    
    [Fact]
    public void TestSetFieldValue()
    {
        var exampleEntity = new TestEntity();
        
        _stringField.SetFieldValue(new AttributeValue { S = "Test" }, exampleEntity);
        exampleEntity.TestString.Should().Be("Test");
        
        _charField.SetFieldValue(new AttributeValue { S = "T" }, exampleEntity);
        exampleEntity.TestChar.Should().Be('T');
        
        _intField.SetFieldValue(new AttributeValue { N = "0" }, exampleEntity);
        exampleEntity.TestInt.Should().Be(0);
        
        _shortField.SetFieldValue(new AttributeValue { N = "1" }, exampleEntity);
        exampleEntity.TestShort.Should().Be(1);
        
        _longField.SetFieldValue(new AttributeValue { N = "2" }, exampleEntity);
        exampleEntity.TestLong.Should().Be(2);
        
        _dateTimeField.SetFieldValue(new AttributeValue { N = "638396640000000000" }, exampleEntity);
        exampleEntity.TestDateTime.Should().Be(new DateTime(2024, 01, 01));
        
        _enumField.SetFieldValue(new AttributeValue { S = "EnumValue3" }, exampleEntity);
        exampleEntity.TestEnum.Should().Be(TestEnum.EnumValue3);
    }
    
    private class TestEntity
    {
        public string TestString { get; set; }
        public char TestChar { get; set; }
        public int TestInt { get; set; }
        public short TestShort { get; set; }
        public long TestLong { get; set; }
        public DateTime TestDateTime { get; set; }
        public TestEnum TestEnum { get; set; }
    }

    private enum TestEnum
    {
        EnumValue1,
        EnumValue2,
        EnumValue3
    }
}
