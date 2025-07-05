using PlatformService.Models;
using System.Threading;
using System.Threading.Tasks;

namespace PlatformService.Data;

public interface IPlatformRepo
{
  Task<bool> SaveChanges(CancellationToken ct = default);
  Task<IEnumerable<Platform>> GetAllPlatforms(CancellationToken ct = default);
  Task<Platform?> GetPlatformById(int id, CancellationToken ct = default);
  Task CreatePlatform(Platform platform, CancellationToken ct = default);
}