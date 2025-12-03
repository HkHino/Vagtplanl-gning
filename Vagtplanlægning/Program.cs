using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Neo4j.Driver;
using Vagtplanlægning.Data;
using Vagtplanlægning.Mapping;
using Vagtplanlægning.Repositories;
using Vagtplanlægning.Services;

var builder = WebApplication.CreateBuilder(args);

// Læs hvilken provider vi vil bruge: "MySql", "Mongo", "MySqlWithMongoFallback", "Neo4j"
var providerRaw = builder.Configuration["DatabaseProvider"] ?? "MySql";
var provider = providerRaw.Trim().ToLowerInvariant();
Console.WriteLine($"[DB PROVIDER] Raw='{providerRaw}' Normalized='{provider}'");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Fælles services (uafhængige af provider)
builder.Services.AddScoped<IShiftPlanService, ShiftPlanService>();      // <--- beholdt
builder.Services.AddScoped<IShiftExecutionService, ShiftExecutionService>(); // <--- beholdt

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
        return client.GetDatabase(mongoUrl.DatabaseName ?? "vagtplanlaegning");
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
        builder.Services.AddScoped<IBicycleRepository, MySqlBicycleRepository>();
        builder.Services.AddScoped<IShiftRepository, MySqlShiftRepository>();
        builder.Services.AddScoped<IShiftPlanRepository, MySqlShiftPlanRepository>();
        break;

    case "mongo":
        builder.Services.AddScoped<IEmployeeRepository, MongoEmployeeRepository>();
        builder.Services.AddScoped<IRouteRepository, MongoRouteRepository>();
        builder.Services.AddScoped<IBicycleRepository, MongoBicycleRepository>();
        builder.Services.AddScoped<IShiftRepository, MongoShiftRepository>();
        //lave Mongo-variant for ShiftPlans, registreres her.
        break;

    case "mysqlwithmongofallback":
        // EMPLOYEES
        builder.Services.AddScoped<MySqlEmployeeRepository>();
        builder.Services.AddScoped<MongoEmployeeRepository>();
        builder.Services.AddScoped<IEmployeeRepository, EmployeeRepositoryFallback>();

        // ROUTES
        builder.Services.AddScoped<MySqlRouteRepository>();
        builder.Services.AddScoped<MongoRouteRepository>();
        builder.Services.AddScoped<IRouteRepository, RouteRepositoryFallback>();

        // BICYCLES
        builder.Services.AddScoped<MySqlBicycleRepository>();
        builder.Services.AddScoped<MongoBicycleRepository>();
        builder.Services.AddScoped<IBicycleRepository, BicycleRepositoryFallback>();

        // SHIFT PLANS (kun MySQL snapshot – som du allerede har)
        builder.Services.AddScoped<IShiftPlanRepository, MySqlShiftPlanRepository>();

        // SHIFTS – fallback: MySQL primær, Mongo sekundær
        builder.Services.AddScoped<MySqlShiftRepository>();      // <--- ny her
        builder.Services.AddScoped<MongoShiftRepository>();      // <--- ny her
        builder.Services.AddScoped<IShiftRepository>(sp =>
        {
            var primary = sp.GetRequiredService<MySqlShiftRepository>();
            var fallback = sp.GetRequiredService<MongoShiftRepository>();
            var logger = sp.GetRequiredService<ILogger<FallbackShiftRepository>>();
            return new FallbackShiftRepository(primary, fallback, logger);
        });
        break;

    case "neo4j":
        var uri = builder.Configuration["Neo4j:Uri"];
        var user = builder.Configuration["Neo4j:User"];
        var pass = builder.Configuration["Neo4j:Password"];

        builder.Services.AddSingleton<IDriver>(_ =>
            GraphDatabase.Driver(uri, AuthTokens.Basic(user, pass)));

        builder.Services.AddScoped<IEmployeeRepository, Neo4jEmployeeRepository>();
        builder.Services.AddScoped<IRouteRepository, MySqlRouteRepository>();
        builder.Services.AddScoped<IBicycleRepository, MySqlBicycleRepository>();
        builder.Services.AddScoped<IShiftRepository, MySqlShiftRepository>();
        builder.Services.AddScoped<IShiftPlanRepository, MySqlShiftPlanRepository>();
        break;

    default:
        throw new InvalidOperationException($"Unknown DatabaseProvider value '{providerRaw}'.");
}

//-----------------allow cors -------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
