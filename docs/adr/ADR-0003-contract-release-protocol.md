# ADR-0003: Releases are GitHub tags with the .nupkg attached

**Status:** Accepted  
**Date:** 2026-10-12

## Context

Consumers need the contract package without a private NuGet feed (GitHub free tier, study-project budget). Options: (a) GitHub Packages as feed (requires `GITHUB_TOKEN` auth in every consumer build, including Docker), (b) NuGet.org (public feed — would publish study code to the public gallery with retention implications), (c) tag the repo and attach the `.nupkg` to the GitHub Release; consumers clone the tag and pack locally.

Option (a) complicates CI and Docker builds with token plumbing; option (b) publishes to the world with no unpublish. Option (c) needs no credentials on the consumer side.

## Decision

Every contract release is a git tag `vX.Y.Z`. The `release` job in CI (`ci.yml`) runs `dotnet pack` on the tag build and uploads `LogPipe.Contracts.X.Y.Z.nupkg` as a Release asset. Consumers obtain the package via `fetch-contracts.ps1 -Ref vX.Y.Z` (clone → pack → local `./packages` feed) or a Dockerfile `ADD ...#vX.Y.Z` + `dotnet pack` step.

## Consequences

- No secrets or tokens required by any consumer build — the contract is fetched from the public git repo, not a package feed.
- The tag is the immutable source of truth; the Release asset is a convenience (rebuildable from the tag at any time).
- Version discipline is enforced by the pinned `<Version>` in the csproj plus the SemVer rule: breaking wire change ⇒ major bump (ADR-0001).
