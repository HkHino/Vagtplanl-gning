using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Vagtplanlægning;
using Xunit;


namespace Vagtplanlægning.UnitTests.Integration
{
    public class EmployeesEndpointTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public EmployeesEndpointTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetEmployees_ReturnsOk_AndJsonArray()
        {
            // Hvis controller hedder [Route("api/[controller]")]
            // og klassen hedder EmployeesController -> /api/employees
            var response = await _client.GetAsync("/api/employees");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = (await response.Content.ReadAsStringAsync()).Trim();

            // bare en simpel sanity-check på, at det ligner JSON-array
            Assert.NotEmpty(body);
            Assert.True(body.StartsWith("[") || body.StartsWith("{"));
        }

        [Fact]
        public async Task GetEmployee_InvalidId_ReturnsNotFoundOrBadRequest()
        {
            // prøver et “umuligt” id – afhænger af din controller-logik
            var response = await _client.GetAsync("/api/employees/-1");

            // Nogle laver 404, andre 400 – begge er ok som “negativ test”
            Assert.Contains(response.StatusCode,
                new[] { HttpStatusCode.NotFound, HttpStatusCode.BadRequest });
        }
    }
}
