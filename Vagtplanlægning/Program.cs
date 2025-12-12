using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Neo4j.Driver;
using Vagtplanlægning.Authentication;
using Vagtplanlægning.Authentication.Policies;
using Vagtplanlægning.Configurations;
using Vagtplanlægning.Data;
using Vagtplanlægning.Mapping;
using Vagtplanlægning.Repositories;
using Vagtplanlægning.Repositories.MySqlRepository;
using Vagtplanlægning.Services;

var builder = WebApplication.CreateBuilder(args);

// Read which provider we want to use: "MySql", "Mongo", "MySqlWithMongoFallback", "Neo4j"
var providerRaw = builder.Configuration["DatabaseProvider"] ?? "mysqlwithmongofallback";
var provider = providerRaw.Trim().ToLowerInvariant();
Console.WriteLine($"[DB PROVIDER] Raw='{providerRaw}' Normalized='{provider}'");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Common services (independent of provider)
builder.Services.AddScoped<IShiftPlanService, ShiftPlanService>(); 
builder.Services.AddScoped<IShiftExecutionService, ShiftExecutionService>();

// --------------------------------------------------------
// 1) Configure MySQL DbContext, if it is needed
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
// 2) Configure MongoDB, if it is needed
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

    builder.Services.AddSingleton<MongoDbContext>();
    builder.Services.AddScoped<MongoDbContext>();

}

// --------------------------------------------------------
// 3) Provider-specific registration of repositories
// --------------------------------------------------------
switch (provider)
{
    case "mysql":
        builder.Services.AddScoped<IEmployeeRepository, MySqlEmployeeRepository>();
        builder.Services.AddScoped<IRouteRepository, MySqlRouteRepository>();
        builder.Services.AddScoped<IBicycleRepository, MySqlBicycleRepository>();
        builder.Services.AddScoped<IShiftRepository, MySqlShiftRepository>();
        builder.Services.AddScoped<IShiftPlanRepository, MySqlShiftPlanRepository>();
        builder.Services.AddScoped<IUserRepository, MySqlUserRepository>();

        // Monthly hours via MySQL
        builder.Services.AddScoped<IMonthlyHoursReportService, MySqlMonthlyHoursReportService>();
        break;

    case "mongo":
        builder.Services.AddScoped<IEmployeeRepository, MongoEmployeeRepository>();
        builder.Services.AddScoped<IRouteRepository, MongoRouteRepository>();
        builder.Services.AddScoped<IBicycleRepository, MongoBicycleRepository>();
        builder.Services.AddScoped<IShiftRepository, MongoShiftRepository>();
        
        // Monthly hours kun via Mongo
        builder.Services.AddScoped<IMonthlyHoursReportService, MongoMonthlyHoursReportService>();
        break;


    case "mysqlwithmongofallback":
        // USERS
        builder.Services.AddScoped<MySqlUserRepository>();
        // builder.Services.AddScoped<MongoUserRepository>();
        builder.Services.AddScoped<IUserRepository, MySqlUserRepository>();
        
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

        // SHIFT PLANS
        builder.Services.AddScoped<MySqlShiftPlanRepository>();
        builder.Services.AddScoped<MongoShiftPlanRepository>();
        builder.Services.AddScoped<IShiftPlanRepository, FallbackShiftPlanRepository>();

        // SHIFTS – fallback: MySQL primær, Mongo sekundær
        builder.Services.AddScoped<MySqlShiftRepository>();
        builder.Services.AddScoped<MongoShiftRepository>();
        builder.Services.AddScoped<IShiftRepository>(sp =>
        {
            var primary = sp.GetRequiredService<MySqlShiftRepository>();
            var fallback = sp.GetRequiredService<MongoShiftRepository>();
            var logger = sp.GetRequiredService<ILogger<FallbackShiftRepository>>();
            return new FallbackShiftRepository(primary, fallback, logger);
        });

        // Monthly hours: fallback
        builder.Services.AddScoped<MySqlMonthlyHoursReportService>();
        builder.Services.AddScoped<MongoMonthlyHoursReportService>();
        builder.Services.AddScoped<IMonthlyHoursReportService>(sp =>
        {
            var primary = sp.GetRequiredService<MySqlMonthlyHoursReportService>();
            var fallback = sp.GetRequiredService<MongoMonthlyHoursReportService>();
            var logger = sp.GetRequiredService<ILogger<FallbackMonthlyHoursReportService>>();
            return new FallbackMonthlyHoursReportService(primary, fallback, logger);
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


// --------------------------------------------------------
// 4) Allow CORS
// --------------------------------------------------------
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

// --------------------------------------------------------
// 5) Setup Authentication, Authorization, Swagger, AutoMapper
// --------------------------------------------------------
var issuer = builder.Configuration["Issuer"];
var audience = builder.Configuration["Audience"];
var key = builder.Configuration["Key"];

// Use more relaxed rules in test environments
var isTestEnv =
    builder.Environment.IsEnvironment("Test") ||
    builder.Environment.IsEnvironment("Testing") ||
    builder.Environment.IsEnvironment("IntegrationTests");

// In "real" environments we must fail hard on missing configuration
if (!isTestEnv)
{
    if (issuer == null || audience == null || key == null)
    {
        throw new InvalidOperationException("Missing required configuration parameter.");
    }

    if (issuer is null || audience is null || key is null)
    {
        throw new Exception("Missing configuration");
    }
}


// --------------------------------------------------------
// 5) Setup Services / Dependency Injection
// --------------------------------------------------------

// Setup of Services
AuthenticationConfig.Configure(builder.Services, issuer, audience, key);
AuthorizationConfig.Configure(builder.Services);
SwaggerConfig.Configure(builder.Services);

// Authorization
builder.Services.AddSingleton<IAuthorizationHandler, IsUserHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, IsAdminHandler>();

// JWT
builder.Services.AddSingleton<JwtHelper>();

// Mapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// --------------------------------------------------------
// 6) Setup App and run the application
// --------------------------------------------------------
var app = builder.Build();
app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();

public partial class Program { }
