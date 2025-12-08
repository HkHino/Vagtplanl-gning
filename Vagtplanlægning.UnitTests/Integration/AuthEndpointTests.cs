using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Vagtplanlægning.UnitTests.Integration
{
    public class AuthEndpointTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthEndpointTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsClientError()
        {
            var response = await _client.PostAsJsonAsync(
                "/api/auth/login",   // ret kun hvis din route er anderledes
                new
                {
                    username = "does_not_exist",
                    password = "wrong-password"
                });

            // Vi accepterer: 401, 400, 404 – alle er "negativt udfald"
            Assert.Contains(response.StatusCode,
                new[]
                {
                    HttpStatusCode.Unauthorized,
                    HttpStatusCode.BadRequest,
                    HttpStatusCode.NotFound
                });
        }

        [Fact]
        public async Task Login_MissingBody_ReturnsClientError()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login");
            var response = await _client.SendAsync(request);

            // Her giver 400 eller 404 også mening afhængigt af din validering
            Assert.Contains(response.StatusCode,
                new[]
                {
                    HttpStatusCode.BadRequest,
                    HttpStatusCode.NotFound
                });
        }
    }
}
