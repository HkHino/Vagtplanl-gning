using AutoMapper;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Models;

public class EmployeeProfile : Profile
{
    /// <summary>
    /// AutoMapper profile for mapping between employee domain entities and DTOs.
    ///
    /// Mappings:
    /// - <see cref="Employee"/> → <see cref="EmployeeDto"/>
    /// - <see cref="CreateEmployeeDto"/> → <see cref="Employee"/>
    /// - <see cref="UpdateEmployeeDto"/> → <see cref="Employee"/>
    /// </summary>
    public EmployeeProfile()
    {
        CreateMap<Employee, EmployeeDto>();
        CreateMap<CreateEmployeeDto, Employee>();
        CreateMap<UpdateEmployeeDto, Employee>();
    }
}
