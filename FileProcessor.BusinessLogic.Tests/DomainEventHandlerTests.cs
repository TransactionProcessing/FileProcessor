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
    using Moq;
    using Shouldly;
    using Shouldly.Configuration;
    using Testing;
    using Xunit;

    public class DomainEventHandlerTests
    {
        [Fact]
        public void FileDomainEventHandler_FileLineAddedEvent_EventIsHandled()
        {
            Mock<IMediator> mediator = new Mock<IMediator>();
            FileDomainEventHandler eventHandler = new FileDomainEventHandler(mediator.Object);
            FileLineAddedEvent fileLineAddedEvent = TestData.FileLineAddedEvent;
            Should.NotThrow(async () =>
                            {
                                await eventHandler.Handle(fileLineAddedEvent, CancellationToken.None);
                            });
        }

        [Fact]
        public void FileDomainEventHandler_FileAddedToImportLogEvent_EventIsHandled()
        {
            Mock<IMediator> mediator = new Mock<IMediator>();
            FileDomainEventHandler eventHandler = new FileDomainEventHandler(mediator.Object);
            FileAddedToImportLogEvent fileAddedToImportLogEvent = TestData.FileAddedToImportLogEvent;
            Should.NotThrow(async () =>
                            {
                                await eventHandler.Handle(fileAddedToImportLogEvent, CancellationToken.None);
                            });
        }
    }
}
