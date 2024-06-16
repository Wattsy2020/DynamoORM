using Amazon.DynamoDBv2.Model;

namespace DynamoDB;

public class JobReport
{
    public string? UserId { get; init; }
    public DateTime? JobCreationTimestamp { get; init; }
    public JobStatus? JobStatus { get; init; }
    public string? ReportS3Bucket { get; init; }
    public string? ReportS3Key { get; init; }
    public DateTime? ReportCreationTimestamp { get; init; }
    public string? FilterParameters { get; init; }

    public bool IsValid()
        => UserId != null
           && JobCreationTimestamp != null
           && JobStatus != null;

    public override string ToString()
        => $"""
            JobReport
            {"{"}
                {nameof(UserId)} = {UserId}
                {nameof(JobCreationTimestamp)} = {JobCreationTimestamp}
                {nameof(JobStatus)} = {JobStatus}
                {nameof(ReportS3Bucket)} = {ReportS3Bucket}
                {nameof(ReportS3Key)} = {ReportS3Key}
                {nameof(ReportCreationTimestamp)} = {ReportCreationTimestamp}
                {nameof(FilterParameters)} = {FilterParameters} 
            {"}"}
            """;
    
    // todo: could write this using LINQ and reflection
    // the reflection should check if a variable is annotated as nullable, if it isn't then it should enforce its always present
    public Dictionary<string, AttributeValue> ToAttributes()
    {
        var attributes = new Dictionary<string, AttributeValue>
        {
            { nameof(UserId), new AttributeValue { S = UserId } },
            { nameof(JobCreationTimestamp), new AttributeValue { N = JobCreationTimestamp!.Value.Ticks.ToString() }},
            { nameof(JobStatus), new AttributeValue { S = JobStatus.ToString() }},
        };

        if (ReportS3Bucket != null)
        {
            attributes[nameof(ReportS3Bucket)] = new AttributeValue { S = ReportS3Bucket };
        }

        if (ReportS3Key != null)
        {
            attributes[nameof(ReportS3Key)] = new AttributeValue { S = ReportS3Key };
        }

        if (ReportCreationTimestamp != null)
        {
            attributes[nameof(ReportCreationTimestamp)] = new AttributeValue
                { N = ReportCreationTimestamp!.Value.Ticks.ToString() };
        }

        if (FilterParameters != null)
        {
            attributes[nameof(FilterParameters)] = new AttributeValue { S = FilterParameters };
        }

        return attributes;
    }

    public static JobReport FromAttributes(Dictionary<string, AttributeValue> attributes)
    {
        if (!attributes.TryGetValue(nameof(UserId), out var userId))
        {
            throw new InvalidDynamoDataException($"Required field {nameof(UserId)} was not found");
        }
        
        if (!attributes.TryGetValue(nameof(JobCreationTimestamp), out var jobCreationTimestamp))
        {
            throw new InvalidDynamoDataException($"Required field {nameof(UserId)} was not found");
        }
        
        if (!attributes.TryGetValue(nameof(JobStatus), out var jobStatus))
        {
            throw new InvalidDynamoDataException($"Required field {nameof(UserId)} was not found");
        }

        attributes.TryGetValue(nameof(ReportS3Bucket), out var reportS3Bucket);
        attributes.TryGetValue(nameof(ReportS3Key), out var reportS3Key);
        attributes.TryGetValue(nameof(ReportCreationTimestamp), out var reportCreationTimestamp);
        attributes.TryGetValue(nameof(FilterParameters), out var filterParameters);

        // TODO: fill in the rest
        return new JobReport
        {
            UserId = userId.S ?? throw new InvalidDynamoDataException($"{nameof(UserId)} has the wrong type"),
            JobCreationTimestamp = ParseDateTime(jobCreationTimestamp),
            JobStatus = ParseEnum<JobStatus>(jobStatus),
            ReportS3Bucket = reportS3Bucket?.S,
            ReportS3Key = reportS3Key?.S,
            ReportCreationTimestamp = reportCreationTimestamp is null? null : ParseDateTime(reportCreationTimestamp),
            FilterParameters = filterParameters?.S
        };
    }

    private static DateTime ParseDateTime(AttributeValue value)
    {
        var ticks = value.N ?? throw new InvalidDynamoDataException("Expected a numeric type");
        if (!long.TryParse(ticks, out var ticksLong))
        {
            throw new InvalidDynamoDataException($"Expected an integer type, instead received: {ticks}");
        }
        return new DateTime(ticks: ticksLong);
    }

    private static T ParseEnum<T>(AttributeValue value) where T: struct
    {
        var enumString = value.S ?? throw new InvalidDynamoDataException("Expected a string type");
        if (!Enum.TryParse<T>(enumString, out var enumValue))
        {
            throw new InvalidDynamoDataException($"{enumString} is not a valid enum of {typeof(T)}");
        }

        return enumValue;
    }
}

public enum JobStatus
{
    Unprocessed,
    Processing,
    Processed
}