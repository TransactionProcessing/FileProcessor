using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FileProcessor.DataTransferObjects.Requests;
using FileProcessor.Models;
using FileProcessor.FileProfile.DomainEvents;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.General;
using SimpleResults;
using FileProfileModel = FileProcessor.Models.FileProfile;

namespace FileProcessor.FileProfileAggregate;

public static class FileProfileAggregateExtensions
{
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    public static void PlayEvent(this FileProfileAggregate aggregate, FileProfileCreatedEvent domainEvent)
    {
        aggregate.IsCreated = true;
        aggregate.FileProfiles[domainEvent.FileProfileId] = new FileProfileState
        {
            FileProfileId = domainEvent.FileProfileId,
            Name = domainEvent.Name,
            ListeningDirectory = domainEvent.ListeningDirectory,
            RequestType = domainEvent.RequestType,
            OperatorName = domainEvent.OperatorName,
            LineTerminator = domainEvent.LineTerminator,
            FileFormatHandler = domainEvent.FileFormatHandler,
            IsArchived = false
        };
        aggregate.AppliedEventCount++;
    }

    public static void PlayEvent(this FileProfileAggregate aggregate, FileProfileNameUpdatedEvent domainEvent)
    {
        FileProfileState fileProfile = aggregate.GetRequiredProfileState(domainEvent.FileProfileId);
        fileProfile.Name = domainEvent.Name;
        aggregate.AppliedEventCount++;
    }

    public static void PlayEvent(this FileProfileAggregate aggregate, FileProfileListeningDirectoryUpdatedEvent domainEvent)
    {
        FileProfileState fileProfile = aggregate.GetRequiredProfileState(domainEvent.FileProfileId);
        fileProfile.ListeningDirectory = domainEvent.ListeningDirectory;
        aggregate.AppliedEventCount++;
    }

    public static void PlayEvent(this FileProfileAggregate aggregate, FileProfileRequestTypeUpdatedEvent domainEvent)
    {
        FileProfileState fileProfile = aggregate.GetRequiredProfileState(domainEvent.FileProfileId);
        fileProfile.RequestType = domainEvent.RequestType;
        aggregate.AppliedEventCount++;
    }

    public static void PlayEvent(this FileProfileAggregate aggregate, FileProfileOperatorNameUpdatedEvent domainEvent)
    {
        FileProfileState fileProfile = aggregate.GetRequiredProfileState(domainEvent.FileProfileId);
        fileProfile.OperatorName = domainEvent.OperatorName;
        aggregate.AppliedEventCount++;
    }

    public static void PlayEvent(this FileProfileAggregate aggregate, FileProfileLineTerminatorUpdatedEvent domainEvent)
    {
        FileProfileState fileProfile = aggregate.GetRequiredProfileState(domainEvent.FileProfileId);
        fileProfile.LineTerminator = domainEvent.LineTerminator;
        aggregate.AppliedEventCount++;
    }

    public static void PlayEvent(this FileProfileAggregate aggregate, FileProfileFileFormatHandlerUpdatedEvent domainEvent)
    {
        FileProfileState fileProfile = aggregate.GetRequiredProfileState(domainEvent.FileProfileId);
        fileProfile.FileFormatHandler = domainEvent.FileFormatHandler;
        aggregate.AppliedEventCount++;
    }

    public static void PlayEvent(this FileProfileAggregate aggregate, FileProfileArchivedEvent domainEvent)
    {
        FileProfileState fileProfile = aggregate.GetRequiredProfileState(domainEvent.FileProfileId);
        fileProfile.IsArchived = true;
        aggregate.AppliedEventCount++;
    }

    public static Result CreateProfile(this FileProfileAggregate aggregate, CreateFileProfileRequest request)
    {
        Result validationResult = aggregate.ValidateCreateRequest(request);
        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        if (aggregate.FileProfiles.TryGetValue(request.FileProfileId, out FileProfileState existingProfile))
        {
            if (aggregate.IsSameProfile(existingProfile, request))
            {
                return Result.Success();
            }

            return Result.Invalid($"File profile [{request.FileProfileId}] already exists");
        }

        FileProfileCreatedEvent fileProfileCreatedEvent = new(aggregate.AggregateId,
                                                              request.FileProfileId,
                                                              request.Name.Trim(),
                                                              request.ListeningDirectory.Trim(),
                                                              request.RequestType.Trim(),
                                                              request.OperatorName.Trim(),
                                                              request.LineTerminator,
                                                              request.FileFormatHandler.Trim());

        aggregate.ApplyAndAppend(fileProfileCreatedEvent);
        return Result.Success();
    }

