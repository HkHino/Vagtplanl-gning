/*using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Tests.Integration;
using Xunit;


namespace Vagtplanlægning.UnitTests.Integration
{
    public class ReportsEndpointTests :
        IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ReportsEndpointTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task MonthlyHours_ValidRequest_ReturnsOkAndJson()
        {
            // Arrange: brug en kombination, som vi ved giver data (employeeId=2, year=2025, month=11)
            var url = "/api/reports/monthly-hours?employeeId=2&year=2025&month=11";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var rows = await response.Content.ReadFromJsonAsync<IEnumerable<MonthlyHoursRow>>();
            Assert.NotNull(rows);
            Assert.NotEmpty(rows);

            //todo Her kan vi også logge / måle execution time, hvis vi vil.
        }

        [Fact]
        public async Task MonthlyHours_InvalidMonth_ReturnsBadRequest()
        {
            var url = "/api/reports/monthly-hours?employeeId=2&year=2025&month=0";

            var response = await _client.GetAsync(url);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
    public partial class Program { }

}*/
