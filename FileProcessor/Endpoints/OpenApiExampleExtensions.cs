#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace FileProcessor.Endpoints;

/// <summary>Adds the documented request, parameter, and response examples to an endpoint.</summary>
public static class OpenApiExampleExtensions
{
    private static readonly IReadOnlyDictionary<string, EndpointExamples> Examples = new Dictionary<string, EndpointExamples>
    {
        ["DomainEvent"] = new()
        {
            RequestBody = """
                {
                  "fileId": "c31f6a4b-8cd1-4d7e-9a2c-1f8a5b6d7e90",
                  "estateId": "435613ac-a468-47a3-ac4f-649d89764c22",
                  "merchantId": "ab1c99fb-1c6c-4694-9a32-b71be5d1da33",
                  "lineNumber": 1,
                  "fileLine": "D,Example Operator,01234567890,10.00",
                  "EventId": "42cae457-c061-42d0-b544-f1767d812ae5"
                }
                """,
            Parameters = new Dictionary<string, object> { ["eventType"] = "FileLineAddedEvent" },
            Responses = new Dictionary<string, string>
            {
                ["500"] = """
                    {
                      "type": "about:blank",
                      "title": "One or more event handlers failed",
                      "status": 500,
                      "eventId": "42cae457-c061-42d0-b544-f1767d812ae5",
                      "eventType": "FileLineAddedEvent",
                      "failures": [
                        { "handler": "FileDomainEventHandler", "error": "The transaction could not be processed." }
                      ]
                    }
                    """
            }
        },
        ["MigrateProfileVersions"] = new()
        {
            Responses = new Dictionary<string, string>
            {
                ["200"] = """
                    {
                      "profiles_checked": 12,
                      "profiles_with_initial_version": 12,
                      "files_checked": 48,
                      "files_assigned": 45,
                      "files_skipped": 3,
                      "failures": []
                    }
                    """
            }
        },
        ["UploadFile"] = new()
        {
            RequestBody = """
                {
                  "file_id": "c31f6a4b-8cd1-4d7e-9a2c-1f8a5b6d7e90",
                  "estate_id": "435613ac-a468-47a3-ac4f-649d89764c22",
                  "merchant_id": "ab1c99fb-1c6c-4694-9a32-b71be5d1da33",
                  "user_id": "a782891c-5c7d-45de-b002-1585a6db28df",
                  "file_profile_id": "9c89c59e-8608-47b1-b280-052a75bcf84b",
                  "upload_date_time": "2026-10-22T09:30:00Z",
                  "file": "H,Operator,Phone,Amount\nD,Example Operator,01234567890,10.00"
                }
                """,
            Responses = new Dictionary<string, string>
            {
                ["202"] = "\"c31f6a4b-8cd1-4d7e-9a2c-1f8a5b6d7e90\"",
                ["400"] = "\"The uploaded file must not be empty.\""
            }
        },
        ["GetFile"] = new()
        {
            Parameters = new Dictionary<string, object>
            {
                ["fileId"] = "c31f6a4b-8cd1-4d7e-9a2c-1f8a5b6d7e90",
                ["estateId"] = "435613ac-a468-47a3-ac4f-649d89764c22"
            },
            Responses = new Dictionary<string, string>
            {
                ["200"] = """
                    {
                      "file_id": "c31f6a4b-8cd1-4d7e-9a2c-1f8a5b6d7e90",
                      "processing_completed": true,
                      "is_rejected": false,
                      "rejection_code": null,
                      "rejection_reason": null,
                      "observed_detail_record_count": 1,
                      "maximum_detail_record_count": 1000,
                      "estate_id": "435613ac-a468-47a3-ac4f-649d89764c22",
                      "user_id": "a782891c-5c7d-45de-b002-1585a6db28df",
                      "user_email_address": "operator@example.com",
                      "merchant_id": "ab1c99fb-1c6c-4694-9a32-b71be5d1da33",
                      "merchant_name": "Example Merchant",
                      "file_profile_id": "9c89c59e-8608-47b1-b280-052a75bcf84b",
                      "file_profile_version_id": "469752f8-e6f8-42c3-bff9-a79717c11d84",
                      "file_profile_name": "Voucher transactions",
                      "file_import_log_id": "9eeab611-33a4-4e31-9e84-b61d1302f743",
                      "file_location": "files/2026/10/vouchers.csv",
                      "file_lines": [
                        {
                          "line_data": "D,Example Operator,01234567890,10.00",
                          "line_number": 1,
                          "processing_result": 2,
                          "transaction_id": "cb95782a-1c7f-49bb-a96d-8cf4dbeb1f57",
                          "event_id": "42cae457-c061-42d0-b544-f1767d812ae5",
                          "rejection_reason": null,
                          "failure_reason": null,
                          "response_code": "0000",
                          "dispatch_attempt_count": 1,
                          "failed_dispatch_attempts": []
                        }
                      ],
                      "processing_summary": {
                        "failed_lines": 0,
                        "ignored_lines": 0,
                        "not_processed_lines": 0,
                        "rejected_lines": 0,
                        "successfully_processed_lines": 1,
                        "total_lines": 1
                      }
                    }
                    """
            }
        },
        ["ReplayLine"] = new()
        {
            Parameters = new Dictionary<string, object>
            {
                ["fileId"] = "c31f6a4b-8cd1-4d7e-9a2c-1f8a5b6d7e90",
                ["estateId"] = "435613ac-a468-47a3-ac4f-649d89764c22",
                ["lineNumber"] = 1
            }
        },
        ["GetImportLogs"] = new()
        {
            Parameters = new Dictionary<string, object>
            {
                ["estateId"] = "435613ac-a468-47a3-ac4f-649d89764c22",
                ["startDate"] = "2026-10-21",
                ["endDate"] = "2026-10-22",
                ["merchantId"] = "ab1c99fb-1c6c-4694-9a32-b71be5d1da33",
                ["pageNumber"] = 1,
                ["pageSize"] = 50,
                ["descending"] = true
            },
            Responses = new Dictionary<string, string>
            {
                ["200"] = """
                    {
                      "file_import_logs": [
                        {
                          "file_import_log_id": "9eeab611-33a4-4e31-9e84-b61d1302f743",
                          "import_log_date_time": "2026-10-22T09:30:00Z",
                          "import_log_date": "2026-10-22T00:00:00Z",
                          "import_log_time": "09:30:00",
                          "file_count": 1,
                          "files": [
                            {
                              "file_id": "c31f6a4b-8cd1-4d7e-9a2c-1f8a5b6d7e90",
                              "file_import_log_id": "9eeab611-33a4-4e31-9e84-b61d1302f743",
                              "file_path": "files/2026/10/vouchers.csv",
                              "file_profile_id": "9c89c59e-8608-47b1-b280-052a75bcf84b",
                              "file_profile_version_id": "469752f8-e6f8-42c3-bff9-a79717c11d84",
                              "file_uploaded_date_time": "2026-10-22T09:30:00Z",
                              "merchant_id": "ab1c99fb-1c6c-4694-9a32-b71be5d1da33",
                              "original_file_name": "vouchers.csv",
                              "user_id": "a782891c-5c7d-45de-b002-1585a6db28df"
                            }
                          ]
                        }
                      ],
                      "page_number": 1,
                      "page_size": 50,
                      "total_count": 1,
                      "has_more": false
                    }
                    """
            }
        },
        ["GetImportLog"] = new()
        {
            Parameters = new Dictionary<string, object>
            {
                ["fileImportLogId"] = "9eeab611-33a4-4e31-9e84-b61d1302f743",
                ["estateId"] = "435613ac-a468-47a3-ac4f-649d89764c22",
                ["merchantId"] = "ab1c99fb-1c6c-4694-9a32-b71be5d1da33"
            },
            Responses = new Dictionary<string, string>
            {
                ["200"] = """
                    {
                      "file_import_log_id": "9eeab611-33a4-4e31-9e84-b61d1302f743",
                      "import_log_date_time": "2026-10-22T09:30:00Z",
                      "import_log_date": "2026-10-22T00:00:00Z",
                      "import_log_time": "09:30:00",
                      "file_count": 1,
                      "files": [
                        {
                          "file_id": "c31f6a4b-8cd1-4d7e-9a2c-1f8a5b6d7e90",
                          "file_import_log_id": "9eeab611-33a4-4e31-9e84-b61d1302f743",
                          "file_path": "files/2026/10/vouchers.csv",
                          "file_profile_id": "9c89c59e-8608-47b1-b280-052a75bcf84b",
                          "file_profile_version_id": "469752f8-e6f8-42c3-bff9-a79717c11d84",
                          "file_uploaded_date_time": "2026-10-22T09:30:00Z",
                          "merchant_id": "ab1c99fb-1c6c-4694-9a32-b71be5d1da33",
                          "original_file_name": "vouchers.csv",
                          "user_id": "a782891c-5c7d-45de-b002-1585a6db28df"
                        }
                      ]
                    }
                    """
            }
        },
        ["GetFileProfiles"] = new()
        {
            Responses = new Dictionary<string, string>
            {
                ["200"] = """
                    [
                      {
                        "file_profile_id": "9c89c59e-8608-47b1-b280-052a75bcf84b",
                        "file_profile_version_id": "469752f8-e6f8-42c3-bff9-a79717c11d84",
                        "file_profile_version": 1,
                        "name": "Voucher transactions",
                        "operator_name": "Example Operator",
                        "listening_directory": "files/incoming/vouchers",
                        "processed_directory": "files/incoming/vouchers/processed",
                        "failed_directory": "files/incoming/vouchers/failed",
                        "file_format_handler": "CsvFileFormatHandler",
                        "request_type": "Voucher",
                        "line_terminator": "\n"
                      }
                    ]
                    """
            }
        },
        ["CreateFileProfile"] = new()
        {
            RequestBody = """
                {
                  "file_profile_id": "9c89c59e-8608-47b1-b280-052a75bcf84b",
                  "name": "Voucher transactions",
                  "listening_directory": "files/incoming/vouchers",
                  "request_type": "Voucher",
                  "operator_name": "Example Operator",
                  "line_terminator": "LineFeed",
                  "file_format_handler": "CsvFileFormatHandler"
                }
                """,
            Responses = new Dictionary<string, string>
            {
                ["200"] = """
                    {
                      "file_profile_id": "9c89c59e-8608-47b1-b280-052a75bcf84b",
                      "file_profile_version_id": "469752f8-e6f8-42c3-bff9-a79717c11d84",
                      "file_profile_version": 1,
                      "name": "Voucher transactions",
                      "operator_name": "Example Operator",
                      "listening_directory": "files/incoming/vouchers",
                      "processed_directory": "files/incoming/vouchers/processed",
                      "failed_directory": "files/incoming/vouchers/failed",
                      "file_format_handler": "CsvFileFormatHandler",
                      "request_type": "Voucher",
                      "line_terminator": "\n"
                    }
                    """
            }
        },
        ["GetFileProfile"] = new()
        {
            Parameters = new Dictionary<string, object> { ["fileProfileId"] = "9c89c59e-8608-47b1-b280-052a75bcf84b" },
            Responses = new Dictionary<string, string>
            {
                ["200"] = """
                    {
                      "file_profile_id": "9c89c59e-8608-47b1-b280-052a75bcf84b",
                      "file_profile_version_id": "469752f8-e6f8-42c3-bff9-a79717c11d84",
                      "file_profile_version": 1,
                      "name": "Voucher transactions",
                      "operator_name": "Example Operator",
                      "listening_directory": "files/incoming/vouchers",
                      "processed_directory": "files/incoming/vouchers/processed",
                      "failed_directory": "files/incoming/vouchers/failed",
                      "file_format_handler": "CsvFileFormatHandler",
                      "request_type": "Voucher",
                      "line_terminator": "\n"
                    }
                    """
            }
        },
        ["UpdateFileProfile"] = new()
        {
            Parameters = new Dictionary<string, object> { ["fileProfileId"] = "9c89c59e-8608-47b1-b280-052a75bcf84b" },
            RequestBody = """
                {
                  "name": "Voucher transactions",
                  "listening_directory": "files/incoming/vouchers",
                  "request_type": "Voucher",
                  "operator_name": "Example Operator",
                  "line_terminator": "LineFeed",
                  "file_format_handler": "CsvFileFormatHandler"
                }
                """,
            Responses = new Dictionary<string, string>
            {
                ["200"] = """
                    {
                      "file_profile_id": "9c89c59e-8608-47b1-b280-052a75bcf84b",
                      "file_profile_version_id": "7738df59-9b15-44cb-8d7a-df4845bb8bd4",
                      "file_profile_version": 2,
                      "name": "Voucher transactions",
                      "operator_name": "Example Operator",
                      "listening_directory": "files/incoming/vouchers",
                      "processed_directory": "files/incoming/vouchers/processed",
                      "failed_directory": "files/incoming/vouchers/failed",
                      "file_format_handler": "CsvFileFormatHandler",
                      "request_type": "Voucher",
                      "line_terminator": "\n"
                    }
                    """
            }
        },
        ["ArchiveFileProfile"] = new()
        {
            Parameters = new Dictionary<string, object> { ["fileProfileId"] = "9c89c59e-8608-47b1-b280-052a75bcf84b" }
        },
        ["GetFileProfileVersions"] = new()
        {
            Parameters = new Dictionary<string, object> { ["fileProfileId"] = "9c89c59e-8608-47b1-b280-052a75bcf84b" },
            Responses = new Dictionary<string, string>
            {
                ["200"] = """
                    [
                      {
                        "file_profile_id": "9c89c59e-8608-47b1-b280-052a75bcf84b",
                        "file_profile_version_id": "469752f8-e6f8-42c3-bff9-a79717c11d84",
                        "file_profile_version": 1,
                        "name": "Voucher transactions",
                        "operator_name": "Example Operator",
                        "listening_directory": "files/incoming/vouchers",
                        "processed_directory": "files/incoming/vouchers/processed",
                        "failed_directory": "files/incoming/vouchers/failed",
                        "file_format_handler": "CsvFileFormatHandler",
                        "request_type": "Voucher",
                        "line_terminator": "\n"
                      },
                      {
                        "file_profile_id": "9c89c59e-8608-47b1-b280-052a75bcf84b",
                        "file_profile_version_id": "7738df59-9b15-44cb-8d7a-df4845bb8bd4",
                        "file_profile_version": 2,
                        "name": "Voucher transactions",
                        "operator_name": "Example Operator",
                        "listening_directory": "files/incoming/vouchers",
                        "processed_directory": "files/incoming/vouchers/processed",
                        "failed_directory": "files/incoming/vouchers/failed",
                        "file_format_handler": "CsvFileFormatHandler",
                        "request_type": "Voucher",
                        "line_terminator": "\n"
                      }
                    ]
                    """
            }
        },
        ["GetFileProfileVersion"] = new()
        {
            Parameters = new Dictionary<string, object>
            {
                ["fileProfileId"] = "9c89c59e-8608-47b1-b280-052a75bcf84b",
                ["fileProfileVersionId"] = "7738df59-9b15-44cb-8d7a-df4845bb8bd4"
            },
            Responses = new Dictionary<string, string>
            {
                ["200"] = """
                    {
                      "file_profile_id": "9c89c59e-8608-47b1-b280-052a75bcf84b",
                      "file_profile_version_id": "7738df59-9b15-44cb-8d7a-df4845bb8bd4",
                      "file_profile_version": 2,
                      "name": "Voucher transactions",
                      "operator_name": "Example Operator",
                      "listening_directory": "files/incoming/vouchers",
                      "processed_directory": "files/incoming/vouchers/processed",
                      "failed_directory": "files/incoming/vouchers/failed",
                      "file_format_handler": "CsvFileFormatHandler",
                      "request_type": "Voucher",
                      "line_terminator": "\n"
                    }
                    """
            }
        }
    };

