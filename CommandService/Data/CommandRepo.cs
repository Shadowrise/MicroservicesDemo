using CommandService.Models;
using Microsoft.EntityFrameworkCore;

namespace CommandService.Data;

public class CommandRepo : ICommandRepo
{
  private readonly AppDbContext _context;

  public CommandRepo(AppDbContext context)
  {
    _context = context;
  }

  public async Task<bool> SaveChanges(CancellationToken ct = default)
  {
    return (await _context.SaveChangesAsync(ct) >= 0);
  }

  public async Task<IEnumerable<Platform>> GetAllPlatforms(CancellationToken ct = default)
  {
    return await _context.Platforms.ToListAsync(ct);
  }

  public async Task CreatePlatform(Platform platform, CancellationToken ct = default)
  {
    if (platform == null) throw new ArgumentNullException(nameof(platform));
    await _context.Platforms.AddAsync(platform, ct);
  }

  public async Task<bool> PlatformExists(int platformId, CancellationToken ct = default)
  {
    return await _context.Platforms.AnyAsync(p => p.Id == platformId, ct);
  }

  public async Task<bool> ExternalPlatformExists(int externalPlatformId, CancellationToken ct = default)
  {
    return await _context.Platforms.AnyAsync(p => p.ExternalID == externalPlatformId, ct);
  }

  public async Task<IEnumerable<Command>> GetCommandsForPlatform(int platformId, CancellationToken ct = default)
  {
    return await _context.Commands.Where(c => c.PlatformId == platformId).ToListAsync(ct);
  }

  public async Task<Command?> GetCommand(int platformId, int commandId, CancellationToken ct = default)
  {
    return await _context.Commands.FirstOrDefaultAsync(c => c.PlatformId == platformId && c.Id == commandId, ct);
  }

  public async Task CreateCommand(int platformId, Command command, CancellationToken ct = default)
  {
    if (command == null) throw new ArgumentNullException(nameof(command));
    command.PlatformId = platformId;
    await _context.Commands.AddAsync(command, ct);
  }
}