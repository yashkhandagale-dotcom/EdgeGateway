using EdgeGateway.Domain.Entities;

namespace EdgeGateway.Application.Interfaces
{
    public interface IEdgeTelemetryService
    {
        Task WriteSignalAsync(Signal signal);
        Task<IEnumerable<Signal>> ReadSignalsAsync(string assetId, DateTime from, DateTime to);
    }
}
