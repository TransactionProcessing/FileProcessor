using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileProcessor.Controllers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.EventStore.EventHandling;
    using Shared.General;
    using Shared.Logger;
    using Shared.Serialisation;

    [Route(DomainEventController.ControllerRoute)]
    [ApiController]
    [ExcludeFromCodeCoverage]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DomainEventController : ControllerBase
    {
        #region Fields

        /// <summary>
        /// The domain event handler resolver
        /// </summary>
        private readonly IDomainEventHandlerResolver DomainEventHandlerResolver;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEventController"/> class.
        /// </summary>
        /// <param name="domainEventHandlerResolver">The domain event handler resolver.</param>
        public DomainEventController(IDomainEventHandlerResolver domainEventHandlerResolver)
        {
            this.DomainEventHandlerResolver = domainEventHandlerResolver;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Posts the event asynchronous.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PostEventAsync([FromBody] Object request,
                                                        CancellationToken cancellationToken)
        {
            var domainEvent = await this.GetDomainEvent(request);

            cancellationToken.Register(() => this.Callback(cancellationToken, domainEvent.EventId));

            try
            {
                Logger.LogInformation($"Processing event - ID [{domainEvent.EventId}], Type[{domainEvent.GetType().Name}]");

                List<IDomainEventHandler> eventHandlers = this.DomainEventHandlerResolver.GetDomainEventHandlers(domainEvent);

                if (eventHandlers == null || eventHandlers.Any() == false)
                {
                    // Log a warning out 
                    Logger.LogWarning($"No event handlers configured for Event Type [{domainEvent.GetType().Name}]");
                    return this.Ok();
                }

                List<Task> tasks = new List<Task>();
                foreach (IDomainEventHandler domainEventHandler in eventHandlers)
                {
                    tasks.Add(domainEventHandler.Handle(domainEvent, cancellationToken));
                }

                Task.WaitAll(tasks.ToArray());

                Logger.LogInformation($"Finished processing event - ID [{domainEvent.EventId}]");

                return this.Ok();
            }
            catch (Exception ex)
            {
                String domainEventData = JsonConvert.SerializeObject(domainEvent);
                Logger.LogError(new Exception($" Failed to Process Event, Event Data received [{domainEventData}]", ex));

                throw;
            }
        }

        /// <summary>
        /// Callbacks the specified cancellation token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="eventId">The event identifier.</param>
        private void Callback(CancellationToken cancellationToken,
                              Guid eventId)
        {
            if (cancellationToken.IsCancellationRequested) //I think this would always be true anyway
            {
                Logger.LogInformation($"Cancel request for EventId {eventId}");
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        /// <summary>
        /// Gets the domain event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        /// <returns></returns>
        private async Task<IDomainEvent> GetDomainEvent(Object domainEvent)
        {
            String eventType = this.Request.Query["eventType"].ToString();

            var type = TypeMap.GetType(eventType);

            if (type == null)
                throw new Exception($"Failed to find a domain event with type {eventType}");

            JsonIgnoreAttributeIgnorerContractResolver jsonIgnoreAttributeIgnorerContractResolver = new JsonIgnoreAttributeIgnorerContractResolver();
            var jsonSerialiserSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ContractResolver = jsonIgnoreAttributeIgnorerContractResolver
            };

            if (type.IsSubclassOf(typeof(DomainEvent)))
            {
                var json = JsonConvert.SerializeObject(domainEvent, jsonSerialiserSettings);
                DomainEventFactory domainEventFactory = new();

                return domainEventFactory.CreateDomainEvent(json, type);
            }

            return null;
        }

        #endregion

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "domainevents";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + DomainEventController.ControllerName;

        #endregion
    }
}
