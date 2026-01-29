//using RabbitMQ.Client;
//using System.Text;
//using System.Text.Json;
//using EdgeGateway.Domain.Entities;

//namespace EdgeGateway.Infrastructure.Messaging;

//public class RabbitMqBatchPublisher : IMessageBatchPublisher, IDisposable
//{
//    private readonly IConnection _connection;
//    private readonly IModel _channel;
//    private readonly string _exchange;

//    public RabbitMqBatchPublisher(
//        string host,
//        int port,
//        string username,
//        string password,
//        string exchange,
//        bool useTls = true)
//    {
//        _exchange = exchange;

//        var factory = new ConnectionFactory
//        {
//            HostName = host,
//            Port = port,
//            UserName = username,
//            Password = password
//        };

//        if (useTls)
//        {
//            factory.Ssl.Enabled = true;
//            factory.Ssl.ServerName = host;  // match your server certificate
//            factory.Ssl.Version = System.Security.Authentication.SslProtocols.Tls12;
//            factory.Ssl.AcceptablePolicyErrors =
//                System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch |
//                System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors;
//        }

//        _connection = factory.CreateConnection();
//        _channel = _connection.CreateModel();

//        // Declare exchange (direct)
//        _channel.ExchangeDeclare(exchange: _exchange, type: ExchangeType.Direct, durable: true);
//    }

//    public Task PublishAsync(IEnumerable<SignalData> signals)
//    {
//        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(signals));

//        _channel.BasicPublish(
//            exchange: _exchange,
//            routingKey: "signal.batch",
//            basicProperties: null,
//            body: body
//        );

//        return Task.CompletedTask;
//    }

//    public void Dispose()
//    {
//        _channel?.Close();
//        _connection?.Close();
//    }
//}



using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using EdgeGateway.Domain.Entities;
using EdgeGateway.Application.Interfaces;

namespace EdgeGateway.Infrastructure.Messaging
{
    public class RabbitMqBatchPublisher : IMessageBatchPublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchange;
        private readonly string _queueName = "signals.queue";
        private readonly string _routingKey = "signal.batch";

        public RabbitMqBatchPublisher(
            string host,
            int port,
            string username,
            string password,
            string exchange,
            bool useTls = false)
        {
            _exchange = exchange;

            var factory = new ConnectionFactory
            {
                HostName = host,
                Port = port,
                UserName = username,
                Password = password
            };

            // Enable TLS if required
            if (useTls)
            {
                factory.Ssl.Enabled = true;
                factory.Ssl.ServerName = host;
                factory.Ssl.Version = System.Security.Authentication.SslProtocols.Tls12;
                factory.Ssl.AcceptablePolicyErrors =
                    System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch |
                    System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors;
            }

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // 1️⃣ Declare exchange (Direct)
            _channel.ExchangeDeclare(
                exchange: _exchange,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false
            );

            // 2️⃣ Declare queue
            _channel.QueueDeclare(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            // 3️⃣ Bind queue to exchange
            _channel.QueueBind(
                queue: _queueName,
                exchange: _exchange,
                routingKey: _routingKey
            );
        }

        public Task PublishAsync(IEnumerable<SignalData> signals)
        {
            var json = JsonSerializer.Serialize(signals);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true; // message survives broker restart

            _channel.BasicPublish(
                exchange: _exchange,
                routingKey: _routingKey,
                basicProperties: properties,
                body: body
            );

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
