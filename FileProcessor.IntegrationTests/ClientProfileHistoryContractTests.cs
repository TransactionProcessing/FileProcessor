using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.Client;
using FileProcessor.Models;
using System.Net;
using System.Net.Http;
using System.Text;
using NUnit.Framework;
using SimpleResults;

namespace FileProcessor.IntegrationTests;

public class ClientProfileHistoryContractTests
{
    [Test]
    public void FileProcessorClient_ExposesHistoricalProfileQueries()
    {
        IFileProcessorClient client = null;
        Func<Task<Result<List<FileProfile>>>> getVersions = () => client.GetFileProfileVersions("token",
                                                                                               Guid.NewGuid(),
                                                                                               CancellationToken.None);
        Func<Task<Result<FileProfile>>> getVersion = () => client.GetFileProfileVersion("token",
                                                                                         Guid.NewGuid(),
                                                                                         Guid.NewGuid(),
                                                                                         CancellationToken.None);

        Assert.That((object)getVersions, Is.Not.Null);
        Assert.That((object)getVersion, Is.Not.Null);
    }

    [Test]
    public async Task FileProcessorClient_GetFileProfileVersion_PreservesNotFoundStatus()
    {
        using HttpClient httpClient = new(new NotFoundHandler());
        FileProcessorClient client = new(_ => "https://fileprocessor",
                                        httpClient,
                                        _ => String.Empty,
                                        (_, _) => null);

        Result<FileProfile> result = await client.GetFileProfileVersion("token",
                                                                         Guid.NewGuid(),
                                                                         Guid.NewGuid(),
                                                                         CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That(result.Message, Does.Contain("historical version missing"));
    }

    private sealed class NotFoundHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                                CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("historical version missing", Encoding.UTF8, "text/plain")
            });
        }
    }
}
