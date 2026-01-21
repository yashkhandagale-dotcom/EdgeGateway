using EdgeGateway.Application.Interfaces;
using EdgeGateway.Domain.Entities;
using EdgeGateway.Infrastructure.Configuration;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Options;

namespace EdgeGateway.Infrastructure.Persistence
{
    public class InfluxDbTelemetryService : IEdgeTelemetryService
    {
        private readonly InfluxDBClient _client;
        private readonly string _bucket;
        private readonly string _org;

        public InfluxDbTelemetryService(IOptions<InfluxDbOptions> options)
        {
            var config = options.Value;

            _bucket = config.Bucket ?? string.Empty;
            _org = config.Org ?? string.Empty;

            // Correct way for latest InfluxDB.Client
            _client = new InfluxDBClient(config.Url, config.Token);
        }


        public async Task WriteSignalAsync(Signal signal)
        {
            // ✅ Create PointData
            var point = PointData.Measurement("asset_signals")
    .Tag("assetId", signal.AssetId)
    .Tag("deviceId", signal.DeviceId)
    .Tag("signalTypeId", signal.SignalTypeId)
    .Tag("mappingId", signal.MappingId)
    .Field("value", signal.Value)  // numeric
    .Field("unit", signal.Unit)    // string, fine if you don’t aggregate on it
    .Timestamp(signal.Timestamp, WritePrecision.Ns);


            // ✅ Get WriteApiAsync (do NOT use 'using')
            var writeApi = _client.GetWriteApiAsync();
            await writeApi.WritePointAsync(point, _bucket, _org);

            Console.WriteLine($"Signal written: {signal.AssetId} | {signal.Value}");
        }

        public async Task<IEnumerable<Signal>> ReadSignalsAsync(string assetId, DateTime from, DateTime to)
        {
            var flux = $@"
                from(bucket:""{_bucket}"")
                |> range(start: {from:o}, stop: {to:o})
                |> filter(fn: (r) => r.assetId == ""{assetId}"")
                |> filter(fn: (r) => r._field == ""value"")"; 


            var queryApi = _client.GetQueryApi();
            var tables = await queryApi.QueryAsync(flux, _org);

            var result = new List<Signal>();

            foreach (var table in tables)
            {
                foreach (var record in table.Records)
                {
                    var value = record.GetValue();
                    double numericValue = double.TryParse(value?.ToString(), out var parsedValue) ? parsedValue : 0.0;

                    result.Add(new Signal
                    {
                        AssetId = record.GetValueByKey("assetId")?.ToString() ?? string.Empty,
                        DeviceId = record.GetValueByKey("deviceId")?.ToString() ?? string.Empty,
                        SignalTypeId = record.GetValueByKey("signalTypeId")?.ToString() ?? string.Empty,
                        MappingId = record.GetValueByKey("mappingId")?.ToString() ?? string.Empty,
                        Value = numericValue,
                        Unit = record.GetValueByKey("unit")?.ToString() ?? string.Empty,
                        Timestamp = record.GetTimeInDateTime().GetValueOrDefault()
                    });
                }
            }

            return result;
        }
    }
}
