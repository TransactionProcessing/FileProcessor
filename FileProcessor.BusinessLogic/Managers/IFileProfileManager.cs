using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.DataTransferObjects.Requests;
using SimpleResults;
using FileProfileModel = global::FileProcessor.Models.FileProfile;

namespace FileProcessor.BusinessLogic.Managers;

public interface IFileProfileManager
{
    Task<Result> EnsureSeededFileProfiles(CancellationToken cancellationToken);

    Task<Result<List<FileProfileModel>>> GetAllFileProfiles(CancellationToken cancellationToken);

    Task<Result<FileProfileModel>> GetFileProfile(Guid fileProfileId, CancellationToken cancellationToken);

    Task<Result<FileProfileModel>> CreateFileProfile(CreateFileProfileRequest request, CancellationToken cancellationToken);

    Task<Result<FileProfileModel>> UpdateFileProfile(Guid fileProfileId, UpdateFileProfileRequest request, CancellationToken cancellationToken);

    Task<Result> ArchiveFileProfile(Guid fileProfileId, CancellationToken cancellationToken);
}
