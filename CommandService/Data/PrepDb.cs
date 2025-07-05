using CommandService.Models;
using CommandService.SyncDataServices.Grpc;

namespace CommandService.Data;

public static class PrepDb
{
  public static async Task PrepPopulation(IApplicationBuilder app)
  {
    using (var serviceScope = app.ApplicationServices.CreateScope())
    {
      var grpcClient = serviceScope.ServiceProvider.GetService<IPlatformDataClient>()!;
      var platforms = await grpcClient.ReturnAllPlatforms();
      await SeedData(serviceScope.ServiceProvider.GetService<ICommandRepo>()!, platforms);
    }
  }

  private static async Task SeedData(ICommandRepo repo, IEnumerable<Platform> platforms)
  {
    Console.WriteLine("--> Seeding new platforms...");
    foreach (var platform in platforms)
    {
      if (await repo.ExternalPlatformExists(platform.ExternalID))
      {
        Console.WriteLine($"--> Platform with ID {platform.ExternalID} already exists");
        continue;
      }
      Console.WriteLine($"--> Adding platform {platform.Name} to the database");
      await repo.CreatePlatform(platform);
    }
    await repo.SaveChanges();
  }
}