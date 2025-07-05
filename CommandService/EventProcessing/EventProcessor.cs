using System.Text.Json;
using AutoMapper;
using CommandService.Data;
using CommandService.Dtos;
using CommandService.Models;

namespace CommandService.EventProcessing;

public class EventProcessor : IEventProcessor
{
  private readonly IServiceScopeFactory _scopeFactory;
  private readonly IMapper _mapper;

  public EventProcessor(IServiceScopeFactory scopeFactory, IMapper mapper)
  {
    _scopeFactory = scopeFactory;
    _mapper = mapper;
  }

  public async Task ProcessEvent(string message)
  {
    var eventType = DetermineEvent(message);
    switch (eventType)
    {
      case EventType.PlatformPublished:
        await AddPlatform(message);
        break;
      default:
        break;
    }
  }

  private EventType DetermineEvent(string notificationMessage)
  {
    Console.WriteLine("--> Determining Event");
    var eventType = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage)!;

    switch (eventType.Event)
    {
      case "Platform_Published":
        Console.WriteLine("--> Platform Published Event Detected");
        return EventType.PlatformPublished;
      default:
        Console.WriteLine("--> Could not determine the event type");
        return EventType.Undetermined;
    }
  }

  private async Task AddPlatform(string platformPublishedMessage)
  {
    using (var scope = _scopeFactory.CreateScope())
    {
      var repo = scope.ServiceProvider.GetRequiredService<ICommandRepo>();
      var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishDto>(platformPublishedMessage);
      if (platformPublishedDto == null)
      {
        Console.WriteLine("--> Could not deserialize PlatformPublishedDto");
        return;
      }

      try
      {
        var platform = _mapper.Map<Platform>(platformPublishedDto);
        if (await repo.ExternalPlatformExists(platform.ExternalID))
        {
          Console.WriteLine("--> Platform already exists in DB");
          return;
        }
        await repo.CreatePlatform(platform);
        await repo.SaveChanges();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"--> Could not add Platform to DB: {ex.Message}");
      }
    }
  }
}

enum EventType
{
  PlatformPublished,
  Undetermined
}