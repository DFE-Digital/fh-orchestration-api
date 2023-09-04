using AutoMapper;
using StackExchange.Redis;

namespace FamilyHubs.Orchestration.Core;

public class AutoMappingProfiles : Profile
{
    public AutoMappingProfiles()
    {
        CreateMap<Role, Role>().ReverseMap();
    }
}
