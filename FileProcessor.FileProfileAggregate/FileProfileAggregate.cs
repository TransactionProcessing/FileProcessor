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
using Shared.Results;
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
            IsArchived = false,
            CurrentVersionId = domainEvent.FileProfileVersionId == Guid.Empty ? domainEvent.FileProfileId : domainEvent.FileProfileVersionId,
            CurrentVersion = domainEvent.FileProfileVersion,
            Versions = new List<FileProfileVersionState>
            {
                new()
                {
                    FileProfileVersionId = domainEvent.FileProfileVersionId == Guid.Empty ? domainEvent.FileProfileId : domainEvent.FileProfileVersionId,
                    FileProfileVersion = domainEvent.FileProfileVersion,
                    Name = domainEvent.Name,
                    ListeningDirectory = domainEvent.ListeningDirectory,
                    RequestType = domainEvent.RequestType,
                    OperatorName = domainEvent.OperatorName,
                    LineTerminator = domainEvent.LineTerminator,
                    FileFormatHandler = domainEvent.FileFormatHandler
                }
            }
        };
    }

    public static void PlayEvent(this FileProfileAggregate aggregate, FileProfileVersionCreatedEvent domainEvent)
    {
        FileProfileState fileProfile = aggregate.GetRequiredProfileState(domainEvent.FileProfileId);
        FileProfileVersionState version = new()
        {
            FileProfileVersionId = domainEvent.FileProfileVersionId,
            FileProfileVersion = domainEvent.FileProfileVersion,
            Name = domainEvent.Name,
            ListeningDirectory = domainEvent.ListeningDirectory,
            RequestType = domainEvent.RequestType,
            OperatorName = domainEvent.OperatorName,
            LineTerminator = domainEvent.LineTerminator,
            FileFormatHandler = domainEvent.FileFormatHandler
        };

        fileProfile.Versions.Add(version);
        fileProfile.CurrentVersionId = version.FileProfileVersionId;
        fileProfile.CurrentVersion = version.FileProfileVersion;
        fileProfile.Name = version.Name;
        fileProfile.ListeningDirectory = version.ListeningDirectory;
        fileProfile.RequestType = version.RequestType;
        fileProfile.OperatorName = version.OperatorName;
        fileProfile.LineTerminator = version.LineTerminator;
        fileProfile.FileFormatHandler = version.FileFormatHandler;
    }

    public static void PlayEvent(this FileProfileAggregate aggregate, FileProfileNameUpdatedEvent domainEvent)
    {
        FileProfileState fileProfile = aggregate.GetRequiredProfileState(domainEvent.FileProfileId);
        fileProfile.Name = domainEvent.Name;
        fileProfile.UpdateCurrentVersionSnapshot();
    }

    public static void PlayEvent(this FileProfileAggregate aggregate, FileProfileListeningDirectoryUpdatedEvent domainEvent)
    {
        FileProfileState fileProfile = aggregate.GetRequiredProfileState(domainEvent.FileProfileId);
        fileProfile.ListeningDirectory = domainEvent.ListeningDirectory;
        fileProfile.UpdateCurrentVersionSnapshot();
    }

    public static void PlayEvent(this FileProfileAggregate aggregate, FileProfileRequestTypeUpdatedEvent domainEvent)
    {
        FileProfileState fileProfile = aggregate.GetRequiredProfileState(domainEvent.FileProfileId);
        fileProfile.RequestType = domainEvent.RequestType;
        fileProfile.UpdateCurrentVersionSnapshot();
    }

    public static void PlayEvent(this FileProfileAggregate aggregate, FileProfileOperatorNameUpdatedEvent domainEvent)
    {
        FileProfileState fileProfile = aggregate.GetRequiredProfileState(domainEvent.FileProfileId);
        fileProfile.OperatorName = domainEvent.OperatorName;
        fileProfile.UpdateCurrentVersionSnapshot();
    }

    public static void PlayEvent(this FileProfileAggregate aggregate, FileProfileLineTerminatorUpdatedEvent domainEvent)
    {
        FileProfileState fileProfile = aggregate.GetRequiredProfileState(domainEvent.FileProfileId);
        fileProfile.LineTerminator = domainEvent.LineTerminator;
        fileProfile.UpdateCurrentVersionSnapshot();
    }

    public static void PlayEvent(this FileProfileAggregate aggregate, FileProfileFileFormatHandlerUpdatedEvent domainEvent)
    {
        FileProfileState fileProfile = aggregate.GetRequiredProfileState(domainEvent.FileProfileId);
        fileProfile.FileFormatHandler = domainEvent.FileFormatHandler;
        fileProfile.UpdateCurrentVersionSnapshot();
    }

    public static void PlayEvent(this FileProfileAggregate aggregate, FileProfileArchivedEvent domainEvent)
    {
        FileProfileState fileProfile = aggregate.GetRequiredProfileState(domainEvent.FileProfileId);
        fileProfile.IsArchived = true;
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

        var lineTerminatorResult = request.LineTerminator.Value.ToLineTerminator();
        if (lineTerminatorResult.IsFailed)
            return ResultHelpers.CreateFailure(lineTerminatorResult);

        FileProfileCreatedEvent fileProfileCreatedEvent = new(aggregate.AggregateId,
                                                              request.FileProfileId,
                                                              request.Name.Trim(),
                                                              request.ListeningDirectory.Trim(),
                                                              request.RequestType.Trim(),
                                                              request.OperatorName.Trim(),
                                                              lineTerminatorResult.Data,
                                                              request.FileFormatHandler.Trim(),
                                                              Guid.NewGuid(),
                                                              1);

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
        
        String lineTerminator = profile.LineTerminator;
        if (request.LineTerminator.HasValue) {
            Result<String> lineTerminatorResult = request.LineTerminator.Value.ToLineTerminator();
            if (lineTerminatorResult.IsFailed) {
                return ResultHelpers.CreateFailure(lineTerminatorResult);
            }

            lineTerminator = lineTerminatorResult.Data;
        }

        String name = request.Name == null ? profile.Name : request.Name.Trim();
        String listeningDirectory = request.ListeningDirectory == null ? profile.ListeningDirectory : request.ListeningDirectory.Trim();
        String requestType = request.RequestType == null ? profile.RequestType : request.RequestType.Trim();
        String operatorName = request.OperatorName == null ? profile.OperatorName : request.OperatorName.Trim();
        String fileFormatHandler = request.FileFormatHandler == null ? profile.FileFormatHandler : request.FileFormatHandler.Trim();

        if (IsDomainEventRequired(profile.Name, name) == false &&
            IsDomainEventRequired(profile.ListeningDirectory, listeningDirectory) == false &&
            IsDomainEventRequired(profile.RequestType, requestType) == false &&
            IsDomainEventRequired(profile.OperatorName, operatorName) == false &&
            IsDomainEventRequired(profile.LineTerminator, lineTerminator) == false &&
            IsDomainEventRequired(profile.FileFormatHandler, fileFormatHandler) == false)
        {
            return Result.Success();
        }

        Int32 nextVersion = profile.Versions.Count == 0 ? 1 : profile.Versions.Max(version => version.FileProfileVersion) + 1;
        aggregate.ApplyAndAppend(new FileProfileVersionCreatedEvent(aggregate.AggregateId,
                                                                     fileProfileId,
                                                                     Guid.NewGuid(),
                                                                     nextVersion,
                                                                     name,
                                                                     listeningDirectory,
                                                                     requestType,
                                                                     operatorName,
                                                                     lineTerminator,
                                                                     fileFormatHandler));

        return Result.Success();
    }

    private static String? TrimWhitespaceExceptLineBreaks(String value)
    {
        if (String.IsNullOrEmpty(value))
            return value;

        int start = 0;
        int end = value.Length - 1;

        while (start <= end &&
               Char.IsWhiteSpace(value[start]) &&
               value[start] != '\r' &&
               value[start] != '\n')
        {
            start++;
        }

        while (end >= start &&
               Char.IsWhiteSpace(value[end]) &&
               value[end] != '\r' &&
               value[end] != '\n')
        {
            end--;
        }

        return value.Substring(start, end - start + 1);
    }

    private static Boolean IsDomainEventRequired(String originalValue, String newValue)
    {
        String trimmedNewValue = TrimWhitespaceExceptLineBreaks(newValue);

        if (String.IsNullOrEmpty(trimmedNewValue))
            return false;

        String trimmedOriginalValue = TrimWhitespaceExceptLineBreaks(originalValue);
        
        return !String.Equals(trimmedOriginalValue, trimmedNewValue, StringComparison.InvariantCultureIgnoreCase);
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

    public static List<FileProfileModel> GetProfileVersions(this FileProfileAggregate aggregate, Guid fileProfileId)
    {
        if (aggregate.FileProfiles.TryGetValue(fileProfileId, out FileProfileState profile) == false)
            return new List<FileProfileModel>();

        return profile.Versions.OrderBy(version => version.FileProfileVersion)
                               .Select(version => version.ToModel(fileProfileId))
                               .ToList();
    }

    public static FileProfileModel GetProfileVersion(this FileProfileAggregate aggregate,
                                                      Guid fileProfileId,
                                                      Guid fileProfileVersionId)
    {
        if (aggregate.FileProfiles.TryGetValue(fileProfileId, out FileProfileState profile) == false)
            return null;

        FileProfileVersionState version = profile.Versions.SingleOrDefault(candidate => candidate.FileProfileVersionId == fileProfileVersionId);
        return version?.ToModel(fileProfileId);
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
               Comparer.Equals(profile.LineTerminator, request.LineTerminator.Value.ToLineTerminator()) &&
               Comparer.Equals(profile.FileFormatHandler, request.FileFormatHandler.Trim()) &&
               profile.IsArchived == false;
    }

    private static Result ValidateRequiredCreateFields(CreateFileProfileRequest request)
    {
        var errors = new List<string>();

        AddIfInvalid(request.ListeningDirectory, "No listening directory provided", errors);
        AddIfInvalid(request.RequestType, "No request type provided", errors);
        AddIfInvalid(request.OperatorName, "No operator name provided", errors);
        AddLineTerminatorIfInvalid(request.LineTerminator, "No line terminator provided", errors);
        AddIfInvalid(request.FileFormatHandler, "No file format handler provided", errors);

        return errors.Count == 0
            ? Result.Success()
            : Result.Invalid(errors);
    }

    private static void AddIfInvalid(string? value, string errorMessage, ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(errorMessage);
        }
    }

    private static void AddLineTerminatorIfInvalid(LineTerminatorType? value,
                                                   string errorMessage,
                                                   ICollection<string> errors)
    {
        if (value.HasValue == false || Enum.IsDefined(typeof(LineTerminatorType), value.Value) == false)
        {
            errors.Add(errorMessage);
        }
    }

    private static Result ValidateProposedUpdateValues(UpdateFileProfileRequest request)
    {
        return ValidateOptionalStringField(request.Name, "No file profile name provided") ??
               ValidateOptionalStringField(request.ListeningDirectory, "No listening directory provided") ??
               ValidateOptionalStringField(request.RequestType, "No request type provided") ??
               ValidateOptionalStringField(request.OperatorName, "No operator name provided") ??
               ValidateOptionalLineTerminatorField(request.LineTerminator, "No line terminator provided") ??
               ValidateOptionalStringField(request.FileFormatHandler, "No file format handler provided");
    }

    private static Result ValidateOptionalLineTerminatorField(LineTerminatorType? value, string errorMessage)
    {
        if (value.HasValue == false)
        {
            return Result.Success();
        }

        return Enum.IsDefined(typeof(LineTerminatorType), value.Value)
            ? Result.Success()
            : Result.Invalid(errorMessage);
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
                                    fileProfile.FileFormatHandler,
                                    fileProfile.CurrentVersionId,
                                    fileProfile.CurrentVersion);
    }

    private static FileProfileModel ToModel(this FileProfileVersionState version, Guid fileProfileId)
    {
        return new FileProfileModel(fileProfileId,
                                    version.Name,
                                    version.ListeningDirectory,
                                    version.RequestType,
                                    version.OperatorName,
                                    version.LineTerminator,
                                    version.FileFormatHandler,
                                    version.FileProfileVersionId,
                                    version.FileProfileVersion);
    }

    private static void UpdateCurrentVersionSnapshot(this FileProfileState fileProfile)
    {
        FileProfileVersionState currentVersion = fileProfile.Versions.Single(version => version.FileProfileVersionId == fileProfile.CurrentVersionId);
        currentVersion.Name = fileProfile.Name;
        currentVersion.ListeningDirectory = fileProfile.ListeningDirectory;
        currentVersion.RequestType = fileProfile.RequestType;
        currentVersion.OperatorName = fileProfile.OperatorName;
        currentVersion.LineTerminator = fileProfile.LineTerminator;
        currentVersion.FileFormatHandler = fileProfile.FileFormatHandler;
    }
}

