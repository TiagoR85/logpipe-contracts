# ADR-0002: One package with client and server stubs (`GrpcServices="Both"`)

**Status:** Accepted  
**Date:** 2026-10-12

## Context

`logpipe-ingest` is the gRPC **client** (it forwards log batches upstream); `logpipe-search` is the gRPC **server** (it hosts the ingest endpoint for downstream agents). Options were: (a) two packages (`LogPipe.Contracts.Client` / `LogPipe.Contracts.Server`), or (b) one package compiling the proto with `GrpcServices="Both"`.

Two packages double the release surface and force consumers that need both (tests, tooling) to coordinate two version pins.

## Decision

One package, one version. The csproj compiles the proto with `GrpcServices="Both"`, shipping `LogIngest.LogIngestClient` (for ingest's forwarder and any test harness) and `LogIngest.LogIngestBase` (implemented by search's gRPC service). Server-only consumers can ignore the client stub and vice versa.

## Consequences

- One version number covers both sides; a wire change is tested once, against both stubs, in this repo's test suite.
- The generated code is excluded from this repo's coverage denominator (`[LogPipe.Contracts]LogPipe.Contracts.V1.*`) — only hand-written code (`LogEntryRules`) is measured.
