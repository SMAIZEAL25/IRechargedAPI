using AutoMapper;
using IRecharge_API.DTO;
using IRecharge_API.Entities;
using System.Net.NetworkInformation;

namespace IRecharge_API.MapConfig
{
    public class MapConfig : Profile 
    {
        public MapConfig()
        {
            CreateMap<RegisterUserDTO, User>();
            CreateMap<User, LoginDto>().ReverseMap();
        }
    }
}
