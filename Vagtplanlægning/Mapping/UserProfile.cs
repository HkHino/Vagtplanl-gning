using AutoMapper;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Vagtplanlægning.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>();
        }
    }
}
