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
            //CreateMap<RegisterUserDTO, User>();
            //     //.ForMember(dest => dest.Password, opt => opt.Ignore());

            CreateMap<User, LoginDto>().ReverseMap();
        }
    }
}
