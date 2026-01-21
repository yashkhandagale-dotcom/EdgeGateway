using EdgeGateway.Application.Interfaces;
using EdgeGateway.Domain.Entities;

namespace EdgeGateway.Application.Services
{
    public class EdgeTelemetryService
    {
        private readonly IEdgeTelemetryService _influxService;

        public EdgeTelemetryService(IEdgeTelemetryService influxService)
        {
            _influxService = influxService;
        }

        public async Task SendSignalAsync(Signal signal)
        {
            await _influxService.WriteSignalAsync(signal);
        }

        public async Task<IEnumerable<Signal>> GetSignalsAsync(string assetId, DateTime from, DateTime to)
        {
            return await _influxService.ReadSignalsAsync(assetId, from, to);
        }
    }
}
