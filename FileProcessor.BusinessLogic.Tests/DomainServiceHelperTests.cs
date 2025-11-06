using System;
using FileProcessor.BusinessLogic.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Identity.Client;
using Shared.EventStore.Helpers;
using Shouldly;
using SimpleResults;
using Xunit;

namespace FileProcessor.BusinessLogic.Tests;

public class DomainServiceHelperTests
{
    public DomainServiceHelperTests() {
        Shared.Logger.Logger.Initialise(NullLogger.Instance);
    }

    [Fact]
    public void DomainServiceHelper_HandleGetAggregateResult_SuccessfulGet_ResultHandled()
    {
        Guid aggregateId = Guid.Parse("0639682D-1D28-4AD8-B29D-4B76619083F1");
        Result<TestAggregate> result = Result.Success(new TestAggregate
        {
            AggregateId = aggregateId
        });

        var handleResult = DomainServiceHelper.HandleGetAggregateResult(result, aggregateId, true);
        handleResult.IsSuccess.ShouldBeTrue();
        handleResult.Data.ShouldBeOfType(typeof(TestAggregate));
        handleResult.Data.AggregateId.ShouldBe(aggregateId);
    }

    [Fact]
    public void DomainServiceHelper_HandleGetAggregateResult_FailedGet_ResultHandled()
    {
        Guid aggregateId = Guid.Parse("0639682D-1D28-4AD8-B29D-4B76619083F1");
        Result<TestAggregate> result = Result.Failure("Failed Get");

        var handleResult = DomainServiceHelper.HandleGetAggregateResult(result, aggregateId, true);
        handleResult.IsFailed.ShouldBeTrue();
        handleResult.Message.ShouldBe("Failed Get");
    }

    [Fact]
    public void DomainServiceHelper_HandleGetAggregateResult_FailedGet_NotFoundButIsError_ResultHandled()
    {
        Guid aggregateId = Guid.Parse("0639682D-1D28-4AD8-B29D-4B76619083F1");
        Result<TestAggregate> result = Result.NotFound("Failed Get");

        var handleResult = DomainServiceHelper.HandleGetAggregateResult(result, aggregateId, true);
        handleResult.IsFailed.ShouldBeTrue();
        handleResult.Message.ShouldBe("Failed Get");
    }

    [Fact]
    public void DomainServiceHelper_HandleGetAggregateResult_FailedGet_NotFoundButIsNotError_ResultHandled()
    {
        Guid aggregateId = Guid.Parse("0639682D-1D28-4AD8-B29D-4B76619083F1");
        Result<TestAggregate> result = Result.NotFound("Failed Get");

        var handleResult = DomainServiceHelper.HandleGetAggregateResult(result, aggregateId, false);
        handleResult.IsSuccess.ShouldBeTrue();
        handleResult.Data.ShouldBeOfType(typeof(TestAggregate));
        handleResult.Data.AggregateId.ShouldBe(aggregateId);
    }
}