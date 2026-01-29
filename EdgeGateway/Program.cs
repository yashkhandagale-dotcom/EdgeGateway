using EdgeGateway.Application.Interfaces;
using EdgeGateway.Application.Services;
using EdgeGateway.Infrastructure.Influx;
using EdgeGateway.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

// ?? InfluxDB Token
var influxToken = Environment.GetEnvironmentVariable("INFLUX_TOKEN");

if (string.IsNullOrWhiteSpace(influxToken))
{
    throw new Exception("INFLUX_TOKEN environment variable not set");
}

//Console.WriteLine($"INFLUX_TOKEN length: {influxToken.Length}");

// --------------------
// Services
// --------------------

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// InfluxDB
builder.Services.AddSingleton<IInfluxSignalReader>(_ =>
    new InfluxSignalReader(
        "http://localhost:8086",
        influxToken,
        //builder.Configuration["Influx:Token"]!,
        builder.Configuration["Influx:Org"]!,
        builder.Configuration["Influx:Bucket"]!
    ));
// Use plain AMQP (non-TLS) for development
builder.Services.AddSingleton<IMessageBatchPublisher>(_ =>
    new RabbitMqBatchPublisher(
        host: "127.0.0.1",      // Docker host
        port: 59396,            // host port mapping to container 5672
        username: "guest",
        password: "guest",
        exchange: "signals.exchange",
        useTls: false           // plain AMQP for dev
    ));


// Use batch publisher in SignalSyncService
builder.Services.AddScoped<SignalSyncService>();


// Application service
builder.Services.AddScoped<SignalSyncService>();

// --------------------
// App
// --------------------

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
