using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Vagtplanlægning.UnitTests.Integration
{
    public class RoutesEndpointTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public RoutesEndpointTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetRoutes_ReturnsOkOrNotFound()
        {
            // GET /api/routes
            var response = await _client.GetAsync("/api/routes");

            // Positivt scenarie: 200 OK
            // Negativt/scenarie uden data: 404 NotFound
            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        [Fact]
        public async Task GetRoute_InvalidId_ReturnsNotFoundOrBadRequest()
        {
            // Et “umuligt” id – afhænger af din controllerlogik
            var response = await _client.GetAsync("/api/routes/-1");

            // Typisk enten 404 (ingen sådan route) eller 400 (invalid id)
            Assert.True(
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.BadRequest
            );
        }
    }
}
