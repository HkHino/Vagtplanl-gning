using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Vagtplanlægning.UnitTests.Integration
{
    public class ReportsEndpointTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ReportsEndpointTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        // EP + positiv test: gyldige parametre -> 200 OK
        [Fact]
        public async Task MonthlyHours_ValidRequest_ReturnsOk()
        {
            var response = await _client.GetAsync(
                "/api/reports/monthly-hours?employeeId=2&year=2025&month=11");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = (await response.Content.ReadAsStringAsync()).Trim();
            Assert.NotEmpty(body);
            Assert.True(body.StartsWith("[") || body.StartsWith("{"));
        }

        // BVA: month = 13 -> 400
        [Fact]
        public async Task MonthlyHours_InvalidMonthHigh_ReturnsBadRequest()
        {
            var response = await _client.GetAsync(
                "/api/reports/monthly-hours?employeeId=2&year=2025&month=13");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // BVA: month = 0 -> 400
        [Fact]
        public async Task MonthlyHours_MonthZero_ReturnsBadRequest()
        {
            var response = await _client.GetAsync(
                "/api/reports/monthly-hours?employeeId=2&year=2025&month=0");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // BVA: month = -1 -> 400
        [Fact]
        public async Task MonthlyHours_MonthNegative_ReturnsBadRequest()
        {
            var response = await _client.GetAsync(
                "/api/reports/monthly-hours?employeeId=2&year=2025&month=-1");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // BVA: år lige under 2020 -> 400
        [Fact]
        public async Task MonthlyHours_YearBelow2020_ReturnsBadRequest()
        {
            var response = await _client.GetAsync(
                "/api/reports/monthly-hours?employeeId=2&year=2019&month=11");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
