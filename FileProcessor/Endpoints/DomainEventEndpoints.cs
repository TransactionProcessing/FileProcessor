using System;
using FileProcessor.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FileProcessor.Endpoints;

public static class DomainEventEndpoints
{
    public static readonly String Route = "api/domainevents";

    public static void MapDomainEventEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost(Route, DomainEventHandlers.HandleDomainEvent)
            .WithName("DomainEvent")
            .WithSummary("Process a domain event")
            .WithDescription("Internal event-delivery endpoint. Send the event JSON body and identify its registered event type in the `eventType` request header. Returns 200 when all configured handlers succeed or no handlers are configured; returns 500 when one or more handlers fail.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithEndpointExamples();
    }
}
