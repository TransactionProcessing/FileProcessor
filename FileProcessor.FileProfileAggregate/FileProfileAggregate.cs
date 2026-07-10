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

        if (request.Name != null && Comparer.Equals(profile.Name, request.Name.Trim()) == false)
        {
            aggregate.ApplyAndAppend(new FileProfileNameUpdatedEvent(aggregate.AggregateId, fileProfileId, request.Name.Trim()));
        }

        if (request.ListeningDirectory != null && Comparer.Equals(profile.ListeningDirectory, request.ListeningDirectory.Trim()) == false)
        {
            aggregate.ApplyAndAppend(new FileProfileListeningDirectoryUpdatedEvent(aggregate.AggregateId, fileProfileId, request.ListeningDirectory.Trim()));
        }

        if (request.RequestType != null && Comparer.Equals(profile.RequestType, request.RequestType.Trim()) == false)
        {
            aggregate.ApplyAndAppend(new FileProfileRequestTypeUpdatedEvent(aggregate.AggregateId, fileProfileId, request.RequestType.Trim()));
        }

        if (request.OperatorName != null && Comparer.Equals(profile.OperatorName, request.OperatorName.Trim()) == false)
        {
            aggregate.ApplyAndAppend(new FileProfileOperatorNameUpdatedEvent(aggregate.AggregateId, fileProfileId, request.OperatorName.Trim()));
        }

        if (request.LineTerminator != null && Comparer.Equals(profile.LineTerminator, request.LineTerminator) == false)
        {
            aggregate.ApplyAndAppend(new FileProfileLineTerminatorUpdatedEvent(aggregate.AggregateId, fileProfileId, request.LineTerminator));
        }

        if (request.FileFormatHandler != null && Comparer.Equals(profile.FileFormatHandler, request.FileFormatHandler.Trim()) == false)
        {
            aggregate.ApplyAndAppend(new FileProfileFileFormatHandlerUpdatedEvent(aggregate.AggregateId, fileProfileId, request.FileFormatHandler.Trim()));
        }

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

        if (string.IsNullOrWhiteSpace(request.ListeningDirectory))
        {
            return Result.Invalid("No listening directory provided");
        }

        if (string.IsNullOrWhiteSpace(request.RequestType))
        {
            return Result.Invalid("No request type provided");
        }

        if (string.IsNullOrWhiteSpace(request.OperatorName))
        {
            return Result.Invalid("No operator name provided");
        }

        if (string.IsNullOrEmpty(request.LineTerminator))
        {
            return Result.Invalid("No line terminator provided");
        }

        if (string.IsNullOrWhiteSpace(request.FileFormatHandler))
        {
            return Result.Invalid("No file format handler provided");
        }

        if (aggregate.FileProfiles.Values.Any(profile => profile.IsArchived == false && Comparer.Equals(profile.Name, request.Name.Trim())))
        {
            return Result.Invalid($"A file profile with name [{request.Name}] already exists");
        }

        if (aggregate.FileProfiles.Values.Any(profile => profile.IsArchived == false && Comparer.Equals(profile.RequestType, request.RequestType.Trim())))
        {
            return Result.Invalid($"A file profile with request type [{request.RequestType}] already exists");
        }

        return Result.Success();
    }

    private static Result ValidateUpdateRequest(this FileProfileAggregate aggregate, Guid fileProfileId, UpdateFileProfileRequest request)
    {
        if (request == null)
        {
            return Result.Invalid("No file profile data provided");
        }

        FileProfileState currentProfile = aggregate.GetRequiredProfileState(fileProfileId);

        String proposedName = request.Name?.Trim();
        String proposedListeningDirectory = request.ListeningDirectory?.Trim();
        String proposedRequestType = request.RequestType?.Trim();
        String proposedOperatorName = request.OperatorName?.Trim();
        String proposedLineTerminator = request.LineTerminator;
        String proposedFileFormatHandler = request.FileFormatHandler?.Trim();

        if (request.Name != null && string.IsNullOrWhiteSpace(proposedName))
        {
            return Result.Invalid("No file profile name provided");
        }

        if (request.ListeningDirectory != null && string.IsNullOrWhiteSpace(proposedListeningDirectory))
        {
            return Result.Invalid("No listening directory provided");
        }

        if (request.RequestType != null && string.IsNullOrWhiteSpace(proposedRequestType))
        {
            return Result.Invalid("No request type provided");
        }

        if (request.OperatorName != null && string.IsNullOrWhiteSpace(proposedOperatorName))
        {
            return Result.Invalid("No operator name provided");
        }

        if (request.LineTerminator != null && string.IsNullOrEmpty(proposedLineTerminator))
        {
            return Result.Invalid("No line terminator provided");
        }

        if (request.FileFormatHandler != null && string.IsNullOrWhiteSpace(proposedFileFormatHandler))
        {
            return Result.Invalid("No file format handler provided");
        }

        if (request.Name != null &&
            Comparer.Equals(currentProfile.Name, proposedName) == false &&
            aggregate.FileProfiles.Values.Any(profile => profile.FileProfileId != fileProfileId && profile.IsArchived == false && Comparer.Equals(profile.Name, proposedName)))
        {
            return Result.Invalid($"A file profile with name [{request.Name}] already exists");
        }

        if (request.RequestType != null &&
            Comparer.Equals(currentProfile.RequestType, proposedRequestType) == false &&
            aggregate.FileProfiles.Values.Any(profile => profile.FileProfileId != fileProfileId && profile.IsArchived == false && Comparer.Equals(profile.RequestType, proposedRequestType)))
        {
            return Result.Invalid($"A file profile with request type [{request.RequestType}] already exists");
        }

        return Result.Success();
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