    public static Result UpdateProfile(this FileProfileAggregate aggregate, Guid fileProfileId, UpdateFileProfileRequest request)
    {
        if (aggregate.FileProfiles.TryGetValue(fileProfileId, out FileProfileState profile) == false || profile.IsArchived)
        {
            return Result.NotFound($"File profile with Id [{fileProfileId}] not found");
        }

        Result validationResult = aggregate.ValidateUpdateRequest(fileProfileId, request);
        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        aggregate.ApplyStringUpdateIfChanged(profile.Name,
                                              request.Name,
                                              value => new FileProfileNameUpdatedEvent(aggregate.AggregateId, fileProfileId, value));
        aggregate.ApplyStringUpdateIfChanged(profile.ListeningDirectory,
                                             request.ListeningDirectory,
                                             value => new FileProfileListeningDirectoryUpdatedEvent(aggregate.AggregateId, fileProfileId, value));
        aggregate.ApplyStringUpdateIfChanged(profile.RequestType,
                                             request.RequestType,
                                             value => new FileProfileRequestTypeUpdatedEvent(aggregate.AggregateId, fileProfileId, value));
        aggregate.ApplyStringUpdateIfChanged(profile.OperatorName,
                                             request.OperatorName,
                                             value => new FileProfileOperatorNameUpdatedEvent(aggregate.AggregateId, fileProfileId, value));
        aggregate.ApplyStringUpdateIfChanged(profile.LineTerminator,
                                             request.LineTerminator,
                                             value => new FileProfileLineTerminatorUpdatedEvent(aggregate.AggregateId, fileProfileId, value));
        aggregate.ApplyStringUpdateIfChanged(profile.FileFormatHandler,
                                             request.FileFormatHandler,
                                             value => new FileProfileFileFormatHandlerUpdatedEvent(aggregate.AggregateId, fileProfileId, value));

        return Result.Success();
    }

    public static Result ArchiveProfile(this FileProfileAggregate aggregate, Guid fileProfileId)
    {
        if (aggregate.FileProfiles.TryGetValue(fileProfileId, out FileProfileState profile) == false)
        {
            return Result.NotFound($"File profile with Id [{fileProfileId}] not found");
        }

        if (profile.IsArchived)
        {
            return Result.Success();
        }

        aggregate.ApplyAndAppend(new FileProfileArchivedEvent(aggregate.AggregateId, fileProfileId));
        return Result.Success();
    }

    public static List<FileProfileModel> GetAllProfiles(this FileProfileAggregate aggregate)
    {
        return aggregate.FileProfiles.Values
                                    .Where(fileProfile => fileProfile.IsArchived == false)
                                    .Select(fileProfile => fileProfile.ToModel())
                                    .OrderBy(fileProfile => fileProfile.Name)
                                    .ToList();
    }

    public static FileProfileModel GetProfile(this FileProfileAggregate aggregate, Guid fileProfileId)
    {
        if (aggregate.FileProfiles.TryGetValue(fileProfileId, out FileProfileState profile) == false || profile.IsArchived)
        {
            return null;
        }

        return profile.ToModel();
    }

    private static Result ValidateCreateRequest(this FileProfileAggregate aggregate, CreateFileProfileRequest request)
    {
        if (request == null)
        {
            return Result.Invalid("No file profile data provided");
        }

        if (request.FileProfileId == Guid.Empty)
        {
            return Result.Invalid("No file profile Id provided");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Invalid("No file profile name provided");
        }

        Result requiredFieldsResult = ValidateRequiredCreateFields(request);
        if (requiredFieldsResult.IsFailed)
        {
            return requiredFieldsResult;
        }

        return aggregate.ValidateCreateUniqueness(request);
    }

