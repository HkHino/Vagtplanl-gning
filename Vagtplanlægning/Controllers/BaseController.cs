using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vagtplanlægning.Data;

namespace Vagtplanlægning.Controllers;



public class BaseController : ControllerBase
{
    protected readonly IMapper _mapper;
    protected readonly AppDbContext _db;

    public BaseController(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }
    
}