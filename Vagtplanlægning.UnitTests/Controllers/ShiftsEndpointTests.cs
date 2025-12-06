using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Vagtplanlægning.DTOs;
using Xunit;

namespace Vagtplanlægning.UnitTests.Integration
{
    public class ShiftsEndpointTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ShiftsEndpointTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetShifts_ReturnsOkOrNotFound()
        {
            var response = await _client.GetAsync("/api/shifts");

            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        [Fact]
        public async Task GetShift_InvalidId_ReturnsNotFoundOrBadRequest()
        {
            var response = await _client.GetAsync("/api/shifts/999999");

            Assert.True(
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.BadRequest);
        }
    }
}
