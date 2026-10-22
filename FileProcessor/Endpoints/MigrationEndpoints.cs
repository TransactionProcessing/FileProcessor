using System;
using FileProcessor.Handlers;
using FileProcessor.BusinessLogic.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Authorisation;

namespace FileProcessor.Endpoints;

public static class MigrationEndpoints
{
    public static readonly String Route = "api/admin/migrations/profile-versioning";

    public static void MapMigrationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(Route, MigrationHandlers.MigrateProfileVersions)
            .RequireAuthorization()
            .RequireAuthorization(AuthorizationExtensions.PolicyNames.ClientCredentialsOnlyPolicy)
            .WithTags("Migrations")
            .WithName("MigrateProfileVersions")
            .WithSummary("Migrate file profile versions")
            .WithDescription("Requires the client-credentials-only authorization policy. The request has no path parameters, query parameters, or body. Runs the file profile version migration and returns its report.")
            .Produces<FileProfileVersionMigrationReport>(StatusCodes.Status200OK)
            .WithEndpointExamples();
    }
}
