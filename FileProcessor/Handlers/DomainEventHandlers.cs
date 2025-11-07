using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.EventStore.EventHandling;
using Shared.Exceptions;
using Shared.General;
using Shared.Logger;
using Shared.Serialisation;
using SimpleResults;
using static FileProcessor.Common.ModelFactory;
using ModelFactory = FileProcessor.Common.ModelFactory;

namespace FileProcessor.Handlers
{
    public static class DomainEventHandlers
    {
        public static async Task<IResult> HandleDomainEvent(HttpRequest request,
                                                            object body,
                                                            IDomainEventHandlerResolver resolver,
                                                            CancellationToken cancellationToken)
        {
            IDomainEvent domainEvent = await GetDomainEvent(request, body);

            cancellationToken.Register(() => Callback(cancellationToken, domainEvent.EventId));

            try
            {
                Logger.LogInformation($"Processing event - ID [{domainEvent.EventId}], Type[{domainEvent.GetType().Name}]");

                Result<List<IDomainEventHandler>> eventHandlersResult = resolver.GetDomainEventHandlers(domainEvent);

                if (eventHandlersResult.IsFailed)
                {
                    Logger.LogWarning($"No event handlers configured for Event Type [{domainEvent.GetType().Name}]");
                    return Results.Ok();
                }

                var eventHandlers = eventHandlersResult.Data;
                var tasks = eventHandlers.Select(h => h.Handle(domainEvent, cancellationToken));
                await Task.WhenAll(tasks);

                Logger.LogInformation("Finished processing event - ID [{domainEvent.EventId}]");

                return Results.Ok();
            }
            catch (Exception ex)
            {
                string domainEventData = JsonConvert.SerializeObject(domainEvent);
                Logger.LogError($"Failed to process event. Data received [{domainEventData}]", ex);
                throw;
            }
        }

        private static void Callback(CancellationToken cancellationToken, Guid eventId)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Logger.LogInformation($"Cancel request for EventId {eventId}");
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private static async Task<IDomainEvent> GetDomainEvent(HttpRequest request, object domainEvent)
        {
            string eventType = request.Headers["eventType"].ToString();

            Type type = TypeMap.GetType(eventType);

            if (type == null)
                throw new NotFoundException($"Failed to find a domain event with type {eventType}");

            var resolver = new JsonIgnoreAttributeIgnorerContractResolver();
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ContractResolver = resolver
            };

            if (type.IsSubclassOf(typeof(DomainEvent)))
            {
                string json = JsonConvert.SerializeObject(domainEvent, settings);

                var factory = new DomainEventFactory();
                string validatedJson = ValidateEvent(json);
                return factory.CreateDomainEvent(validatedJson, type);
            }

            return null;
        }

        private static string ValidateEvent(string domainEventJson)
        {
            var domainEvent = JObject.Parse(domainEventJson);

            if (!domainEvent.ContainsKey("eventId") ||
                domainEvent["eventId"]!.ToObject<Guid>() == Guid.Empty)
            {
                throw new ArgumentException("Domain Event must contain an Event Id");
            }

            return domainEventJson;
        }
    }
}