public static class LineTerminatorTypeExtensions
{
    public static Result<string> ToLineTerminator(this LineTerminatorType lineTerminator)
    {
        return lineTerminator switch
        {
            LineTerminatorType.LineFeed => Result.Success<string>("\n"),
            LineTerminatorType.CarriageReturnLineFeed => Result.Success<string>("\r\n"),
            LineTerminatorType.CarriageReturn => Result.Success<string>("\r"),
            _ => Result.Invalid($"Invalid line terminator type {lineTerminator}")
        };
    }
}

public record FileProfileAggregate : Aggregate
{
    public static readonly Guid FileProfileCollectionId = Guid.Parse("2E8EAF39-3F51-4C10-9B8B-0D9DE3F80110");

    internal readonly Dictionary<Guid, FileProfileState> FileProfiles;

    public bool IsCreated { get; internal set; }

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

    public Guid CurrentVersionId { get; set; }

    public int CurrentVersion { get; set; }

    public List<FileProfileVersionState> Versions { get; set; } = new();
}

internal sealed class FileProfileVersionState
{
    public Guid FileProfileVersionId { get; set; }

    public int FileProfileVersion { get; set; }

    public string Name { get; set; }

    public string ListeningDirectory { get; set; }

    public string RequestType { get; set; }

    public string OperatorName { get; set; }

    public string LineTerminator { get; set; }

    public string FileFormatHandler { get; set; }
}
