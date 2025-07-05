using System.Text;
using System.Text.Json;
using PlatformService.Dtos;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PlatformService.AsyncDataServices;

public class MessageBusClient : IMessageBusClient
{
  private readonly IConfiguration _configuration;
  private IConnection? _connection;
  private IChannel? _channel; 
  
  private const string _exchangeName = "trigger";

  public MessageBusClient(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  public async Task PublishNewPlatform(PlatformPublishDto platformPublishDto)
  {
    await InitializeRabbitMQ();

    var message = JsonSerializer.Serialize(platformPublishDto);
    if (_connection.IsOpen)
    {
      Console.WriteLine("--> RabbitMQ Connection Open, sending message...");
      
      await SendMessage(message);
    }
    else
    {
      Console.WriteLine("--> RabbitMQ Connection Closed, not sending");
    }
  }
  
  public void Dispose()
  {
    Console.WriteLine("MessageBus Disposed");
    if (_channel.IsOpen)
    {
      _channel.CloseAsync();
      _connection.CloseAsync();
    }
  }
  
  private async Task SendMessage(string message)
  {
    var body = Encoding.UTF8.GetBytes(message);
    await _channel.BasicPublishAsync(_exchangeName, "", body: body);
    
    Console.WriteLine($"--> We have sent {message}");
  }
  
  private async Task InitializeRabbitMQ()
  {
    if (_connection != null && _channel != null)
    {
      return;
    }
    
    var factory = new ConnectionFactory()
    {
      HostName = _configuration["RabbitMQHost"],
      Port = int.Parse(_configuration["RabbitMQPort"])
    };
    
    try
    {
      _connection = await factory.CreateConnectionAsync();
      _channel = await _connection.CreateChannelAsync();
      await _channel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Fanout);
      _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdownAsync;
      
      Console.WriteLine("--> Connected to Message Bus");
      
      //await _channel.QueueDeclareAsync("platform_queue", false, false, false, null);
      //await _channel.QueueBindAsync("platform_queue", "trigger", "");
    }
    catch (Exception e)
    {
      Console.WriteLine($"--> Could not connect to the Message Bus: {e.Message}");
    }
    
  }

  private Task RabbitMQ_ConnectionShutdownAsync(object sender, ShutdownEventArgs e)
  {
    Console.WriteLine("--> RabbitMQ Connection Shutdown");
    return Task.CompletedTask;
  }
}