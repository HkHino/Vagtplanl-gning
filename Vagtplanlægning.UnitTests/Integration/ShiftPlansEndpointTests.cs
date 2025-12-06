using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Vagtplanlægning.DTOs;
using Xunit;

namespace Vagtplanlægning.UnitTests.Integration
{
    public class ShiftPlansEndpointTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ShiftPlansEndpointTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAll_ReturnsOk_AndJsonArray()
        {
            var response = await _client.GetAsync("/api/shiftplans");

            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                 response.StatusCode == HttpStatusCode.NotFound
            );


            var plans = await response.Content.ReadFromJsonAsync<List<ShiftPlanSummaryDto>>();
            Assert.NotNull(plans);
        }

        [Fact]
        public async Task Generate6Weeks_InvalidBody_ReturnsBadRequest()
        {
            var response = await _client.PostAsJsonAsync(
                "/api/shiftplans/generate-6weeks",
                new { startDate = "0001-01-01T00:00:00" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
