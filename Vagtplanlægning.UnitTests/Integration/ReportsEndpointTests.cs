using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Vagtplanlægning.DTOs;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Vagtplanlægning;
using Vagtplanlægning.UnitTests.Integration;

namespace Vagtplanlægning.UnitTests.Integration
{
    public class ReportsEndpointTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ReportsEndpointTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task MonthlyHours_ValidRequest_ReturnsOkAndJson()
        {
            var response = await _client.GetAsync(
                "/api/reports/monthly-hours?employeeId=2&year=2025&month=11");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<List<MonthlyHoursRow>>();
            Assert.NotNull(body);
        }

        [Fact]
        public async Task MonthlyHours_InvalidMonth_ReturnsBadRequest()
        {
            var response = await _client.GetAsync(
                "/api/reports/monthly-hours?employeeId=2&year=2025&month=13");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
