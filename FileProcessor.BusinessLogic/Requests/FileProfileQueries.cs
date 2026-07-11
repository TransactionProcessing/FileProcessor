using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FileProcessor.Models;
using MediatR;
using SimpleResults;
using FileProfileModel = global::FileProcessor.Models.FileProfile;

namespace FileProcessor.BusinessLogic.Requests;

[ExcludeFromCodeCoverage]
public record FileProfileQueries
{
    public record GetFileProfilesQuery : IRequest<Result<List<FileProfileModel>>>;

    public record GetFileProfileQuery(Guid FileProfileId) : IRequest<Result<FileProfileModel>>;
}
