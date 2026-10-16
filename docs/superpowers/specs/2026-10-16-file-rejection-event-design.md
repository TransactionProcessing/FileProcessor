# File size rejection event design

## Status

Draft for review

## Intent

The file processor should accept files with up to 1,000 detail records and reject larger files in a way that is observable and auditable. A rejected file must not produce one `FileLineAddedEvent` per input line, because the current aggregate is rehydrated as each line event is processed and the cost grows badly for very large files.

The rejection should be represented in the file aggregate by a `FileRejectedEvent`, rather than by silently failing before an aggregate exists. This preserves a durable record of the file, its rejection reason, and the configured limit without creating a large or otherwise empty line collection.

## Context

The current processing flow creates the file aggregate and then adds each parsed file line. The 100,000-row experiment demonstrated that this causes aggregate rehydration to become the bottleneck. The 1,000-row test is the intended normal operating boundary and currently represents 1,002 physical lines: one header, 1,000 detail lines, and one trailer, with no trailing blank line.

## Proposed domain model

Add `FileRejectedEvent` to the file domain events. The event should contain:

- `FileId`, `EstateId`, and `MerchantId`, following the existing file event convention.
- A stable rejection code, initially `MaximumDetailRecordsExceeded`.
- A human-readable rejection reason.
- The observed detail-record count.
- The configured maximum detail-record count.
- The rejection timestamp.

The event is terminal for file processing. The aggregate should expose rejection state and details through `FileDetails`, including the rejection code/reason and the observed/configured counts. Existing line-processing summaries remain line-based; a size-rejected file has zero `FileLine` entries and is distinguished by its file-level rejection state.

The exact property names should follow the repository's existing model naming conventions when implemented. The stable code is the contract; the message is for diagnostics and user-facing explanation.

## Processing flow

1. `ProcessUploadedFile` creates the file aggregate as it does today, so the file identity and upload metadata are persisted.
2. Before calling `AddFileLine` for any input line, the service determines the number of detail records according to the selected file profile. Header and trailer records do not count toward the 1,000-detail-record limit. A trailing blank line is not treated as a detail record.
3. If the detail count is at most 1,000, processing continues unchanged. The existing 1,000-row scenario remains the boundary acceptance test.
4. If the detail count is greater than 1,000, the aggregate appends exactly one `FileRejectedEvent` and no `FileLineAddedEvent`s. Processing ends as a terminal rejection.
5. The file-management flow moves the physical file to the existing failed/rejected location using the same failure-path mechanism used for processing failures. The move must happen after the rejection event has been persisted, and retry/recovery must not append another rejection event.
6. A subsequent `GetFile` call returns the file-level rejection state and reason. It must not report the oversized file as successfully processed or as a partially processed 1,000-line file.

This intentionally does not attempt to solve the broader 100,000-row streaming or event-sourcing scalability problem. That can be a separate piece of work if the 1,000-row operational limit is later revisited.

## Configuration

Use a named setting with a default of `1000` detail records. The limit should be injected into the processing service rather than hard-coded in the parser or aggregate. Configuration validation should reject zero or negative values during application startup/options binding.

The service should pass the configured value into the rejection event so the historical decision remains understandable if the configured limit changes later.

## Idempotency and failure handling

- A file already marked rejected must not be parsed or have line events appended on a retry.
- If persistence fails while appending `FileRejectedEvent`, the physical file must remain recoverable in its current processing location and the operation must return the failure.
- If the event is persisted but the physical-file move fails, directory recovery must be able to find the file's terminal rejected state and complete or retry the move without creating a second rejection event.
- The existing successfully processed and line-level rejected paths must retain their current behavior.

## Tests

Add focused aggregate tests that verify:

- `FileRejectedEvent` rehydrates the aggregate as rejected with the recorded code, reason, observed count, and configured limit.
- A rejection cannot be appended before the file is created.
- A rejected aggregate cannot accept file lines or be marked processing-completed.
- Replaying the event preserves the same rejection data.

Add service tests that verify:

- 1,000 detail records proceed to normal line processing.
- 1,001 detail records append `FileCreatedEvent` followed by one `FileRejectedEvent` and append no line events.
- The physical file follows the failed/rejected path after the event is persisted.
- A retry of an already rejected file does not append duplicate events.

Extend the Reqnroll coverage with an opt-in scenario for 1,001 detail rows that asserts the file is rejected and exposes the configured limit/reason. Keep the existing 1,000-row scenario as the normal acceptance test. The generated fixture must continue to use unique complete detail lines, account numbers derived from the row number, amounts between 10 and 249, and no trailing blank line.

## Acceptance criteria

- A 1,000-detail-row file is accepted and processed as it is today.
- A 1,001-detail-row file is rejected before any line event is created.
- The rejection is represented by a durable `FileRejectedEvent` with a stable code and useful diagnostics.
- The file does not become an unobservable empty aggregate: its creation and rejection are both visible in the event stream and API response.
- The event stream for an oversized file is bounded and independent of the number of excess lines.
- Existing file-processing behavior and tests remain green.
