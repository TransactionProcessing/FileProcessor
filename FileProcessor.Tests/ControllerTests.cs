using FileProcessor.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using Shared.EventStore.EventHandling;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FileProcessor.Tests
{
    using System.Threading;
    using File.DomainEvents;
    using Shared.General;
    using Shared.Logger;

    public class ControllerTests
    {
        public ControllerTests()
        {
            Logger.Initialise(new NullLogger());
        }
        [Fact]
        public async Task DomainEventController_EventIdNotPresentInJson_ErrorThrown()
        {
            Mock<IDomainEventHandlerResolver> resolver = new Mock<IDomainEventHandlerResolver>();
            TypeMap.AddType<FileLineAddedEvent>("FileLineAddedEvent");
            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["eventType"] = "FileLineAddedEvent";
            DomainEventController controller = new DomainEventController(resolver.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };
            String json = "{\r\n  \"estateId\": \"435613ac-a468-47a3-ac4f-649d89764c22\",\r\n  \"fileId\": \"17ee2309-ec79-dd25-0af9-f557e565feaa\",\r\n  \"fileLine\": \"\",\r\n  \"lineNumber\": 16\r\n}\t";
            Object request = JsonConvert.DeserializeObject(json);
            ArgumentException ex = Should.Throw<ArgumentException>(async () => {
                await controller.PostEventAsync(request, CancellationToken.None);
            });
            ex.Message.ShouldBe("Domain Event must contain an Event Id");
        }

        [Fact]
        public async Task DomainEventController_EventIdPresentInJson_NoErrorThrown()
        {
            Mock<IDomainEventHandlerResolver> resolver = new Mock<IDomainEventHandlerResolver>();
            TypeMap.AddType<FileLineAddedEvent>("FileLineAddedEvent");
            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["eventType"] = "FileLineAddedEvent";
            DomainEventController controller = new DomainEventController(resolver.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };
            String json = "{\r\n  \"estateId\": \"435613ac-a468-47a3-ac4f-649d89764c22\",\r\n  \"fileId\": \"17ee2309-ec79-dd25-0af9-f557e565feaa\",\r\n  \"fileLine\": \"\",\r\n  \"lineNumber\": 16,\r\n  \"eventId\": \"123456ac-a468-47a3-ac4f-649d89764b44\"\r\n}\t";
            Object request = JsonConvert.DeserializeObject(json);
            Should.NotThrow(async () => {
                await controller.PostEventAsync(request, CancellationToken.None);
            });
        }
    }
}
