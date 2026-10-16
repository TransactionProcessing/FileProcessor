using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.Client;
using FileProcessor.Models;
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
}
