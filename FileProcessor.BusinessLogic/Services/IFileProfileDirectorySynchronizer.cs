using SimpleResults;
using System.Threading;
using System.Threading.Tasks;

namespace FileProcessor.BusinessLogic.Services;

public interface IFileProfileDirectorySynchronizer
{
    Task<Result> SyncAsync(CancellationToken cancellationToken);
}
