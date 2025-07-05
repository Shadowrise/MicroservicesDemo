using AutoMapper;
using CommandService.Models;
using Grpc.Net.Client;
using PlatformService;
using static PlatformService.PlatformService;

namespace CommandService.SyncDataServices.Grpc;

public class PlatformDataClient : IPlatformDataClient
{
  private readonly IConfiguration _configuration;
  private readonly IMapper _mapper;

  public PlatformDataClient(IConfiguration configuration, IMapper mapper)
  {
    _configuration = configuration;
    _mapper = mapper;
  }

  public async Task<IEnumerable<Platform>> ReturnAllPlatforms()
  {
    Console.WriteLine($"--> Calling GRPC Service {_configuration["GrpcPlatform"]}");

    var channel = GrpcChannel.ForAddress(_configuration["GrpcPlatform"]!);
    var client = new PlatformServiceClient(channel);
    var request = new GetAllRequest();
    try
    {
      var response = await client.GetAllPlatformsAsync(request);
      return _mapper.Map<IEnumerable<Platform>>(response.Platforms);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"--> Could not call GRPC Server {ex.Message}");
      throw;
    }
  }
}