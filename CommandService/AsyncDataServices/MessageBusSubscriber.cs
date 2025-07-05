using System.Text;
using CommandService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommandService.AsyncDataServices;

public class MessageBusSubscriber : BackgroundService
{
  private readonly IConfiguration _configuration;
  private readonly IEventProcessor _eventProcessor;
  private IConnection _connection;
  private IChannel _channel;
  private string _queueName;

  public MessageBusSubscriber(IConfiguration configuration, IEventProcessor eventProcessor)
  {
    _configuration = configuration;
    _eventProcessor = eventProcessor;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    await InitializeRabbitMQ();

    stoppingToken.ThrowIfCancellationRequested();

    var consumer = new AsyncEventingBasicConsumer(_channel);

    consumer.ReceivedAsync += async (ModuleHandle, ea) =>
    {
      Console.WriteLine("--> Event Received!");
      var message = Encoding.UTF8.GetString(ea.Body.ToArray());
      await _eventProcessor.ProcessEvent(message);
    };

    await _channel.BasicConsumeAsync(queue: _queueName, autoAck: true, consumer: consumer);
  }

  private async Task InitializeRabbitMQ()
  {
    var factory = new ConnectionFactory()
    {
      HostName = _configuration["RabbitMQHost"]!,
      Port = int.Parse(_configuration["RabbitMQPort"]!)
    };

    _connection = await factory.CreateConnectionAsync();
    _channel = await _connection.CreateChannelAsync();

    await _channel.ExchangeDeclareAsync(exchange: "trigger", type: ExchangeType.Fanout);

    _queueName = await _channel.QueueDeclareAsync();
    await _channel.QueueBindAsync(queue: _queueName, exchange: "trigger", routingKey: "");

    Console.WriteLine("--> Listening on the Message Bus...");

    _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdownAsync;
  }

  private async Task RabbitMQ_ConnectionShutdownAsync(object sender, ShutdownEventArgs @event)
  {
    Console.WriteLine("--> Connection Shutdown");
  }

  public override void Dispose()
  {
    if (_channel.IsOpen)
    {
      _channel.CloseAsync();
      _connection.CloseAsync();
    }
    base.Dispose();
  }
}