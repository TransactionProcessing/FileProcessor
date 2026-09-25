using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.EventStore.EventHandling;
using Shared.Exceptions;
using Shared.General;
using Shared.Logger;
using Shared.Serialisation;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

                List<HandlerExecution> executions = eventHandlersResult.Data
                    .Select(domainEventHandler => ExecuteHandler(domainEventHandler, domainEvent, cancellationToken))
                    .ToList();

                try
                {
                    await Task.WhenAll(executions.Select(execution => execution.Task));
                }
                catch (Exception ex)
                {
                    Logger.LogError(new Exception($"One or more handlers threw while processing event [{domainEvent.EventId}]", ex));
                }

                List<HandlerFailure> failures = executions.SelectMany(GetFailures).ToList();
                if (failures.Any())
                {
                    return Results.Problem(title: "One or more event handlers failed",
                                        statusCode: 500,
                                        extensions: new Dictionary<String, Object>
                                        {
                                            ["eventId"] = domainEvent.EventId,
                                            ["eventType"] = domainEvent.GetType().Name,
                                            ["failures"] = failures
                                        });
                }

                Logger.LogInformation("Finished processing event - ID [{domainEvent.EventId}]");

                return Results.Ok();
            }
            catch (Exception ex)
            {
                string domainEventData = StringSerialiser.Serialise(domainEvent);
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

        private static HandlerExecution ExecuteHandler(IDomainEventHandler handler,
                                                IDomainEvent domainEvent,
                                                CancellationToken cancellationToken)
        {
            try
            {
                return new HandlerExecution(handler, handler.Handle(domainEvent, cancellationToken));
            }
            catch (Exception ex)
            {
                return new HandlerExecution(handler, Task.FromException<Result>(ex));
            }
        }

        private sealed record HandlerExecution(IDomainEventHandler Handler, Task<Result> Task);

        private sealed record HandlerFailure(String Handler, String Error);

        private static IEnumerable<HandlerFailure> GetFailures(HandlerExecution execution)
        {
            if (execution.Task.IsFaulted)
            {
                String error = String.Join("; ", execution.Task.Exception?.Flatten().InnerExceptions
                    .SelectMany(exception => exception.GetExceptionMessages()) ?? Enumerable.Empty<String>());
                yield return new HandlerFailure(execution.Handler.GetType().Name, error);
            }
            else if (execution.Task.IsCanceled)
            {
                yield return new HandlerFailure(execution.Handler.GetType().Name, "Handler execution was cancelled");
            }
            else if (execution.Task.Result.IsFailed)
            {
                yield return new HandlerFailure(execution.Handler.GetType().Name, execution.Task.Result.Message);
            }
        }

        private static async Task<IDomainEvent> GetDomainEvent(HttpRequest request, object domainEvent)
        {
            string eventType = request.Headers["eventType"].ToString();

            Type type = TypeMap.GetType(eventType);

            if (type == null)
                throw new NotFoundException($"Failed to find a domain event with type {eventType}");
            
            if (type.IsSubclassOf(typeof(DomainEvent)))
            {
                String json = StringSerialiser.Serialise(domainEvent);

                Logger.LogInformation($"Deserialising event. Type [{type.Name}], Json [{json}]");

                var factory = new DomainEventFactory();
                return factory.CreateDomainEvent(json, type);
            }

            return null;
        }
    }
}
