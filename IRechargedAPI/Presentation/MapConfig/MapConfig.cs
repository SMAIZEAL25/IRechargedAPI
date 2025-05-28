using AutoMapper;
using IRechargedAPI.Domian.Entities;
using IRechargedAPI.Presentation.DTO;
using System.Net.NetworkInformation;

namespace IRechargedAPI.Presentation.MapConfig
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
