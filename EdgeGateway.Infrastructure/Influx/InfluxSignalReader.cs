using InfluxDB.Client;
using EdgeGateway.Application.Interfaces;
using EdgeGateway.Domain.Entities;

namespace EdgeGateway.Infrastructure.Influx;

public class InfluxSignalReader : IInfluxSignalReader
{
    private readonly InfluxDBClient _client;
    private readonly string _org;
    private readonly string _bucket;

    public InfluxSignalReader(string url, string token, string org, string bucket)
    {
        _client = InfluxDBClientFactory.Create(url, token);
        _org = org;
        _bucket = bucket;
    }

    public async Task<IEnumerable<SignalData>> ReadAsync(DateTime from, DateTime to)
    {
        var fromIso = from.ToUniversalTime().ToString("o");
        var toIso = to.ToUniversalTime().ToString("o");

        var flux = $@"
        from(bucket: ""{_bucket}"")
          |> range(
              start: time(v: ""{fromIso}""),
              stop:  time(v: ""{toIso}"")
          )
          |> filter(fn: (r) => r._measurement == ""ps_test"")
        ";

        var tables = await _client.GetQueryApi().QueryAsync(flux, _org);

        return tables
            .SelectMany(t => t.Records)
            .Select(r => new SignalData
            {
                SignalId = r.Values.TryGetValue("signalId", out var id)
                    ? id?.ToString()!
                    : "unknown",
                Value = Convert.ToDouble(r.GetValue()),
                Timestamp = r.GetTimeInDateTime().GetValueOrDefault()
            });
    }
}
