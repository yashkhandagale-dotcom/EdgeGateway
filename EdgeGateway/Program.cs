using EdgeGateway.Application.Services;
using EdgeGateway.Application.Interfaces; // Add this line
using EdgeGateway.Domain.Entities;
using EdgeGateway.Infrastructure.Persistence;
using EdgeGateway.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    static async Task Main()
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        // Setup Dependency Injection
        var services = new ServiceCollection();
        services.Configure<InfluxDbOptions>(configuration.GetSection("InfluxDbOptions"));
        services.AddSingleton<IEdgeTelemetryService, InfluxDbTelemetryService>();
        services.AddSingleton<EdgeTelemetryService>();

        var serviceProvider = services.BuildServiceProvider();

        var telemetryService = serviceProvider.GetRequiredService<EdgeTelemetryService>();

        // Simulate device sending signals continuously
        for (int i = 0; i < 50; i++)
        {   
            var signal = new Signal
            {
                AssetId = "asset-001",
                DeviceId = "device-001",
                SignalTypeId = "temperature",
                MappingId = "map-001",
                Value = 20 + i,
                Unit = "C",
                Timestamp = DateTime.UtcNow
            };

            await telemetryService.SendSignalAsync(signal);
            await Task.Delay(1000); // simulate device polling interval
        }

        // Read back signals from InfluxDB
        var signals = await telemetryService.GetSignalsAsync("asset-001", DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow);
        foreach (var s in signals)
        {
            Console.WriteLine($"{s.Timestamp} | {s.AssetId} | {s.Value}{s.Unit}");
        }
    }
}
