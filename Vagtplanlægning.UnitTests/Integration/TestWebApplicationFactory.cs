using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using Vagtplanlægning;

namespace Vagtplanlægning.UnitTests.Integration
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        public TestWebApplicationFactory()
        {
            // === Konfiguration til Program.cs ===

            // DatabaseProvider læses helt i toppen af Program.cs
            Environment.SetEnvironmentVariable("DatabaseProvider", "mysqlwithmongofallback");

            // MySQL-connectionstring – må gerne være fake, fallback tager over
            Environment.SetEnvironmentVariable(
                "ConnectionStrings__MySql2",
                "Server=127.0.0.1;Port=3306;Database=cykelBudDB;User=api;Password=1234;");

            // Mongo – her kan du sætte en ægte, men den her duer til local default
            Environment.SetEnvironmentVariable(
                "ConnectionStrings__Mongo",
                "mongodb+srv://mpfugl_db_user:Wy0ngZaAEtoUZLbA@vagtplanlaegning.bceb2r2.mongodb.net/vagtplanlaegning?retryWrites=true&w=majority");

            // JWT – Program.cs crasher hvis de er null
            Environment.SetEnvironmentVariable("Issuer", "TestIssuer");
            Environment.SetEnvironmentVariable("Audience", "TestAudience");
            Environment.SetEnvironmentVariable("Key", "THIS IS A VERY SECRET TEST KEY 1234567890");
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
        }
    }
}