    /// <summary>Attaches the examples for the endpoint's OpenAPI operation.</summary>
    public static RouteHandlerBuilder WithEndpointExamples(this RouteHandlerBuilder builder)
    {
        return builder.AddOpenApiOperationTransformer(ApplyExamplesAsync);
    }

    private static Task ApplyExamplesAsync(OpenApiOperation operation,
                                          OpenApiOperationTransformerContext context,
                                          CancellationToken cancellationToken)
    {
        if (Examples.TryGetValue(operation.OperationId ?? string.Empty, out EndpointExamples? examples) == false || examples is null)
            return Task.CompletedTask;

        if (examples.RequestBody is not null && operation.RequestBody?.Content is { } requestContent)
        {
            foreach (OpenApiMediaType mediaType in requestContent.Values)
                mediaType.Example = JsonNode.Parse(examples.RequestBody);
        }

        if (examples.Responses is not null)
        {
            foreach ((string statusCode, string example) in examples.Responses)
            {
                if (operation.Responses?.TryGetValue(statusCode, out IOpenApiResponse? response) != true || response is null)
                    continue;

                if (response.Content is null)
                    continue;

                foreach (OpenApiMediaType mediaType in response.Content.Values)
                    mediaType.Example = JsonNode.Parse(example);
            }
        }

        if (examples.Parameters is not null)
        {
            AddDomainEventHeaderParameter(operation);

            foreach (OpenApiParameter parameter in (operation.Parameters ?? []).OfType<OpenApiParameter>())
            {
                if (parameter.Name is string name && examples.Parameters.TryGetValue(name, out object? value) && value is not null)
                    parameter.Example = JsonSerializer.SerializeToNode(value, value.GetType());
            }
        }

        return Task.CompletedTask;
    }

    private static void AddDomainEventHeaderParameter(OpenApiOperation operation)
    {
        if (operation.OperationId != "DomainEvent" || (operation.Parameters ?? []).Any(parameter => parameter.Name == "eventType"))
            return;

        operation.Parameters ??= [];
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "eventType",
            In = ParameterLocation.Header,
            Description = "Registered event type name used to deserialize the JSON body.",
            Required = true,
            Schema = new OpenApiSchema { Type = JsonSchemaType.String },
            Example = JsonValue.Create("FileLineAddedEvent")
        });
    }

    private sealed class EndpointExamples
    {
        public string? RequestBody { get; init; }
        public IReadOnlyDictionary<string, string>? Responses { get; init; }
        public IReadOnlyDictionary<string, object>? Parameters { get; init; }
    }
}
