using AutoMapper;
using CommandService.Data;
using CommandService.Dtos;
using CommandService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandService.Controllers;

[Route("api/c/platforms/{platformId}/[controller]")]
[ApiController]
public class CommandsController : ControllerBase
{
  private readonly ICommandRepo _repo;
  private readonly IMapper _mapper;

  public CommandsController(ICommandRepo repo, IMapper mapper)
  {
    _repo = repo;
    _mapper = mapper;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<CommandReadDto>>> GetCommandsForPlatform(int platformId, CancellationToken ct)
  {
    Console.WriteLine($"--> Getting Commands for Platform {platformId}");
    var commands = await _repo.GetCommandsForPlatform(platformId, ct);
    return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commands));
  }

  [HttpGet("{commandId}")]
  public async Task<ActionResult<CommandReadDto>> GetCommandForPlatform(int platformId, int commandId, CancellationToken ct)
  {
    Console.WriteLine($"--> Getting Command {commandId} for Platform {platformId}");
    if (!await _repo.PlatformExists(platformId, ct))
    {
      return NotFound();
    }
    var command = await _repo.GetCommand(platformId, commandId, ct);
    if (command == null)
    {
      return NotFound();
    }
    return Ok(_mapper.Map<CommandReadDto>(command));
  }

  [HttpPost]
  public async Task<ActionResult<CommandReadDto>> CreateCommandForPlatform(int platformId, CommandCreateDto commandCreateDto, CancellationToken ct)
  {
    Console.WriteLine($"--> Creating Command for Platform {platformId}");
    if (!await _repo.PlatformExists(platformId, ct))
    {
      return NotFound();
    }
    var command = _mapper.Map<Command>(commandCreateDto);
    await _repo.CreateCommand(platformId, command, ct);
    await _repo.SaveChanges(ct);

    var commandReadDto = _mapper.Map<CommandReadDto>(command);
    return CreatedAtAction(nameof(GetCommandsForPlatform), new { platformId }, commandReadDto);
  }
}