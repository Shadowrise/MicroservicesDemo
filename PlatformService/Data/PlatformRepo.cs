using PlatformService.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace PlatformService.Data;

public class PlatformRepo : IPlatformRepo
{
  private readonly AppDbContext _context;

  public PlatformRepo(AppDbContext context)
  {
    _context = context;
  }

  public async Task CreatePlatform(Platform platform, CancellationToken ct = default)
  {
    if (platform == null)
    {
      throw new ArgumentNullException(nameof(platform));
    }
    await _context.Platforms.AddAsync(platform, ct);
  }

  public async Task<IEnumerable<Platform>> GetAllPlatforms(CancellationToken ct = default)
  {
    return await _context.Platforms.ToListAsync(ct);
  }

  public async Task<Platform?> GetPlatformById(int id, CancellationToken ct = default)
  {
    return await _context.Platforms.FirstOrDefaultAsync(p => p.Id == id, ct);
  }

  public async Task<bool> SaveChanges(CancellationToken ct = default)
  {
    return await _context.SaveChangesAsync(ct) >= 0;
  }
}