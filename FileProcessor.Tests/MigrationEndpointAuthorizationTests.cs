using System.Linq;
using System.Threading.Tasks;
using FileProcessor.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Shared.Authorisation;
using Shouldly;
using Xunit;

namespace FileProcessor.Tests;

public class MigrationEndpointAuthorizationTests
{
    [Fact]
    public async Task MigrationEndpoint_RequiresClientCredentialsPolicy()
    {
        WebApplication app = WebApplication.CreateBuilder().Build();
        app.MapMigrationEndpoints();
        await app.StartAsync();

        RouteEndpoint endpoint = app.Services.GetRequiredService<EndpointDataSource>()
                                    .Endpoints
                                    .OfType<RouteEndpoint>()
                                    .Single(endpoint => endpoint.Metadata.GetMetadata<EndpointNameMetadata>()?.EndpointName == "MigrateProfileVersions");

        endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>()
               .ShouldContain(metadata => metadata.Policy == AuthorizationExtensions.PolicyNames.ClientCredentialsOnlyPolicy);
    }
}
