using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Vagtplanlægning.DTOs;
using Xunit;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace Vagtplanlægning.UnitTests.Integration
{
    public class ReportsEndpointTests :
        IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ReportsEndpointTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task MonthlyHours_ValidRequest_ReturnsOkAndJson()
        {
            // Arrange: brug en kombination, som du ved giver data (employeeId=2, year=2025, month=11)
            var url = "/api/reports/monthly-hours?employeeId=2&year=2025&month=11";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var rows = await response.Content.ReadFromJsonAsync<IEnumerable<MonthlyHoursRow>>();
            Assert.NotNull(rows);
            Assert.NotEmpty(rows);

            // Her kan du også logge / måle execution time, hvis du vil.
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

}
