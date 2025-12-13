using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vagtplanlægning.Data;

namespace Vagtplanlægning.Controllers;

public class BaseController : ControllerBase
{
    protected readonly IMapper _mapper;

    public BaseController(IMapper mapper)
    {
        _mapper = mapper;
    }
    
}