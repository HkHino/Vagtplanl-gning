using AutoMapper;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Employee, EmployeeDto>();
            CreateMap<CreateEmployeeDto, Employee>();
            CreateMap<UpdateEmployeeDto, Employee>();
        }
    }
}
