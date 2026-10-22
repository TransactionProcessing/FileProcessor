using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.BusinessLogic.Requests;
using FileProcessor.Handlers;
using Imposter.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Shouldly;
using SimpleResults;
using FileProcessor.Testing;
using Shared.General;
using Shared.Logger;
using Xunit;

namespace FileProcessor.BusinessLogic.Tests;

public class FileHandlersTests
{
    [Fact]
    public async Task ReplayLine_SendsReplayCommand_AndReturnsOk()
    {
        IMediatorImposter mediator = new();
        mediator.Send(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

        IResult result = await FileHandlers.ReplayLine(
            mediator.Instance(), TestData.FileId, TestData.EstateId, TestData.LineNumber, CancellationToken.None);

        StatusCodeHttpResult response = result.ShouldBeOfType<StatusCodeHttpResult>();
        response.StatusCode.ShouldBe(StatusCodes.Status204NoContent);
        mediator.Send(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any()).Called(Count.Once());
    }

    [Fact]
    public async Task UploadFile_WhenEventStoreAcceptanceFails_DeletesTemporaryFile()
    {
        string temporaryDirectory = Path.Combine(Path.GetTempPath(), $"file-processor-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryDirectory);

        try
        {
            Dictionary<string, string> settings = new(TestData.DefaultAppSettings)
            {
                ["AppSettings:TemporaryFileLocation"] = temporaryDirectory
            };
            IConfigurationRoot configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            ConfigurationReader.Initialise(configuration);
            Logger.Initialise(NullLogger.Instance);

            IMediatorImposter mediator = new();
            mediator.Send(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

            await using MemoryStream content = new("D,1,1,1"u8.ToArray());
            FormFile formFile = new(content, 0, content.Length, "file", "source.csv")
            {
                Headers = new HeaderDictionary
                {
                    ["Content-Disposition"] = "form-data; name=\"file\"; filename=\"source.csv\""
                }
            };
            FormFileCollection files = new() { formFile };
            FormCollection formCollection = new(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), files);

            DefaultHttpContext httpContext = new();
            httpContext.Request.Form = formCollection;
            await FileHandlers.UploadFile(
                mediator.Instance(),
                CreateUploadRequest(formFile),
                httpContext.Request,
                CancellationToken.None);

            Directory.GetFiles(temporaryDirectory).ShouldBeEmpty();
        }
        finally
        {
            if (Directory.Exists(temporaryDirectory))
                Directory.Delete(temporaryDirectory, true);
        }
    }

    [Fact]
    public async Task UploadFile_UsesUniqueTemporaryPathForRepeatedClientFilename()
    {
        string temporaryDirectory = Path.Combine(Path.GetTempPath(), $"file-processor-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryDirectory);

        try
        {
            Dictionary<string, string> settings = new(TestData.DefaultAppSettings)
            {
                ["AppSettings:TemporaryFileLocation"] = temporaryDirectory
            };
            IConfigurationRoot configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            ConfigurationReader.Initialise(configuration);
            Logger.Initialise(NullLogger.Instance);

            IMediatorImposter mediator = new();
            mediator.Send(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

            FormCollection firstForm = CreateFormCollection();
            DefaultHttpContext firstContext = new();
            firstContext.Request.Form = firstForm;
            await FileHandlers.UploadFile(mediator.Instance(), CreateUploadRequest(firstForm.Files[0]), firstContext.Request, CancellationToken.None);

            FormCollection secondForm = CreateFormCollection();
            DefaultHttpContext secondContext = new();
            secondContext.Request.Form = secondForm;
            await FileHandlers.UploadFile(mediator.Instance(), CreateUploadRequest(secondForm.Files[0]), secondContext.Request, CancellationToken.None);

            Directory.GetFiles(temporaryDirectory).Length.ShouldBe(2);
        }
        finally
        {
            if (Directory.Exists(temporaryDirectory))
                Directory.Delete(temporaryDirectory, true);
        }
    }

    [Fact]
    public async Task UploadFile_DoesNotUseClientFilenameCharactersInTemporaryPath()
    {
        string temporaryDirectory = Path.Combine(Path.GetTempPath(), $"file-processor-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryDirectory);

        try
        {
            Dictionary<string, string> settings = new(TestData.DefaultAppSettings)
            {
                ["AppSettings:TemporaryFileLocation"] = temporaryDirectory
            };
            IConfigurationRoot configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            ConfigurationReader.Initialise(configuration);
            Logger.Initialise(NullLogger.Instance);

            IMediatorImposter mediator = new();
            mediator.Send(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

            FormCollection formCollection = CreateFormCollection("invoice:payload.csv");
            DefaultHttpContext httpContext = new();
            httpContext.Request.Form = formCollection;
            await FileHandlers.UploadFile(mediator.Instance(), CreateUploadRequest(formCollection.Files[0]), httpContext.Request, CancellationToken.None);

            string storedFileName = Path.GetFileName(Directory.GetFiles(temporaryDirectory).Single());
            storedFileName.ShouldNotContain(":");
            Path.GetExtension(storedFileName).ShouldBeEmpty();
        }
        finally
        {
            if (Directory.Exists(temporaryDirectory))
                Directory.Delete(temporaryDirectory, true);
        }
    }

    private static UploadFileForm CreateUploadRequest(IFormFile file) => new()
    {
        FileId = TestData.FileId,
        EstateId = TestData.EstateId,
        MerchantId = TestData.MerchantId,
        UserId = TestData.UserId,
        FileProfileId = TestData.FileProfileId,
        File = file
    };

    private static FormCollection CreateFormCollection(string fileName = "source.csv")
    {
        MemoryStream content = new("D,1,1,1"u8.ToArray());
        FormFile formFile = new(content, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary
            {
                ["Content-Disposition"] = $"form-data; name=\"file\"; filename=\"{fileName}\""
            }
        };
        return new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), new FormFileCollection { formFile });
    }
}
