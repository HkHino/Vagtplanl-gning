using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
//using MongoDB.Driver;
using Vagtplanlægning.Mapping;
using Vagtplanlægning.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Connection string navn: DefaultConnection i appsettings.json
var cs = builder.Configuration.GetConnectionString("DefaultConnection");

// DbContext (Pomelo for MySQL/MariaDB)
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseMySql(cs, ServerVersion.AutoDetect(cs)));

// AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

// Repositories
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

builder.Services.AddScoped<IShiftRepository, MySqlShiftRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
