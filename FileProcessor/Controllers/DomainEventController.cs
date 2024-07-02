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
    using Newtonsoft.Json.Linq;
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

            List<IDomainEventHandler> eventHandlers = this.GetDomainEventHandlers(domainEvent);

            try
            {
                Logger.LogWarning($"Processing event - ID [{domainEvent.EventId}], Type[{domainEvent.GetType().Name}]");

                if (eventHandlers == null || eventHandlers.Any() == false)
                {
                    // Log a warning out 
                    Logger.LogInformation($"No event handlers configured for Event Type [{domainEvent.GetType().Name}]");
                    return this.Ok();
                }

                List<Task> tasks = new List<Task>();
                foreach (IDomainEventHandler domainEventHandler in eventHandlers)
                {
                    tasks.Add(domainEventHandler.Handle(domainEvent, cancellationToken));
                }

                Task.WaitAll(tasks.ToArray());

                Logger.LogWarning($"Finished processing event - ID [{domainEvent.EventId}]");

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

        private List<IDomainEventHandler> GetDomainEventHandlers(IDomainEvent domainEvent)
        {

            if (this.Request.Headers.ContainsKey("EventHandler"))
            {
                var eventHandler = this.Request.Headers["EventHandler"];
                var eventHandlerType = this.Request.Headers["EventHandlerType"];
                var resolver = Startup.Container.GetInstance<IDomainEventHandlerResolver>(eventHandlerType);
                // We are being told by the caller to use a specific handler
                var allhandlers = resolver.GetDomainEventHandlers(domainEvent);
                var handlers = allhandlers.Where(h => h.GetType().Name.Contains(eventHandler));

                return handlers.ToList();

            }

            List<IDomainEventHandler> eventHandlers = this.DomainEventHandlerResolver.GetDomainEventHandlers(domainEvent);
            return eventHandlers;
        }

        private async Task<IDomainEvent> GetDomainEvent(Object domainEvent)
        {
            String eventType = this.Request.Headers["eventType"].ToString();

            Type type = TypeMap.GetType(eventType);

            if (type == null)
                throw new Exception($"Failed to find a domain event with type {eventType}");

            JsonIgnoreAttributeIgnorerContractResolver jsonIgnoreAttributeIgnorerContractResolver = new JsonIgnoreAttributeIgnorerContractResolver();
            JsonSerializerSettings jsonSerialiserSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ContractResolver = jsonIgnoreAttributeIgnorerContractResolver
            };

            if (type.IsSubclassOf(typeof(DomainEvent)))
            {
                String json = JsonConvert.SerializeObject(domainEvent, jsonSerialiserSettings);

                DomainEventFactory domainEventFactory = new();
                String validatedJson = this.ValidateEvent(json);
                return domainEventFactory.CreateDomainEvent(validatedJson, type);
            }

            return null;
        }

        private String ValidateEvent(String domainEventJson)
        {
            JObject domainEvent = JObject.Parse(domainEventJson);

            if (domainEvent.ContainsKey("eventId") == false || domainEvent["eventId"].ToObject<Guid>() == Guid.Empty)
            {
                throw new ArgumentException("Domain Event must contain an Event Id");
            }

            return domainEventJson;
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
