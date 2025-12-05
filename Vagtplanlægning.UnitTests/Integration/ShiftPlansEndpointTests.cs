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
    public class ShiftPlansEndpointTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ShiftPlansEndpointTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAll_ReturnsOk_AndJsonArray()
        {
            // Act
            var response = await _client.GetAsync("/api/shiftplans");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<List<ShiftPlanSummaryDto>>();
            Assert.NotNull(body);
        }

        [Fact]
        public async Task Generate6Weeks_InvalidBody_ReturnsBadRequest()
        {
            // Mangler StartDate -> skal give 400
            var response = await _client.PostAsJsonAsync("/api/shiftplans/generate-6weeks", new { });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
