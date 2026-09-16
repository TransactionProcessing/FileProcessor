using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.BusinessLogic.Tests
{
    using System.Threading;
    using EventHandling;
    using File.DomainEvents;
    using FileImportLog.DomainEvents;
    using MediatR;
    using Imposter.Abstractions;
    using Microsoft.Extensions.Logging;
    using Requests;
    using SimpleResults;
    using Shouldly;
    using Shouldly.Configuration;
    using Testing;
    using Xunit;

    public class DomainEventHandlerTests
    {
        [Fact]
        public async Task FileDomainEventHandler_FileLineAddedEvent_MediatorFailureIsLoggedWithFileAndLine()
        {
            IMediatorImposter mediator = new IMediatorImposter();
            CapturingLogger<FileDomainEventHandler> logger = new();
            mediator.Send(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure("Processing failed"));
            FileDomainEventHandler eventHandler = new(mediator.Instance(), logger);

            Result result = await eventHandler.Handle(TestData.FileLineAddedEvent, CancellationToken.None);

            result.IsFailed.ShouldBeTrue();
            logger.Messages.ShouldContain(message => message.Contains("event-handler-dispatch-failed") &&
                                                     message.Contains(TestData.FileLineAddedEvent.FileId.ToString()) &&
                                                     message.Contains(TestData.FileLineAddedEvent.LineNumber.ToString()));
        }

        [Fact]
        public async Task FileDomainEventHandler_FileLineAddedEvent_MediatorExceptionIsLoggedAndPropagated()
        {
            IMediatorImposter mediator = new IMediatorImposter();
            CapturingLogger<FileDomainEventHandler> logger = new();
            mediator.Send(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any())
                .ThrowsAsync(new InvalidOperationException("Mediator failed"));
            FileDomainEventHandler eventHandler = new(mediator.Instance(), logger);

            InvalidOperationException exception = await Should.ThrowAsync<InvalidOperationException>(
                () => eventHandler.Handle(TestData.FileLineAddedEvent, CancellationToken.None));

            exception.Message.ShouldBe("Mediator failed");
            logger.Messages.ShouldContain(message => message.Contains("event-handler-dispatch-threw") &&
                                                     message.Contains(TestData.FileLineAddedEvent.FileId.ToString()) &&
                                                     message.Contains(TestData.FileLineAddedEvent.LineNumber.ToString()));
        }

        [Fact]
        public void FileDomainEventHandler_FileLineAddedEvent_EventIsHandled()
        {
            IMediatorImposter mediator = new IMediatorImposter();
            mediator.Send(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
            FileDomainEventHandler eventHandler = new FileDomainEventHandler(mediator.Instance());
            FileLineAddedEvent fileLineAddedEvent = TestData.FileLineAddedEvent;
            Should.NotThrow(async () =>
                            {
                                await eventHandler.Handle(fileLineAddedEvent, CancellationToken.None);
                            });
        }

        [Fact]
        public void FileDomainEventHandler_FileAddedToImportLogEvent_EventIsHandled()
        {
            IMediatorImposter mediator = new IMediatorImposter();
            mediator.Send(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
            FileDomainEventHandler eventHandler = new FileDomainEventHandler(mediator.Instance());
            FileAddedToImportLogEvent fileAddedToImportLogEvent = TestData.FileAddedToImportLogEvent;
            Should.NotThrow(async () =>
                            {
                                await eventHandler.Handle(fileAddedToImportLogEvent, CancellationToken.None);
                            });
        }
    }
}
