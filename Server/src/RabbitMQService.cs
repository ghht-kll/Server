using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Server.src;

public class RabbitMQService : IDisposable
{
    public RabbitMQService(RedisService redisService)
    {
        Console.WriteLine("Sleeping to wait for Rabbit");
        Task.Delay(20000).Wait();
        Console.WriteLine("Posting messages to webApi");

        _redisService = redisService;

        _rabbitMqfactory = new ConnectionFactory() { Uri = new Uri("amqp://guest:guest@rabbitmq/") };
        _rabbitMqConnection = _rabbitMqfactory.CreateConnection();
        _rabbitMqChannel = _rabbitMqConnection.CreateModel();
    }

    public void ReceivingMessages()
    {
        _rabbitMqChannel.QueueDeclare(queue: _configQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
        _rabbitMqChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        _eventingBasicConsumer = new EventingBasicConsumer(_rabbitMqChannel);

        _eventingBasicConsumer.Received += async (model, args) =>
        {
            await Task.Run(() =>
            {
                try
                {
                    var body = args.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    Message? message = JsonSerializer.Deserialize<Message>(json);

                    if (message is not null)
                        _redisService.Set(message);
                    else throw new NullReferenceException();

                    _rabbitMqChannel.BasicAck(deliveryTag: args.DeliveryTag, multiple: false);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        };
        _rabbitMqChannel.BasicConsume(queue: _configQueue, autoAck: false, consumer: _eventingBasicConsumer);
    }

    public void ProcessingRPCRequests()
    {
        _rabbitMqChannel.QueueDeclare(queue: _rcpQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
        _rabbitMqChannel.BasicQos(0, 1, false);
        _eventingBasicConsumer = new EventingBasicConsumer(_rabbitMqChannel);
        _rabbitMqChannel.BasicConsume(queue: _rcpQueue, autoAck: false, consumer: _eventingBasicConsumer);

        _eventingBasicConsumer.Received += async (model, ea) =>
        {
            await Task.Run(() =>
            {
                string response = "";

                var body = ea.Body.ToArray();
                var props = ea.BasicProperties;
                var replyProps = _rabbitMqChannel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                try
                {
                    var message = Encoding.UTF8.GetString(body);
                    if (message == "getChat")
                        response = JsonSerializer.Serialize(_redisService.GetAll());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    _rabbitMqChannel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: responseBytes);
                    _rabbitMqChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
            });
        };
    }

    void IDisposable.Dispose()
    {
        _rabbitMqChannel?.Dispose();
        _rabbitMqConnection?.Dispose();
    }

    private ConnectionFactory _rabbitMqfactory;
    private IConnection _rabbitMqConnection;
    private IModel _rabbitMqChannel;
    private EventingBasicConsumer? _eventingBasicConsumer = null;
    const string _configQueue = "configQueue";
    const string _rcpQueue = "rpc_queue";
    private RedisService _redisService;
}
