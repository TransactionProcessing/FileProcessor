using System;
using System.Diagnostics.CodeAnalysis;
using FileProcessor.DataTransferObjects.Requests;
using FileProcessor.Models;
using MediatR;
using SimpleResults;
using FileProfileModel = global::FileProcessor.Models.FileProfile;

namespace FileProcessor.BusinessLogic.Requests;

[ExcludeFromCodeCoverage]
public record FileProfileCommands
{
    public record CreateFileProfileCommand(CreateFileProfileRequest Request) : IRequest<Result<FileProfileModel>>;

    public record UpdateFileProfileCommand(Guid FileProfileId, UpdateFileProfileRequest Request) : IRequest<Result<FileProfileModel>>;

    public record ArchiveFileProfileCommand(Guid FileProfileId) : IRequest<Result>;
}
