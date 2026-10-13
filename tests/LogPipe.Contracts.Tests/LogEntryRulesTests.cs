using LogPipe.Contracts.Validation;
using LogPipe.Contracts.V1;

namespace LogPipe.Contracts.Tests;

public class LogEntryRulesTests
{
    private static LogEntry Valid() => new()
    {
        Id = Guid.NewGuid().ToString(),
        Timestamp = "2026-10-12T18:03:22.123Z",
        Level = "Information",
        Service = "checkout",
        Message = "order created"
    };

    [Fact]
    public void Valid_entry_passes() => Assert.True(LogEntryRules.TryValidate(Valid(), out _));

    [Theory]
    [InlineData("Bogus")]
    [InlineData("")]
    [InlineData("information ")]
    public void Unknown_level_fails(string level)
    {
        var e = Valid(); e.Level = level;
        Assert.False(LogEntryRules.TryValidate(e, out var reason));
        Assert.Contains("level", reason);
    }

    [Fact]
    public void Level_matching_is_case_insensitive()
    {
        var e = Valid(); e.Level = "error";
        Assert.True(LogEntryRules.TryValidate(e, out _));
    }

    [Fact]
    public void Empty_service_or_message_fails()
    {
        var e = Valid(); e.Service = " ";
        Assert.False(LogEntryRules.TryValidate(e, out _));
        e = Valid(); e.Message = "";
        Assert.False(LogEntryRules.TryValidate(e, out _));
    }

    [Fact]
    public void Overlong_message_fails()
    {
        var e = Valid(); e.Message = new string('x', LogEntryRules.MaxMessageLength + 1);
        Assert.False(LogEntryRules.TryValidate(e, out _));
    }

    [Fact]
    public void Overlong_service_fails()
    {
        var e = Valid(); e.Service = new string('s', LogEntryRules.MaxServiceLength + 1);
        Assert.False(LogEntryRules.TryValidate(e, out _));
    }

    [Theory]
    [InlineData("nothex")]
    [InlineData("aabb")]
    [InlineData("zz0000000000000000000000000000000")]
    public void Malformed_trace_id_fails(string traceId)
    {
        var e = Valid(); e.TraceId = traceId;
        Assert.False(LogEntryRules.TryValidate(e, out _));
    }

    [Fact]
    public void Empty_trace_and_span_ids_are_allowed()
    {
        var e = Valid(); e.TraceId = ""; e.SpanId = "";
        Assert.True(LogEntryRules.TryValidate(e, out _));
    }

    [Fact]
    public void Malformed_span_id_fails()
    {
        var e = Valid(); e.SpanId = "xyz";
        Assert.False(LogEntryRules.TryValidate(e, out _));
    }

    [Fact]
    public void Too_many_attributes_fails()
    {
        var e = Valid();
        for (var i = 0; i <= LogEntryRules.MaxAttributes; i++) e.Attributes[$"k{i}"] = "v";
        Assert.False(LogEntryRules.TryValidate(e, out _));
    }

    [Fact]
    public void Overlong_attribute_value_fails()
    {
        var e = Valid();
        e.Attributes["big"] = new string('v', LogEntryRules.MaxAttributeValueLength + 1);
        Assert.False(LogEntryRules.TryValidate(e, out _));
    }

    [Fact]
    public void Unparseable_timestamp_fails()
    {
        var e = Valid(); e.Timestamp = "yesterday-ish";
        Assert.False(LogEntryRules.TryValidate(e, out _));
    }
}
