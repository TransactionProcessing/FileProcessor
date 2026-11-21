using System.Threading;
using System.Threading.Tasks;
using FileProcessor.BusinessLogic.Services;
using TransactionProcessing.UnionResults;

namespace FileProcessor.Handlers;

public static class MigrationHandlers
{
    public static async Task<Result<object?>> MigrateProfileVersions(IFileProfileVersionMigrationService migrationService,
                                                                                          CancellationToken cancellationToken)
    {
        return ResponseFactory.FromResult(await migrationService.RunAsync(cancellationToken), report => report);
    }
}
