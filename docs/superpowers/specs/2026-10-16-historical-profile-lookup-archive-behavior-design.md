## Context

Issue #842 completes the profile-version behavior needed by existing files. A profile can be archived after files have recorded a profile version. New uploads must stop using the archived profile, while historical versions must remain readable for processing, audit, and troubleshooting.

The current aggregate already stores archive state separately from version state. `GetProfile` and `GetAllProfiles` exclude archived profiles, while `GetProfileVersion` and `GetProfileVersions` can read versions from an archived profile. The business manager already has methods for both historical lookups. The remaining work is to expose those queries consistently and lock the behavior down with tests.

## Goals

- Keep all historical versions readable after the parent profile is archived.
- Reject new uploads that request an archived profile.
- Allow existing files to continue processing through their recorded profile version.
- Return explicit not-found failures for unknown profiles or versions.
- Make repeated archival requests successful without appending duplicate archive events.
- Expose profile history through the HTTP API and client surface.

## Non-goals

- No new persistence projection or database schema changes; those belong to the separate projection work.
- No unarchive operation.
- No change to the immutable version model introduced by issues #839 and #840.
- No fallback from a missing recorded version to the current profile.

## Design

### Aggregate behavior

Keep `IsArchived` on the profile state and leave its `Versions` collection intact. `ArchiveProfile` returns success when the profile is already archived and only appends `FileProfileArchivedEvent` on the first archival. Active profile reads continue to hide archived profiles. Historical version reads continue to search the retained version collection regardless of archive state.

Add or strengthen aggregate tests covering:

- Archive hides the active profile but retains every historical version.
- A second archive call succeeds and does not produce another archive event.
- A missing profile or missing version returns an explicit not-found result at the manager boundary.

### Query/API surface

Add query records, request-handler methods, endpoint mappings, and client methods for:

- `GET /api/file-profiles/{fileProfileId}/versions`
- `GET /api/file-profiles/{fileProfileId}/versions/{fileProfileVersionId}`

The list endpoint returns all versions ordered by version number, including when the parent profile is archived. The detail endpoint returns the requested historical version and reports a not-found result when the profile or version does not exist. Existing active-profile endpoints retain their current archived-profile behavior.

### Upload and processing behavior

Do not add a second archive check in the upload pipeline. Upload validation must continue to resolve the active profile through `GetFileProfile`; an archived profile therefore fails before a new file is accepted.

Processing must continue to resolve `FileProfileVersionId` through the exact historical lookup path already used by the recorded-version work. The parent profile’s archive flag must not prevent that lookup. If the recorded version is unavailable, return the existing actionable error containing the file, profile, and version identifiers.

Add business-logic tests for both sides of the boundary:

- New upload against an archived profile is rejected and does not create an import-log entry.
- Existing file processing succeeds after profile archival when its recorded version remains available.

## Error handling

- Unknown profile on history lookup: `NotFound` identifying the profile ID.
- Unknown version on history lookup: `NotFound` identifying both profile and version IDs.
- Missing recorded version during processing: preserve the existing explicit diagnostic; never silently use the current active profile.
- Archive of an unknown profile: preserve the existing `NotFound` result.

## Verification

- Run focused aggregate, manager, handler/client, and business-logic tests while iterating.
- Run the complete solution build and test suite before completion.
- Run `git diff --check`.
- Report Docker-backed integration tests separately if the local Docker engine is unavailable.
