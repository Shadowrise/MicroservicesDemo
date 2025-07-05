using Microsoft.AspNetCore.Mvc;
using PlatformService.Data;
using PlatformService.Models;
using PlatformService.Dtos;
using AutoMapper;
using PlatformService.AsyncDataServices;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlatformsController : ControllerBase
{
  private readonly IPlatformRepo _repo;
  private readonly IMapper _mapper;
  private readonly ICommandDataClient _commandDataClient;
  private readonly IMessageBusClient _messageBusClient;

  public PlatformsController(IPlatformRepo repo, IMapper mapper, ICommandDataClient commandDataClient, IMessageBusClient messageBusClient)
  {
    _repo = repo;
    _mapper = mapper;
    _commandDataClient = commandDataClient;
    _messageBusClient = messageBusClient;  
  }

  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<PlatformReadDto>>> GetAllPlatforms(CancellationToken cancellationToken)
  {
    var platforms = await _repo.GetAllPlatforms(cancellationToken);

    return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platforms));
  }

  [HttpGet("{id:int}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<PlatformReadDto>> GetPlatformById(int id, CancellationToken cancellationToken)
  {
    var platform = await _repo.GetPlatformById(id, cancellationToken);
    if (platform == null) return NotFound();

    return Ok(_mapper.Map<PlatformReadDto>(platform));
  }

  [HttpPost]
  [ProducesResponseType(StatusCodes.Status201Created)]
  public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto dto, CancellationToken cancellationToken)
  {
    var platform = _mapper.Map<Platform>(dto);
    await _repo.CreatePlatform(platform, cancellationToken);
    await _repo.SaveChanges(cancellationToken);
    var readDto = _mapper.Map<PlatformReadDto>(platform);

    try
    {
      await _commandDataClient.SendPlatformToCommand(readDto);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"--> Could not send synchronously: {ex.Message}");
    }
    
    try
    {
      var platformPublishDto = _mapper.Map<PlatformPublishDto>(readDto);
      platformPublishDto.Event = "Platform_Published";
      await _messageBusClient.PublishNewPlatform(platformPublishDto);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"--> Could not send asynchronously: {ex.Message}");
    }

    return CreatedAtAction(nameof(GetPlatformById), new { id = readDto.Id }, readDto);
  }
}