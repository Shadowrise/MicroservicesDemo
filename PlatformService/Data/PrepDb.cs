using Microsoft.EntityFrameworkCore;
using PlatformService.Models;

namespace PlatformService.Data;

public class PrepDb
{
  public static void PrepPopulation(IApplicationBuilder app, bool isProd)
  {
    using (var serviceScope = app.ApplicationServices.CreateScope())
    {
      var logger = serviceScope.ServiceProvider.GetService<ILogger<PrepDb>>()!;
      var context = serviceScope.ServiceProvider.GetService<AppDbContext>()!;
      SeedData(context, logger, isProd);
    }
  }

  private static void SeedData(AppDbContext context, ILogger<PrepDb> logger, bool isProd)
  {
    if (isProd)
    {
      logger.LogInformation("Attempting to apply migrations...");
      try
      {
        context.Database.Migrate();
      }
      catch (Exception ex)
      {
        logger.LogError("Could not run migrations: {ExMessage}", ex.Message);
      }
    }

    if (!context.Platforms.Any())
    {
      logger.LogInformation("Seeding data...");
      context.Platforms.AddRange(
        new Platform() { Name = "Dot Net", Publisher = "Microsoft", Cost = "Free" },
        new Platform() { Name = "SQL Server Express", Publisher = "Microsoft", Cost = "Free" },
        new Platform() { Name = "Kubernetes", Publisher = "Cloud Native Computing Foundation", Cost = "Free" }
      );
      context.SaveChanges();
    }
    else
    {
      logger.LogInformation("We already have data");
    }
  }
}