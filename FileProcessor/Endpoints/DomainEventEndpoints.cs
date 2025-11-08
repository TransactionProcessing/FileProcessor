using FileProcessor.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FileProcessor.Endpoints;

public static class DomainEventEndpoints
{
    private const string Route = "api/domainevents";

    public static void MapDomainEventEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost(Route, DomainEventHandlers.HandleDomainEvent)
            .ExcludeFromDescription() // hides from Swagger
            .WithName("DomainEvent");
    }
}