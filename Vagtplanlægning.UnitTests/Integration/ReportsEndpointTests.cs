using Microsoft.AspNetCore.Mvc.Testing;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Vagtplanlægning;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.UnitTests.Integration;
using Xunit;

namespace Vagtplanlægning.Tests.Integration
{
    public class ReportsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ReportsEndpointTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task MonthlyHours_ValidRequest_ReturnsOkAndJson()
        {
            // Tilpas parametre efter noget du VED findes
            var response = await _client.GetAsync("/api/reports/monthly-hours?employeeId=2&year=2025&month=11");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<List<MonthlyHoursRow>>();
            Assert.NotNull(body);
        }

        [Fact]
        public async Task MonthlyHours_InvalidMonth_ReturnsBadRequest()
        {
            var response = await _client.GetAsync("/api/reports/monthly-hours?employeeId=2&year=2025&month=13");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
