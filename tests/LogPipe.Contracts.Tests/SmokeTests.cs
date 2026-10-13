using System.Linq;
using Google.Protobuf;
using LogPipe.Contracts.V1;

namespace LogPipe.Contracts.Tests;

public class SmokeTests
{
    [Fact]
    public void Generated_client_and_server_stubs_exist()
    {
        var clientCtor = typeof(LogIngest.LogIngestClient)
            .GetConstructor(new[] { typeof(Grpc.Net.Client.GrpcChannel) });
        Assert.NotNull(clientCtor);
        Assert.NotNull(typeof(LogIngest.LogIngestBase));
    }

    [Fact]
    public void LogEntry_round_trips_over_protobuf()
    {
        var entry = new LogEntry
        {
            Id = Guid.NewGuid().ToString(),
            Timestamp = "2026-10-12T18:03:22.123Z",
            Level = "Error",
            Service = "checkout",
            Message = "payment timeout",
            TraceId = new string('a', 32),
            SpanId = new string('b', 16)
        };
        entry.Attributes["region"] = "eu-west";

        var parsed = LogEntry.Parser.ParseFrom(entry.ToByteArray());

        Assert.Equal(entry.Message, parsed.Message);
        Assert.Equal("eu-west", parsed.Attributes["region"]);
        Assert.Equal(8, LogEntry.Descriptor.Fields.InFieldNumberOrder().Count()); // 7 scalars + map entry field
    }

    [Fact]
    public void Service_descriptor_exposes_StreamLogs()
    {
        var method = LogIngest.Descriptor.Methods.SingleOrDefault(m => m.Name == "StreamLogs");
        Assert.NotNull(method);
        Assert.True(method.IsClientStreaming);
        Assert.False(method.IsServerStreaming);
    }
}
