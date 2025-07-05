using CommandService.Models;

namespace CommandService.Data;

public interface ICommandRepo
{
  Task<bool> SaveChanges(CancellationToken ct = default);

  Task<IEnumerable<Platform>> GetAllPlatforms(CancellationToken ct = default);
  Task CreatePlatform(Platform platform, CancellationToken ct = default);
  Task<bool> PlatformExists(int platformId, CancellationToken ct = default);
  Task<bool> ExternalPlatformExists(int externalPlatformId, CancellationToken ct = default);

  Task<IEnumerable<Command>> GetCommandsForPlatform(int platformId, CancellationToken ct = default);
  Task<Command?> GetCommand(int platformId, int commandId, CancellationToken ct = default);
  Task CreateCommand(int platformId, Command command, CancellationToken ct = default);
}