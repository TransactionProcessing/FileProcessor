# Profile Directory Version Safety Design

## Context

Issue #843 addresses directory changes after files have been accepted under an earlier file profile version. Profile versions already capture the listening directory, and issue #842 made historical versions available for lookup. The remaining risk is that directory synchronization, processing failure moves, and startup recovery still use only the active profile directory.

## Goals

- Keep accepted files associated with the directories recorded by their profile version.
- Ensure new files use the active profile version's directories.
- Discover historical `inprogress` files after a directory change.
- Resolve processed and failed destinations from the recorded profile version.
- Preserve historical directories and avoid silent file relocation.
- Define cleanup behavior explicitly.

## Non-goals

- Do not prohibit directory changes; a directory change creates a new immutable profile version.
- Do not move existing files between version directories automatically.
- Do not add an unarchive operation or a new persistence projection.
- Do not implement automatic deletion of historical directories.

## Design

### Historical directory discovery

Add a profile-manager operation that returns profile versions across the complete aggregate, including archived profiles. Directory synchronization will ensure `inprogress`, `processed`, and `failed` directories for every returned version, deduplicating paths that are shared by multiple versions.

The synchronizer will create directories only. It will not delete, rename, or migrate files from historical locations.

### File processing destinations

Every post-acceptance directory operation will resolve the profile version recorded on the file/import-log command. This includes the failure path that runs before normal processing and the existing processed/failed moves after processing. A missing recorded version remains an explicit failure; it must never fall back to the active profile.

### In-progress recovery

Startup recovery will enumerate the `inprogress` directory for every historical profile version, including versions belonging to archived profiles. Duplicate physical paths will be scanned once. Each discovered file will continue to obtain its file profile/version identifiers from the file aggregate or import-log read model, and replay will use those recorded identifiers.

### Retention and cleanup

Historical profile-version directories are retained indefinitely by application behavior. No automatic cleanup is performed because files may still be in flight, awaiting recovery, or required for audit/replay. Removing an old directory is an explicit operational action outside this issue, after confirming that no accepted or recoverable files remain there.

## Error handling

- A missing historical profile version returns the existing explicit not-found failure with the file, profile, and version identifiers where processing has that context.
- A failure to resolve a recorded version must not redirect the file to a current profile directory.
- Recovery skips malformed or unresolvable files as it does today and logs the reason.
- Directory creation failures continue to surface through the synchronizer result.

## Verification

Add tests covering:

- Synchronization creates current and historical directories, including archived-profile versions, and does not remove old directories.
- A successfully processed file moves to the recorded version's processed directory after the active profile directory changes.
- A rejected or failed file moves to the recorded version's failed directory.
- Recovery finds old and current `inprogress` directories and replays commands with recorded profile/version metadata.
- Duplicate historical versions pointing to one physical directory are processed once.
- Missing historical versions fail without attempting a current-directory fallback.

Run the focused aggregate/business-logic tests, the relevant integration tests, the complete solution build/test suite, and `git diff --check`.
