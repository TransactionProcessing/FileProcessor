using SimpleResults;
using System.Threading;
using System.Threading.Tasks;

namespace FileProcessor.BusinessLogic.Services;

public interface IFileProfileDirectoryRecoveryService
{
    Task<Result> RecoverInProgressFilesAsync(CancellationToken cancellationToken);
}
