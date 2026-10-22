using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.Common;
using FileProcessor.BusinessLogic.Requests;
using FileProcessor.DataTransferObjects.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Results.Web;
using SimpleResults;
using FileProfileModel = global::FileProcessor.Models.FileProfile;

namespace FileProcessor.Handlers;

public static class FileProfileHandlers
{
    public static async Task<IResult> GetFileProfiles(IMediator mediator, CancellationToken cancellationToken)
    {
        FileProfileQueries.GetFileProfilesQuery query = new();
        Result<List<FileProfileModel>> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    public static async Task<IResult> GetFileProfile(IMediator mediator,
                                                     [FromRoute, Description("GUID identifying the file profile.")] Guid fileProfileId,
                                                     CancellationToken cancellationToken)
    {
        FileProfileQueries.GetFileProfileQuery query = new(fileProfileId);
        Result<FileProfileModel> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    public static async Task<IResult> GetFileProfileVersions(IMediator mediator,
                                                              [FromRoute, Description("GUID identifying the file profile.")] Guid fileProfileId,
                                                              CancellationToken cancellationToken)
    {
        FileProfileQueries.GetFileProfileVersionsQuery query = new(fileProfileId);
        Result<List<FileProfileModel>> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    public static async Task<IResult> GetFileProfileVersion(IMediator mediator,
                                                            [FromRoute, Description("GUID identifying the file profile.")] Guid fileProfileId,
                                                            [FromRoute, Description("GUID identifying the requested file profile version.")] Guid fileProfileVersionId,
                                                            CancellationToken cancellationToken)
    {
        FileProfileQueries.GetFileProfileVersionQuery query = new(fileProfileId, fileProfileVersionId);
        Result<FileProfileModel> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    public static async Task<IResult> CreateFileProfile(IMediator mediator,
                                                        [FromBody] CreateFileProfileRequest request,
                                                        CancellationToken cancellationToken)
    {
        FileProfileCommands.CreateFileProfileCommand command = new(request);
        Result<FileProfileModel> result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    public static async Task<IResult> UpdateFileProfile(IMediator mediator,
                                                        [FromRoute, Description("GUID identifying the file profile.")] Guid fileProfileId,
                                                        [FromBody] UpdateFileProfileRequest request,
                                                        CancellationToken cancellationToken)
    {
        FileProfileCommands.UpdateFileProfileCommand command = new(fileProfileId, request);
        Result<FileProfileModel> result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    public static async Task<IResult> ArchiveFileProfile(IMediator mediator,
                                                         [FromRoute, Description("GUID identifying the file profile.")] Guid fileProfileId,
                                                         CancellationToken cancellationToken)
    {
        FileProfileCommands.ArchiveFileProfileCommand command = new(fileProfileId);
        Result result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            return ResponseFactory.FromResult(result);
        }

        return Results.NoContent();
    }
}
