using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using EdgeGateway.Domain.Entities;

// RabbitMQ connection info
var host = "127.0.0.1";
var port = 59396;
var username = "guest";
var password = "guest";

var exchangeName = "signals.exchange";
var queueName = "machine.signals.queue";
var routingKey = "machine.signal";

// Create connection
var factory = new ConnectionFactory
{
    HostName = host,
    Port = port,
    UserName = username,
    Password = password
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Declare exchange & queue, and bind
channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, durable: true);
channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
channel.QueueBind(queueName, exchangeName, routingKey);

// Create a sample signal
var signal = new SignalData
{
    SignalId = "TEST_SIGNAL_1",
    Value = 123.45,
    Timestamp = DateTime.UtcNow
};

// Serialize to JSON
var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(signal));

// Publish to exchange
channel.BasicPublish(exchange: exchangeName,
                     routingKey: routingKey,
                     basicProperties: null,
                     body: body);

Console.WriteLine("✅ Test message published to RabbitMQ!");
Console.WriteLine("Check the queue 'machine.signals.queue' in RabbitMQ Management UI.");
