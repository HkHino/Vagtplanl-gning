using AutoMapper;
using Vagtplanlægning.Models;
using Vagtplanlægning.DTOs;

namespace Vagtplanlægning.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>();
        }
    }
}