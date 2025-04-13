using AutoMapper;
using IRecharge_API.DTO;
using IRecharge_API.Entities;

namespace IRecharge_API.MapConfig
{
    public class MapConfig : Profile 
    {
        public MapConfig()
        {
            CreateMap<User, RegisterUserDTO>().ReverseMap();
            CreateMap<User, LoginDto>().ReverseMap();
        }
    }
}
