using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.BusinessLogic.Managers;
using FileProcessor.BusinessLogic.RequestHandlers;
using FileProcessor.BusinessLogic.Requests;
using FileProcessor.Models;
using Imposter.Abstractions;
using Shouldly;
using SimpleResults;
using Xunit;
using FileProfileModel = FileProcessor.Models.FileProfile;

namespace FileProcessor.BusinessLogic.Tests;

public class FileProfileRequestHandlerTests
{
    [Fact]
    public async Task GetFileProfileVersionsQuery_ForwardsToManager()
    {
        IFileProfileManagerImposter manager = new();
        List<FileProfileModel> expected = new();
        manager.GetFileProfileVersions(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
               .ReturnsAsync(Result.Success(expected));
        FileProfileRequestHandler handler = new(manager.Instance());
        FileProfileQueries.GetFileProfileVersionsQuery query = new(Guid.NewGuid());

        Result<List<FileProfileModel>> result = await handler.Handle(query, CancellationToken.None);

        result.Data.ShouldBeSameAs(expected);
        manager.GetFileProfileVersions(query.FileProfileId, CancellationToken.None).Called(Count.Once());
    }

    [Fact]
    public async Task GetFileProfileVersionQuery_ForwardsToManager()
    {
        IFileProfileManagerImposter manager = new();
        FileProfileModel expected = new(Guid.NewGuid(), "Profile", "/tmp/profile", "Request", "Operator", "\n", "Handler");
        manager.GetFileProfileVersion(Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
               .ReturnsAsync(Result.Success(expected));
        FileProfileRequestHandler handler = new(manager.Instance());
        FileProfileQueries.GetFileProfileVersionQuery query = new(Guid.NewGuid(), Guid.NewGuid());

        Result<FileProfileModel> result = await handler.Handle(query, CancellationToken.None);

        result.Data.ShouldBeSameAs(expected);
        manager.GetFileProfileVersion(query.FileProfileId,
                                      query.FileProfileVersionId,
                                      CancellationToken.None).Called(Count.Once());
    }
}
