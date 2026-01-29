using EdgeGateway.Application.Interfaces;

namespace EdgeGateway.Application.Services;

public class SignalSyncService
{
    private readonly IInfluxSignalReader _reader;
    private readonly IMessageBatchPublisher _batchPublisher;

    public SignalSyncService(
        IInfluxSignalReader reader,
        IMessageBatchPublisher batchPublisher)
    {
        _reader = reader;
        _batchPublisher = batchPublisher;
    }
    public async Task SyncAsync(DateTime from, DateTime to)
    {
        Console.WriteLine("===== SYNC ASYNC ENTERED =====");
        Console.WriteLine($"FROM: {from:O}");
        Console.WriteLine($"TO  : {to:O}");

        var signals = (await _reader.ReadAsync(from, to)).ToList();
        Console.WriteLine($"SIGNALS COUNT: {signals.Count}");

        if (!signals.Any())
        {
            Console.WriteLine("No signals found for the given time range.");
        }
        else
        {
            const int batchSize = 5;

            for (int i = 0; i < signals.Count; i += batchSize)
            {
                var batch = signals.Skip(i).Take(batchSize).ToList();
                Console.WriteLine($"\nPublishing batch {i / batchSize + 1} with {batch.Count} signals...");

                await _batchPublisher.PublishAsync(batch);

                foreach (var s in batch)
                {
                    Console.WriteLine($"Published Signal: {s.SignalId} = {s.Value} at {s.Timestamp:O}");
                }
            }
        }

        Console.WriteLine("===== SYNC ASYNC EXIT =====");
    }

}
