using System.Threading;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IHandlerJob
    {
        Task HandleSync(string jobId, CancellationToken ct);
    }
}