    private static Result ValidateUpdateRequest(this FileProfileAggregate aggregate, Guid fileProfileId, UpdateFileProfileRequest request)
    {
        if (request == null)
        {
            return Result.Invalid("No file profile data provided");
        }

        FileProfileState currentProfile = aggregate.GetRequiredProfileState(fileProfileId);

        Result proposedValuesResult = ValidateProposedUpdateValues(request);
        if (proposedValuesResult.IsFailed)
        {
            return proposedValuesResult;
        }

        return aggregate.ValidateUpdateUniqueness(fileProfileId, currentProfile, request);
    }

    private static Boolean IsSameProfile(this FileProfileAggregate aggregate, FileProfileState profile, CreateFileProfileRequest request)
    {
        return Comparer.Equals(profile.Name, request.Name.Trim()) &&
               Comparer.Equals(profile.ListeningDirectory, request.ListeningDirectory.Trim()) &&
               Comparer.Equals(profile.RequestType, request.RequestType.Trim()) &&
               Comparer.Equals(profile.OperatorName, request.OperatorName.Trim()) &&
               Comparer.Equals(profile.LineTerminator, request.LineTerminator) &&
               Comparer.Equals(profile.FileFormatHandler, request.FileFormatHandler.Trim()) &&
               profile.IsArchived == false;
    }

    private static Result ValidateRequiredCreateFields(CreateFileProfileRequest request)
    {
        return ValidateStringField(request.ListeningDirectory, "No listening directory provided") ??
               ValidateStringField(request.RequestType, "No request type provided") ??
               ValidateStringField(request.OperatorName, "No operator name provided") ??
               ValidateStringField(request.LineTerminator, "No line terminator provided") ??
               ValidateStringField(request.FileFormatHandler, "No file format handler provided");
    }

    private static Result ValidateProposedUpdateValues(UpdateFileProfileRequest request)
    {
        return ValidateOptionalStringField(request.Name, "No file profile name provided") ??
               ValidateOptionalStringField(request.ListeningDirectory, "No listening directory provided") ??
               ValidateOptionalStringField(request.RequestType, "No request type provided") ??
               ValidateOptionalStringField(request.OperatorName, "No operator name provided") ??
               ValidateOptionalStringField(request.LineTerminator, "No line terminator provided") ??
               ValidateOptionalStringField(request.FileFormatHandler, "No file format handler provided");
    }

    private static Result ValidateCreateUniqueness(this FileProfileAggregate aggregate, CreateFileProfileRequest request)
    {
        String proposedName = request.Name.Trim();
        String proposedRequestType = request.RequestType.Trim();

        if (aggregate.FileProfiles.Values.Any(profile => profile.IsArchived == false && Comparer.Equals(profile.Name, proposedName)))
        {
            return Result.Invalid($"A file profile with name [{request.Name}] already exists");
        }

        if (aggregate.FileProfiles.Values.Any(profile => profile.IsArchived == false && Comparer.Equals(profile.RequestType, proposedRequestType)))
        {
            return Result.Invalid($"A file profile with request type [{request.RequestType}] already exists");
        }

        return Result.Success();
    }

    private static Result ValidateUpdateUniqueness(this FileProfileAggregate aggregate, Guid fileProfileId, FileProfileState currentProfile, UpdateFileProfileRequest request)
    {
        String proposedName = request.Name?.Trim();
        String proposedRequestType = request.RequestType?.Trim();

        Result nameValidationResult = aggregate.ValidateUpdatedNameIsUnique(fileProfileId, currentProfile, request.Name, proposedName);
        if (nameValidationResult.IsFailed)
        {
            return nameValidationResult;
        }

        return aggregate.ValidateUpdatedRequestTypeIsUnique(fileProfileId, currentProfile, request.RequestType, proposedRequestType);
    }

    private static Result ValidateUpdatedNameIsUnique(this FileProfileAggregate aggregate,
                                                      Guid fileProfileId,
                                                      FileProfileState currentProfile,
                                                      String rawName,
                                                      String proposedName)
    {
        if (rawName != null &&
            Comparer.Equals(currentProfile.Name, proposedName) == false &&
            aggregate.FileProfiles.Values.Any(profile => profile.FileProfileId != fileProfileId && profile.IsArchived == false && Comparer.Equals(profile.Name, proposedName)))
        {
            return Result.Invalid($"A file profile with name [{rawName}] already exists");
        }

        return Result.Success();
    }

