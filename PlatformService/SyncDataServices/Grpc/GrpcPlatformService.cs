using AutoMapper;
using Grpc.Core;
using PlatformService.Data;
using static PlatformService.PlatformService;

namespace PlatformService.SyncDataServices.Grpc;

public class GrpcPlatformService : PlatformServiceBase
{
  private readonly IPlatformRepo _repo;
  private readonly IMapper _mapper;

  public GrpcPlatformService(IPlatformRepo repo, IMapper mapper)
  {
    _repo = repo;
    _mapper = mapper;
  }

  public override async Task<PlatformResponse> GetAllPlatforms(GetAllRequest request, ServerCallContext context)
  {
    var response = new PlatformResponse();
    var platforms = await _repo.GetAllPlatforms();
    response.Platforms.AddRange(_mapper.Map<IEnumerable<GrpcPlatformModel>>(platforms));
    return response;
  }
}
