using AutoMapper;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<Employee, EmployeeDto>();
            CreateMap<CreateEmployeeDto, Employee>();
            CreateMap<UpdateEmployeeDto, Employee>();
            CreateMap<Shift, ShiftDto>();
        }
    }
}
