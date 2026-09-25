using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.File.DomainEvents;
using FileProcessor.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.EventStore.EventHandling;
using Shared.Exceptions;
using Shared.General;
using Shared.Logger;
using Shared.Serialisation;
using SimpleResults;
using Shouldly;
using FileProcessor.Testing;
using Xunit;

namespace FileProcessor.Tests;

public class DomainEventHandlersTests
{
    private static readonly string EventType = nameof(FileLineAddedEvent);

    public DomainEventHandlersTests()
    {
        StringSerialiser.Initialise(new SystemTextJsonSerializer(SystemTextJsonSerializer.GetDefaultJsonSerializerOptions()));
        Logger.Initialise(new NullLogger());
    }

    [Fact]
    public async Task HandleDomainEvent_WhenMappedTypeIsNotADomainEvent_ThrowsNullReferenceException()
    {
        DefaultHttpContext context = CreateRequest("non-domain-event");
        TypeMap.AddType(typeof(string), "non-domain-event");

        await Should.ThrowAsync<NullReferenceException>(() => DomainEventHandlers.HandleDomainEvent(
            context.Request,
            "not a domain event",
            new Resolver(FailedHandlersResult()),
            CancellationToken.None));
    }

    [Fact]
    public async Task HandleDomainEvent_WhenEventTypeIsNotRegistered_ThrowsLookupException()
    {
        DefaultHttpContext context = CreateRequest("missing-event-type");

        await Should.ThrowAsync<KeyNotFoundException>(() => DomainEventHandlers.HandleDomainEvent(
            context.Request,
            TestData.FileLineAddedEvent,
            new Resolver(Result.Success(new List<IDomainEventHandler>())),
            CancellationToken.None));
    }

    [Fact]
    public async Task HandleDomainEvent_WhenNoHandlersAreConfigured_ReturnsOk()
    {
        DefaultHttpContext context = CreateRequest(EventType);

        IResult result = await DomainEventHandlers.HandleDomainEvent(
            context.Request,
            TestData.FileLineAddedEvent,
            new Resolver(FailedHandlersResult()),
            CancellationToken.None);

        result.ShouldBeOfType<Ok>();
    }

    [Fact]
    public async Task HandleDomainEvent_WhenHandlerSucceeds_ReturnsOk()
    {
        DefaultHttpContext context = CreateRequest(EventType);

        IResult result = await DomainEventHandlers.HandleDomainEvent(
            context.Request,
            TestData.FileLineAddedEvent,
            new Resolver(Result.Success(new List<IDomainEventHandler> { new Handler(Result.Success()) })),
            CancellationToken.None);

        result.ShouldBeOfType<Ok>();
    }

    [Fact]
    public async Task HandleDomainEvent_WhenHandlerReturnsFailure_ReturnsProblemWithFailureDetails()
    {
        DefaultHttpContext context = CreateRequest(EventType);

        IResult result = await DomainEventHandlers.HandleDomainEvent(
            context.Request,
            TestData.FileLineAddedEvent,
            new Resolver(Result.Success(new List<IDomainEventHandler>
            {
                new Handler(Result.Failure("Processing failed"))
            })),
            CancellationToken.None);

        ProblemHttpResult problem = result.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(500);
        problem.ProblemDetails.Title.ShouldBe("One or more event handlers failed");
        problem.ProblemDetails.Extensions["eventId"].ShouldBeOfType<Guid>();
        problem.ProblemDetails.Extensions["eventType"].ShouldBe(EventType);
    }

    [Fact]
    public async Task HandleDomainEvent_WhenHandlerThrowsSynchronously_ReturnsProblem()
    {
        DefaultHttpContext context = CreateRequest(EventType);

        IResult result = await DomainEventHandlers.HandleDomainEvent(
            context.Request,
            TestData.FileLineAddedEvent,
            new Resolver(Result.Success(new List<IDomainEventHandler>
            {
                new Handler(exception: new InvalidOperationException("Handler failed"))
            })),
            CancellationToken.None);

        result.ShouldBeOfType<ProblemHttpResult>();
    }

    [Fact]
    public async Task HandleDomainEvent_WhenHandlerFaultsAsynchronously_ReturnsProblem()
    {
        DefaultHttpContext context = CreateRequest(EventType);

        IResult result = await DomainEventHandlers.HandleDomainEvent(
            context.Request,
            TestData.FileLineAddedEvent,
            new Resolver(Result.Success(new List<IDomainEventHandler>
            {
                new Handler(task: Task.FromException<Result>(new InvalidOperationException("Handler failed")))
            })),
            CancellationToken.None);

        result.ShouldBeOfType<ProblemHttpResult>();
    }

    [Fact]
    public async Task HandleDomainEvent_WhenHandlerIsCancelled_ReturnsProblem()
    {
        DefaultHttpContext context = CreateRequest(EventType);

        IResult result = await DomainEventHandlers.HandleDomainEvent(
            context.Request,
            TestData.FileLineAddedEvent,
            new Resolver(Result.Success(new List<IDomainEventHandler>
            {
                new Handler(task: Task.FromCanceled<Result>(new CancellationToken(true)))
            })),
            CancellationToken.None);

        result.ShouldBeOfType<ProblemHttpResult>();
    }

    [Fact]
    public async Task HandleDomainEvent_WhenCancellationIsAlreadyRequested_ThrowsOperationCanceledException()
    {
        DefaultHttpContext context = CreateRequest(EventType);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(() => DomainEventHandlers.HandleDomainEvent(
            context.Request,
            TestData.FileLineAddedEvent,
            new Resolver(Result.Success(new List<IDomainEventHandler>())),
            cancellation.Token));
    }

    private static DefaultHttpContext CreateRequest(string eventType)
    {
        DefaultHttpContext context = new();
        context.Request.Headers["eventType"] = eventType;
        TypeMap.AddType(typeof(FileLineAddedEvent), EventType);
        return context;
    }

    private static Result<List<IDomainEventHandler>> FailedHandlersResult()
    {
        return new Result<List<IDomainEventHandler>>
        {
            Status = ResultStatus.Failure,
            Message = "No handlers"
        };
    }

    private sealed class Resolver(Result<List<IDomainEventHandler>> result) : IDomainEventHandlerResolver
    {
        public Result<List<IDomainEventHandler>> GetDomainEventHandlers(Shared.DomainDrivenDesign.EventSourcing.IDomainEvent domainEvent) => result;
    }

    private sealed class Handler : IDomainEventHandler
    {
        private readonly Task<Result> task;
        private readonly Exception exception;

        public Handler(Result result = null, Task<Result> task = null, Exception exception = null)
        {
            this.task = task ?? Task.FromResult(result ?? Result.Success());
            this.exception = exception;
        }

        public Task<Result> Handle(Shared.DomainDrivenDesign.EventSourcing.IDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            if (this.exception != null)
                throw this.exception;

            return this.task;
        }
    }
}
