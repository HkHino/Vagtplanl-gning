using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Vagtplanlægning.UnitTests.Integration
{
    public class BicyclesEndpointTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public BicyclesEndpointTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetBicycles_ReturnsOkOrNotFound()
        {
            var response = await _client.GetAsync("/api/bicycles");

            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        [Fact]
        public async Task GetBicycle_InvalidId_ReturnsNotFoundOrBadRequest()
        {
            var response = await _client.GetAsync("/api/bicycles/-1");

            Assert.True(
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.BadRequest
            );
        }
    }
}
