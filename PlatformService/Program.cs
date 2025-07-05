using Microsoft.EntityFrameworkCore;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.SyncDataServices.Grpc;
using PlatformService.SyncDataServices.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
if (builder.Environment.IsDevelopment())
{
  Console.WriteLine("---> Using InMemory Db");
  builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("InMem"));
}
else
{
  Console.WriteLine("---> Using SqlServer Db");
  builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("PlatformsConn")));
}
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddControllers();

builder.Services.AddScoped<IPlatformRepo, PlatformRepo>();
builder.Services.AddHttpClient<ICommandDataClient, HttpCommandDataClient>();
builder.Services.AddScoped<IMessageBusClient, MessageBusClient>();
builder.Services.AddGrpc();

var app = builder.Build();

PrepDb.PrepPopulation(app, app.Environment.IsProduction());

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.MapControllers();
app.MapGrpcService<GrpcPlatformService>();
app.MapGet("/protos/platforms.proto", async context =>
{
  await context.Response.WriteAsync(File.ReadAllText("Protos/platforms.proto"));
});

Console.WriteLine($"CommandService: {builder.Configuration["CommandService"]}");
app.Run();