    private static Result ValidateUpdatedRequestTypeIsUnique(this FileProfileAggregate aggregate,
                                                             Guid fileProfileId,
                                                             FileProfileState currentProfile,
                                                             String rawRequestType,
                                                             String proposedRequestType)
    {
        if (rawRequestType != null &&
            Comparer.Equals(currentProfile.RequestType, proposedRequestType) == false &&
            aggregate.FileProfiles.Values.Any(profile => profile.FileProfileId != fileProfileId && profile.IsArchived == false && Comparer.Equals(profile.RequestType, proposedRequestType)))
        {
            return Result.Invalid($"A file profile with request type [{rawRequestType}] already exists");
        }

        return Result.Success();
    }

    private static Result ValidateStringField(String value, String errorMessage)
    {
        return string.IsNullOrWhiteSpace(value) ? Result.Invalid(errorMessage) : Result.Success();
    }

    private static Result ValidateOptionalStringField(String value, String errorMessage)
    {
        if (value == null)
        {
            return Result.Success();
        }

        return ValidateStringField(value, errorMessage);
    }

    private static void ApplyStringUpdateIfChanged(this FileProfileAggregate aggregate,
                                                   String currentValue,
                                                   String proposedValue,
                                                   Func<String, IDomainEvent> eventFactory)
    {
        if (proposedValue == null)
        {
            return;
        }

        String trimmedValue = proposedValue.Trim();
        if (Comparer.Equals(currentValue, trimmedValue))
        {
            return;
        }

        aggregate.ApplyAndAppend(eventFactory(trimmedValue));
    }

    private static FileProfileState GetRequiredProfileState(this FileProfileAggregate aggregate, Guid fileProfileId)
    {
        if (aggregate.FileProfiles.TryGetValue(fileProfileId, out FileProfileState fileProfile) == false || fileProfile.IsArchived)
        {
            throw new InvalidOperationException($"File profile with Id [{fileProfileId}] not found");
        }

        return fileProfile;
    }

    private static FileProfileModel ToModel(this FileProfileState fileProfile)
    {
        return new FileProfileModel(fileProfile.FileProfileId,
                                    fileProfile.Name,
                                    fileProfile.ListeningDirectory,
                                    fileProfile.RequestType,
                                    fileProfile.OperatorName,
                                    fileProfile.LineTerminator,
                                    fileProfile.FileFormatHandler);
    }
}

public record FileProfileAggregate : Aggregate
{
    public static readonly Guid FileProfileCollectionId = Guid.Parse("2E8EAF39-3F51-4C10-9B8B-0D9DE3F80110");

    internal readonly Dictionary<Guid, FileProfileState> FileProfiles;

    public bool IsCreated { get; internal set; }

    public int AppliedEventCount { get; internal set; }

    [ExcludeFromCodeCoverage]
    public FileProfileAggregate()
    {
        this.FileProfiles = new Dictionary<Guid, FileProfileState>();
    }

    private FileProfileAggregate(Guid aggregateId)
    {
        this.AggregateId = aggregateId;
        this.FileProfiles = new Dictionary<Guid, FileProfileState>();
    }

    public static FileProfileAggregate Create()
    {
        return new FileProfileAggregate(FileProfileCollectionId);
    }

    protected override object GetMetadata()
    {
        return null;
    }

    public override void PlayEvent(IDomainEvent domainEvent) => FileProfileAggregateExtensions.PlayEvent(this, (dynamic)domainEvent);

    public int ActiveProfileCount => this.FileProfiles.Values.Count(profile => profile.IsArchived == false);
}

internal sealed class FileProfileState
{
    public Guid FileProfileId { get; set; }

    public string Name { get; set; }

    public string ListeningDirectory { get; set; }

    public string RequestType { get; set; }

    public string OperatorName { get; set; }

    public string LineTerminator { get; set; }

    public string FileFormatHandler { get; set; }

    public bool IsArchived { get; set; }
}
