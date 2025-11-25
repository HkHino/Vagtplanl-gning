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

// Læs hvilken provider vi vil bruge: "MySql", "Mongo", "MySqlWithMongoFallback", "Neo4j"
var providerRaw = builder.Configuration["DatabaseProvider"] ?? "MySql";
var provider = providerRaw.Trim().ToLowerInvariant();
Console.WriteLine($"[DB PROVIDER] Raw='{providerRaw}' Normalized='{provider}'");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Fælles services
builder.Services.AddScoped<IShiftPlanService, ShiftPlanService>();

// --------------------------------------------------------
// 1) Konfigurer MySQL DbContext, hvis vi har brug for den
// --------------------------------------------------------
if (provider == "mysql" || provider == "mysqlwithmongofallback" || provider == "neo4j")
{
    var cs = builder.Configuration.GetConnectionString("MySql2");
    builder.Services.AddDbContext<AppDbContext>(opts =>
        opts.UseMySql(
            cs,
            new MySqlServerVersion(new Version(8, 0, 43))
        )
    );

}

// --------------------------------------------------------
// 2) Konfigurer Mongo, hvis vi har brug for det
// --------------------------------------------------------
if (provider == "mongo" || provider == "mysqlwithmongofallback")
{
    var cs = builder.Configuration.GetConnectionString("Mongo");
    var mongoUrl = new MongoUrl(cs);

    builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoUrl));
    builder.Services.AddSingleton<IMongoDatabase>(sp =>
    {
        var client = sp.GetRequiredService<IMongoClient>();
        return client.GetDatabase(mongoUrl.DatabaseName ?? "vagtplan");
    });
}

// --------------------------------------------------------
// 3) Provider-specifik registrering af repositories
// --------------------------------------------------------
switch (provider)
{
    case "mysql":
        builder.Services.AddScoped<IEmployeeRepository, MySqlEmployeeRepository>();
        builder.Services.AddScoped<IRouteRepository, MySqlRouteRepository>();
        builder.Services.AddScoped<IShiftRepository, MySqlShiftRepository>();
        builder.Services.AddScoped<IShiftPlanRepository, MySqlShiftPlanRepository>();
        break;

    case "mongo":
        builder.Services.AddScoped<IEmployeeRepository, MongoEmployeeRepository>();
        break;

    case "mysqlwithmongofallback":
        builder.Services.AddScoped<MySqlEmployeeRepository>();
        builder.Services.AddScoped<MongoEmployeeRepository>();

        builder.Services.AddScoped<IEmployeeRepository, EmployeeRepositoryFallback>();

        builder.Services.AddScoped<IRouteRepository, MySqlRouteRepository>();
        builder.Services.AddScoped<IShiftRepository, MySqlShiftRepository>();
        builder.Services.AddScoped<IShiftPlanRepository, MySqlShiftPlanRepository>();
        break;

    case "neo4j":
        var uri = builder.Configuration["Neo4j:Uri"];
        var user = builder.Configuration["Neo4j:User"];
        var pass = builder.Configuration["Neo4j:Password"];

        builder.Services.AddSingleton<IDriver>(_ =>
            GraphDatabase.Driver(uri, AuthTokens.Basic(user, pass)));

        builder.Services.AddScoped<IEmployeeRepository, Neo4jEmployeeRepository>();
        builder.Services.AddScoped<IRouteRepository, MySqlRouteRepository>();
        builder.Services.AddScoped<IShiftRepository, MySqlShiftRepository>();
        builder.Services.AddScoped<IShiftPlanRepository, MySqlShiftPlanRepository>();
        break;

    default:
        throw new InvalidOperationException($"Unknown DatabaseProvider value '{providerRaw}'.");
}

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
