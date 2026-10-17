using System.Threading;
using System.Threading.Tasks;
using FileProcessor.BusinessLogic.Services;
using Shared.Results.Web;

namespace FileProcessor.Handlers;

public static class MigrationHandlers
{
    public static async Task<Microsoft.AspNetCore.Http.IResult> MigrateProfileVersions(IFileProfileVersionMigrationService migrationService,
                                                                                          CancellationToken cancellationToken)
    {
        return ResponseFactory.FromResult(await migrationService.RunAsync(cancellationToken), report => report);
    }
}
