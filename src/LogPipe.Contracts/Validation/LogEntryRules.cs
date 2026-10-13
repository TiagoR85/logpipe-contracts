using System.Globalization;
using LogPipe.Contracts.V1;

namespace LogPipe.Contracts.Validation;

/// <summary>
/// Validation rules shared by logpipe-ingest (REST ingress) and logpipe-search (gRPC ingress).
/// The contract package owns them so both services cannot drift.
/// </summary>
public static class LogEntryRules
{
    public static readonly IReadOnlySet<string> Levels =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Trace", "Debug", "Information", "Warning", "Error", "Critical"
        };

    public const int MaxMessageLength = 8192;
    public const int MaxServiceLength = 64;
    public const int MaxAttributes = 32;
    public const int MaxAttributeValueLength = 1024;
    public const int MaxEntriesPerBatch = 1000;

    public static bool TryValidate(LogEntry entry, out string? reason)
    {
        if (string.IsNullOrWhiteSpace(entry.Level) || !Levels.Contains(entry.Level))
        {
            reason = $"unknown level '{entry.Level}'";
            return false;
        }

        if (string.IsNullOrWhiteSpace(entry.Service) || entry.Service.Length > MaxServiceLength)
        {
            reason = $"service must be 1..{MaxServiceLength} chars";
            return false;
        }

        if (string.IsNullOrWhiteSpace(entry.Message) || entry.Message.Length > MaxMessageLength)
        {
            reason = $"message must be 1..{MaxMessageLength} chars";
            return false;
        }

        if (!string.IsNullOrEmpty(entry.Timestamp) &&
            !DateTimeOffset.TryParse(entry.Timestamp, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out _))
        {
            reason = $"timestamp '{entry.Timestamp}' is not RFC 3339";
            return false;
        }

        if (!string.IsNullOrEmpty(entry.TraceId) && !IsHex(entry.TraceId, 32))
        {
            reason = "trace_id must be 32 hex chars";
            return false;
        }

        if (!string.IsNullOrEmpty(entry.SpanId) && !IsHex(entry.SpanId, 16))
        {
            reason = "span_id must be 16 hex chars";
            return false;
        }

        if (entry.Attributes.Count > MaxAttributes)
        {
            reason = $"at most {MaxAttributes} attributes";
            return false;
        }

        foreach (var (key, value) in entry.Attributes)
        {
            if (string.IsNullOrWhiteSpace(key) || key.Length > 64 ||
                string.IsNullOrEmpty(value) || value.Length > MaxAttributeValueLength)
            {
                reason = "attribute key/value out of bounds";
                return false;
            }
        }

        reason = null;
        return true;
    }

    private static bool IsHex(string value, int expectedLength)
    {
        if (value.Length != expectedLength) return false;
        foreach (var c in value)
        {
            var hex = c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';
            if (!hex) return false;
        }
        return true;
    }
}
