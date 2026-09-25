using System;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.BusinessLogic.Requests;
using FileProcessor.Handlers;
using Imposter.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Shouldly;
using SimpleResults;
using FileProcessor.Testing;
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
}
