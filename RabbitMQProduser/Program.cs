using RabbitMQ.Client;
using RabbitMQProduser;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory() { HostName = "localhost", };

const string CONFIG_QUEUE = "configQueue";

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: CONFIG_QUEUE, durable: false, exclusive: false, autoDelete: false, arguments: null);

for (int i = 0; i < 1000; i++)
{
    Message m = new Message() { Name = $"test{i}", Text = $"test{i}" };
    string message = JsonSerializer.Serialize(m);
    var body = Encoding.UTF8.GetBytes(message);

    channel.BasicPublish(exchange: "", routingKey: CONFIG_QUEUE, basicProperties: null, body: body);
}
