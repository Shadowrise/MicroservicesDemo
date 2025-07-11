using AutoMapper;
using PlatformService.Dtos;
using PlatformService.Models;

namespace PlatformService.Profiles;

public class PlatformProfile : Profile
{
  public PlatformProfile()
  {
    CreateMap<Platform, PlatformReadDto>();
    CreateMap<PlatformCreateDto, Platform>();
    CreateMap<PlatformReadDto, PlatformPublishDto>();
    CreateMap<Platform, GrpcPlatformModel>()
      .ForMember(dest => dest.PlatformId, opt => opt.MapFrom(src => src.Id));
  }
}