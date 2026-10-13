# ADR-0001: The .proto lives here and ships as a versioned NuGet package

**Status:** Accepted  
**Date:** 2026-10-12

## Context

The logpipe family (ingest, search) needs one wire contract for the gRPC ingest stream. The options were: (a) duplicate the .proto in each service repo and `Protobuf Include` it locally, (b) a separate `logpipe-contracts` repo publishing a versioned NuGet package, or (c) a shared folder synced by script.

Option (a) drifts silently — the two services can compile against different messages. Option (c) has no versioning story and breaks Docker builds. Option (b) gives every consumer an immutable, SemVer-pinned artifact.

## Decision

The `.proto` (and the C# stubs generated from it) live in this repo and are published as the `LogPipe.Contracts` NuGet package. Consumers pin the exact version in `Directory.Packages.props` and fetch it via a local feed (no private NuGet feed — see [ADR-0003](ADR-0003-contract-release-protocol.md)).

## Consequences

- Breaking wire changes are a major version bump; consumers opt in explicitly.
- `fetch-contracts.ps1` (in each consumer repo) clones the tag, packs into `./packages`, and the consuming `nuget.config` points at that relative feed — works locally and in CI/Docker with no secrets.
- Contract changes land in one repo with its own tests (round-trip serialization, stub existence, `LogEntryRules` validation), reviewed before any consumer sees them.
