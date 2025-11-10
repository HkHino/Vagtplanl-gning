using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Neo4j.Driver;
using Vagtplanlægning.Data;
using Vagtplanlægning.Mapping;
using Vagtplanlægning.Repositories;
using Vagtplanlægning.Services;



//todo : username for Mongodb : mpfugl_db_user
//todo : password for Mongodb: Wy0ngZaAEtoUZLbA

var builder = WebApplication.CreateBuilder(args);

// read which db to use: "MySql", "Mongo", "Neo4j"
var provider = builder.Configuration["DatabaseProvider"] ?? "Mongo"; //Mongo , MySql, Neo4j

// Always add AutoMapper, controllers, swagger
builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
builder.Services.AddScoped<IShiftPlanService, ShiftPlanService>();
builder.Services.AddScoped<IShiftExecutionService, ShiftExecutionService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (provider == "MySql")
{
    var cs = builder.Configuration.GetConnectionString("MySql");
    builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseMySql(cs, Microsoft.EntityFrameworkCore.ServerVersion.AutoDetect(cs)));
   

    builder.Services.AddScoped<IEmployeeRepository, MySqlEmployeeRepository>();
}
else if (provider == "Mongo")
{
    var cs = builder.Configuration.GetConnectionString("Mongo");
    var mongoUrl = new MongoUrl(cs);

    builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoUrl));
    builder.Services.AddSingleton<IMongoDatabase>(sp =>
    {
        var client = sp.GetRequiredService<IMongoClient>();
        return client.GetDatabase(mongoUrl.DatabaseName ?? "vagtplan");
    });

    builder.Services.AddScoped<IEmployeeRepository, MongoEmployeeRepository>();
}
else if (provider == "Neo4j")
{
    var uri = builder.Configuration["Neo4j:Uri"];      // e.g. "bolt://localhost:7687"
    var user = builder.Configuration["Neo4j:User"];    // e.g. "neo4j"
    var pass = builder.Configuration["Neo4j:Password"];

    builder.Services.AddSingleton<IDriver>(_ =>
        GraphDatabase.Driver(uri, AuthTokens.Basic(user, pass)));

    builder.Services.AddScoped<IEmployeeRepository, Neo4jEmployeeRepository>();
}

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();


