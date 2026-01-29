    using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using EdgeGateway.Application.Interfaces;
using EdgeGateway.Domain.Entities;

namespace EdgeGateway.Infrastructure.Messaging;

public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    private const string ExchangeName = "signals.exchange";
    private const string QueueName = "machine.signals.queue";
    private const string RoutingKey = "machine.signal";

    public RabbitMqPublisher(string host, int port, string username, string password)
    {
        var factory = new ConnectionFactory
        {
            HostName = host,
            Port = port,
            UserName = username,
            Password = password
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, durable: true);
        _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(QueueName, ExchangeName, RoutingKey);
    }

    public Task PublishAsync(SignalData signal)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(signal));

        _channel.BasicPublish(
            exchange: ExchangeName,
            routingKey: RoutingKey,
            basicProperties: null,
            body: body
        );

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel.Close();
        _connection.Close();
    }
}
