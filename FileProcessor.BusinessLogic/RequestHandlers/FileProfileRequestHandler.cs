using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.BusinessLogic.Managers;
using FileProcessor.BusinessLogic.Requests;
using FileProcessor.Models;
using MediatR;
using SimpleResults;
using FileProfileModel = global::FileProcessor.Models.FileProfile;

namespace FileProcessor.BusinessLogic.RequestHandlers;

public class FileProfileRequestHandler : IRequestHandler<FileProfileCommands.CreateFileProfileCommand, Result<FileProfileModel>>,
                                         IRequestHandler<FileProfileCommands.UpdateFileProfileCommand, Result<FileProfileModel>>,
                                         IRequestHandler<FileProfileCommands.ArchiveFileProfileCommand, Result>,
                                         IRequestHandler<FileProfileQueries.GetFileProfilesQuery, Result<List<FileProfileModel>>>,
                                         IRequestHandler<FileProfileQueries.GetFileProfileQuery, Result<FileProfileModel>>
{
    private readonly IFileProfileManager FileProfileManager;

    public FileProfileRequestHandler(IFileProfileManager fileProfileManager)
    {
        this.FileProfileManager = fileProfileManager;
    }

    public Task<Result<FileProfileModel>> Handle(FileProfileCommands.CreateFileProfileCommand request, CancellationToken cancellationToken)
    {
        return this.FileProfileManager.CreateFileProfile(request.Request, cancellationToken);
    }

    public Task<Result<FileProfileModel>> Handle(FileProfileCommands.UpdateFileProfileCommand request, CancellationToken cancellationToken)
    {
        return this.FileProfileManager.UpdateFileProfile(request.FileProfileId, request.Request, cancellationToken);
    }

    public Task<Result> Handle(FileProfileCommands.ArchiveFileProfileCommand request, CancellationToken cancellationToken)
    {
        return this.FileProfileManager.ArchiveFileProfile(request.FileProfileId, cancellationToken);
    }

    public Task<Result<List<FileProfileModel>>> Handle(FileProfileQueries.GetFileProfilesQuery request, CancellationToken cancellationToken)
    {
        return this.FileProfileManager.GetAllFileProfiles(cancellationToken);
    }

    public Task<Result<FileProfileModel>> Handle(FileProfileQueries.GetFileProfileQuery request, CancellationToken cancellationToken)
    {
        return this.FileProfileManager.GetFileProfile(request.FileProfileId, cancellationToken);
    }
}
